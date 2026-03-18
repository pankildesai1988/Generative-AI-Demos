using ArNir.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ArNir.Admin.Controllers;

/// <summary>View-model for the Observability Dashboard index page.</summary>
public class ObservabilityDashboardViewModel
{
    public long    TotalEvents       { get; set; }
    public double  AvgLatencyMs      { get; set; }
    public double  SlaCompliancePct  { get; set; }
    public IReadOnlyList<ProviderSummary> ByProvider { get; set; } = Array.Empty<ProviderSummary>();
    public IReadOnlyList<RecentEvent>     Recent      { get; set; } = Array.Empty<RecentEvent>();
}

/// <summary>Per-provider aggregate stats.</summary>
public class ProviderSummary
{
    public string Provider       { get; set; } = string.Empty;
    public long   EventCount     { get; set; }
    public double AvgLatencyMs   { get; set; }
    public double SlaCompliance  { get; set; }
    public long   TotalTokens    { get; set; }
}

/// <summary>Lightweight row shown in the recent-events table.</summary>
public class RecentEvent
{
    public int      Id          { get; set; }
    public string   EventType   { get; set; } = string.Empty;
    public string   Provider    { get; set; } = string.Empty;
    public string   Model       { get; set; } = string.Empty;
    public long     LatencyMs   { get; set; }
    public bool     IsWithinSla { get; set; }
    public DateTime OccurredAt  { get; set; }
}

/// <summary>
/// Admin SLA dashboard that aggregates <c>MetricEvents</c> for quick insight into
/// provider performance, latency trends, and SLA compliance rates.
/// </summary>
[Authorize]
public class ObservabilityDashboardController : Controller
{
    private readonly IDbContextFactory<ArNirDbContext>     _dbFactory;
    private readonly ILogger<ObservabilityDashboardController> _logger;

    /// <summary>Initialises the controller.</summary>
    public ObservabilityDashboardController(
        IDbContextFactory<ArNirDbContext> dbFactory,
        ILogger<ObservabilityDashboardController> logger)
    {
        _dbFactory = dbFactory;
        _logger    = logger;
    }

    // GET /ObservabilityDashboard
    /// <summary>Renders the SLA dashboard with aggregated metrics.</summary>
    public async Task<IActionResult> Index()
    {
        await using var db = await _dbFactory.CreateDbContextAsync();

        var all = await db.MetricEvents.AsNoTracking().ToListAsync();
        if (!all.Any())
            return View(new ObservabilityDashboardViewModel());

        var vm = new ObservabilityDashboardViewModel
        {
            TotalEvents      = all.Count,
            AvgLatencyMs     = all.Average(x => (double)x.LatencyMs),
            SlaCompliancePct = all.Count(x => x.IsWithinSla) * 100.0 / all.Count,
            ByProvider = all
                .GroupBy(x => x.Provider)
                .Select(g => new ProviderSummary
                {
                    Provider      = g.Key,
                    EventCount    = g.Count(),
                    AvgLatencyMs  = g.Average(x => (double)x.LatencyMs),
                    SlaCompliance = g.Count(x => x.IsWithinSla) * 100.0 / g.Count(),
                    TotalTokens   = g.Sum(x => (long)x.TokensUsed)
                })
                .OrderByDescending(x => x.EventCount)
                .ToList(),
            Recent = all
                .OrderByDescending(x => x.OccurredAt)
                .Take(20)
                .Select(x => new RecentEvent
                {
                    Id          = x.Id,
                    EventType   = x.EventType,
                    Provider    = x.Provider,
                    Model       = x.Model,
                    LatencyMs   = x.LatencyMs,
                    IsWithinSla = x.IsWithinSla,
                    OccurredAt  = x.OccurredAt
                })
                .ToList()
        };

        return View(vm);
    }
}
