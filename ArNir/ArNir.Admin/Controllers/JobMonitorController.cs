using ArNir.RAG.Hosting;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ArNir.Admin.Controllers;

/// <summary>
/// Admin panel for monitoring the background ingestion queue in real time.
/// Provides queue depth and recent job results via AJAX polling.
/// </summary>
[Authorize]
public class JobMonitorController : Controller
{
    private readonly IngestionQueue _queue;
    private readonly ILogger<JobMonitorController> _logger;

    /// <summary>Initialises the controller.</summary>
    public JobMonitorController(IngestionQueue queue, ILogger<JobMonitorController> logger)
    {
        _queue  = queue;
        _logger = logger;
    }

    // GET /JobMonitor
    /// <summary>Renders the Job Monitor dashboard page.</summary>
    public IActionResult Index() => View(_queue);

    // GET /JobMonitor/Status
    /// <summary>
    /// AJAX polling endpoint: returns current queue depth and the 10 most recent job results.
    /// </summary>
    [HttpGet]
    public IActionResult Status()
    {
        var recent = _queue.RecentResults.ToArray()
            .Reverse()
            .Take(10)
            .ToList();

        return Json(new
        {
            queueDepth    = _queue.QueueDepth,
            recentResults = recent.Select(r => new
            {
                r.DocumentName,
                r.Success,
                r.ChunksCreated,
                r.EmbeddingsCreated,
                r.Error,
                completedAt = r.CompletedAt.ToString("yyyy-MM-dd HH:mm:ss")
            })
        });
    }
}
