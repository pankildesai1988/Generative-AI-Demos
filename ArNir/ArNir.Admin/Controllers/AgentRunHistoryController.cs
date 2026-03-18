using ArNir.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ArNir.Admin.Controllers;

/// <summary>
/// Admin panel for viewing persisted <c>AgentRunLogs</c>.
/// Consulting clients can inspect the PlannerAgent's step-by-step execution plans for
/// any historical session, which is useful for debugging and demonstrating AI reasoning.
/// </summary>
public class AgentRunHistoryController : Controller
{
    private readonly IDbContextFactory<ArNirDbContext> _dbFactory;
    private readonly ILogger<AgentRunHistoryController> _logger;

    /// <summary>Initialises the controller.</summary>
    public AgentRunHistoryController(
        IDbContextFactory<ArNirDbContext> dbFactory,
        ILogger<AgentRunHistoryController> logger)
    {
        _dbFactory = dbFactory;
        _logger    = logger;
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

        return View(logs.Select(l => new ArNir.Core.Entities.AgentRunLog
        {
            Id           = l.Id,
            SessionId    = l.SessionId,
            OriginalQuery = l.OriginalQuery,
            Status       = l.Status,
            CreatedAt    = l.CreatedAt,
            CompletedAt  = l.CompletedAt,
            PlanJson     = string.Empty   // not loaded in list view — see Detail
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
            var doc    = System.Text.Json.JsonDocument.Parse(log.PlanJson);
            log.PlanJson = System.Text.Json.JsonSerializer.Serialize(doc, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
        }
        catch { /* leave raw if not valid JSON */ }

        return View(log);
    }
}
