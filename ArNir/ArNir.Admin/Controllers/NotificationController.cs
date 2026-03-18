using ArNir.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ArNir.Admin.Controllers;

/// <summary>
/// Provides a polling endpoint for SLA breach notifications displayed in the navbar bell icon.
/// </summary>
[Authorize]
public class NotificationController : Controller
{
    private readonly IDbContextFactory<ArNirDbContext> _dbFactory;
    private readonly ILogger<NotificationController> _logger;

    /// <summary>Initialises the controller with required services.</summary>
    public NotificationController(
        IDbContextFactory<ArNirDbContext> dbFactory,
        ILogger<NotificationController> logger)
    {
        _dbFactory = dbFactory;
        _logger    = logger;
    }

    /// <summary>
    /// Returns the count of SLA breaches in the last hour plus up to 5 alert details for
    /// the navbar notification bell dropdown.  On DB failure returns a zero-count response
    /// so the UI degrades gracefully.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetUnread()
    {
        try
        {
            await using var db = await _dbFactory.CreateDbContextAsync();

            var cutoff = DateTime.UtcNow.AddHours(-1);

            var breaches = await db.MetricEvents
                .AsNoTracking()
                .Where(e => !e.IsWithinSla && e.OccurredAt >= cutoff)
                .OrderByDescending(e => e.OccurredAt)
                .Select(e => new
                {
                    provider   = e.Provider,
                    model      = e.Model,
                    latencyMs  = e.LatencyMs,
                    occurredAt = e.OccurredAt.ToString("yyyy-MM-dd HH:mm:ss")
                })
                .ToListAsync();

            return Json(new
            {
                count  = breaches.Count,
                alerts = breaches.Take(5).ToList()
            });
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "GetUnread: DB unavailable — returning empty notification response.");
            return Json(new { count = 0, alerts = Array.Empty<object>() });
        }
    }
}
