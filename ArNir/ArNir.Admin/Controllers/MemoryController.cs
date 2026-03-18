using ArNir.Admin.Models;
using ArNir.Core.Entities;
using ArNir.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ArNir.Admin.Controllers;

/// <summary>
/// Admin panel for browsing and managing persisted chat-memory sessions.
/// Operators can view per-session transcripts, delete individual sessions,
/// and purge sessions older than a configurable number of days.
/// </summary>
[Authorize]
public class MemoryController : Controller
{
    private readonly IDbContextFactory<ArNirDbContext> _dbFactory;
    private readonly ILogger<MemoryController> _logger;

    /// <summary>Initialises the controller.</summary>
    public MemoryController(
        IDbContextFactory<ArNirDbContext> dbFactory,
        ILogger<MemoryController> logger)
    {
        _dbFactory = dbFactory;
        _logger    = logger;
    }

    // GET /Memory
    /// <summary>Lists all sessions grouped by SessionId, showing message count and last activity.</summary>
    public async Task<IActionResult> Index()
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        var sessions = await db.ChatMemories
            .AsNoTracking()
            .GroupBy(m => m.SessionId)
            .Select(g => new MemorySessionViewModel
            {
                SessionId     = g.Key,
                MessageCount  = g.Count(),
                LastActivity  = g.Max(m => m.CreatedAt)
            })
            .OrderByDescending(s => s.LastActivity)
            .ToListAsync();

        return View(sessions);
    }

    // GET /Memory/Session/{sessionId}
    /// <summary>Shows the full chat transcript for a single session.</summary>
    public async Task<IActionResult> Session(string sessionId)
    {
        if (string.IsNullOrWhiteSpace(sessionId))
            return BadRequest();

        await using var db = await _dbFactory.CreateDbContextAsync();
        var messages = await db.ChatMemories
            .AsNoTracking()
            .Where(m => m.SessionId == sessionId)
            .OrderBy(m => m.CreatedAt)
            .ToListAsync();

        if (!messages.Any())
            return NotFound();

        ViewBag.SessionId = sessionId;
        return View(messages);
    }

    // POST /Memory/DeleteSession/{sessionId}
    /// <summary>Deletes all chat-memory rows for the specified session.</summary>
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteSession(string sessionId)
    {
        if (string.IsNullOrWhiteSpace(sessionId))
        {
            TempData["Error"] = "Invalid session ID.";
            return RedirectToAction(nameof(Index));
        }

        await using var db = await _dbFactory.CreateDbContextAsync();
        var rows = db.ChatMemories.Where(m => m.SessionId == sessionId);
        int count = await rows.CountAsync();
        db.ChatMemories.RemoveRange(rows);
        await db.SaveChangesAsync();

        _logger.LogInformation("Deleted {Count} memory rows for session {SessionId}.", count, sessionId);
        TempData["Success"] = $"Session '{sessionId[..Math.Min(20, sessionId.Length)]}' deleted ({count} messages).";
        return RedirectToAction(nameof(Index));
    }

    // POST /Memory/PurgeOld
    /// <summary>Deletes all sessions whose last activity is older than <paramref name="daysOld"/> days.</summary>
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> PurgeOld(int daysOld = 30)
    {
        if (daysOld < 1) daysOld = 1;

        var cutoff = DateTime.UtcNow.AddDays(-daysOld);

        await using var db = await _dbFactory.CreateDbContextAsync();

        // Find session IDs whose max CreatedAt is before the cutoff
        var oldSessionIds = await db.ChatMemories
            .GroupBy(m => m.SessionId)
            .Where(g => g.Max(m => m.CreatedAt) < cutoff)
            .Select(g => g.Key)
            .ToListAsync();

        if (oldSessionIds.Count == 0)
        {
            TempData["Success"] = $"No sessions older than {daysOld} day(s) found.";
            return RedirectToAction(nameof(Index));
        }

        var toDelete = db.ChatMemories.Where(m => oldSessionIds.Contains(m.SessionId));
        int rowCount = await toDelete.CountAsync();
        db.ChatMemories.RemoveRange(toDelete);
        await db.SaveChangesAsync();

        _logger.LogInformation("Purged {Sessions} sessions ({Rows} rows) older than {Days} days.",
            oldSessionIds.Count, rowCount, daysOld);
        TempData["Success"] = $"Purged {oldSessionIds.Count} session(s) ({rowCount} messages) older than {daysOld} day(s).";
        return RedirectToAction(nameof(Index));
    }
}
