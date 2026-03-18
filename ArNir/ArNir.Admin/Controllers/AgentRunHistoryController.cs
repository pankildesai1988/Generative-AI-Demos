using ArNir.Agents.Interfaces;
using ArNir.Core.Entities;
using ArNir.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace ArNir.Admin.Controllers;

/// <summary>
/// Admin panel for viewing persisted <c>AgentRunLogs</c> and manually triggering new agent runs.
/// Consulting clients can inspect the PlannerAgent's step-by-step execution plans for
/// any historical session, which is useful for debugging and demonstrating AI reasoning.
/// </summary>
[Authorize]
public class AgentRunHistoryController : Controller
{
    private readonly IDbContextFactory<ArNirDbContext> _dbFactory;
    private readonly IPlannerAgent _plannerAgent;
    private readonly ILogger<AgentRunHistoryController> _logger;

    /// <summary>Initialises the controller.</summary>
    public AgentRunHistoryController(
        IDbContextFactory<ArNirDbContext> dbFactory,
        IPlannerAgent plannerAgent,
        ILogger<AgentRunHistoryController> logger)
    {
        _dbFactory     = dbFactory;
        _plannerAgent  = plannerAgent;
        _logger        = logger;
    }

    // GET /AgentRunHistory
    /// <summary>Lists all agent run logs (newest first).</summary>
    public async Task<IActionResult> Index()
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        var logs = await db.AgentRunLogs
            .AsNoTracking()
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new
            {
                x.Id,
                x.SessionId,
                x.OriginalQuery,
                x.Status,
                x.CreatedAt,
                x.CompletedAt
            })
            .ToListAsync();

        return View(logs.Select(l => new AgentRunLog
        {
            Id            = l.Id,
            SessionId     = l.SessionId,
            OriginalQuery = l.OriginalQuery,
            Status        = l.Status,
            CreatedAt     = l.CreatedAt,
            CompletedAt   = l.CompletedAt,
            PlanJson      = string.Empty   // not loaded in list view — see Detail
        }).ToList());
    }

    // GET /AgentRunHistory/Detail/{id}
    /// <summary>Shows the full PlanJson for a single agent run.</summary>
    public async Task<IActionResult> Detail(Guid id)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        var log = await db.AgentRunLogs.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        if (log == null) return NotFound();

        // Pretty-print JSON for display
        try
        {
            var doc = JsonDocument.Parse(log.PlanJson);
            log.PlanJson = JsonSerializer.Serialize(doc, new JsonSerializerOptions { WriteIndented = true });
        }
        catch { /* leave raw if not valid JSON */ }

        return View(log);
    }

    // GET /AgentRunHistory/TriggerRun
    /// <summary>Renders the form for manually triggering a new agent run.</summary>
    public IActionResult TriggerRun() => View();

    // POST /AgentRunHistory/TriggerRun
    /// <summary>
    /// Triggers a new agent run with the supplied query.
    /// Creates a plan, executes it, and persists the result as an <see cref="AgentRunLog"/>.
    /// </summary>
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> TriggerRun(string query, string? sessionId)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            ModelState.AddModelError("query", "Query is required.");
            return View();
        }

        var sid = string.IsNullOrWhiteSpace(sessionId) ? Guid.NewGuid().ToString() : sessionId;

        var log = new AgentRunLog
        {
            Id            = Guid.NewGuid(),
            SessionId     = sid,
            OriginalQuery = query,
            Status        = "Running",
            CreatedAt     = DateTime.UtcNow
        };

        try
        {
            var plan = await _plannerAgent.CreatePlanAsync(sid, query);
            var executed = await _plannerAgent.ExecutePlanAsync(plan);

            log.PlanJson    = JsonSerializer.Serialize(executed);
            log.Status      = executed.Status.ToString();
            log.CompletedAt = DateTime.UtcNow;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Agent run failed for query '{Query}'.", query);
            log.Status      = "Failed";
            log.PlanJson    = JsonSerializer.Serialize(new { error = ex.Message });
            log.CompletedAt = DateTime.UtcNow;
        }

        await using var db = await _dbFactory.CreateDbContextAsync();
        db.AgentRunLogs.Add(log);
        await db.SaveChangesAsync();

        _logger.LogInformation("Agent run triggered: id={Id}, status={Status}.", log.Id, log.Status);
        TempData["Success"] = "Agent run triggered successfully.";
        return RedirectToAction(nameof(Index));
    }
}
