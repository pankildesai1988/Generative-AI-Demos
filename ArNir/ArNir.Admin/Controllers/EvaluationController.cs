using ArNir.Core.DTOs.Evaluation;
using ArNir.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ArNir.Admin.Controllers;

/// <summary>View-model for the Evaluation dashboard.</summary>
public class EvaluationDashboardViewModel
{
    public EvaluationStatsDto Stats { get; set; } = new();
    public List<EvaluationResultDto> Recent { get; set; } = new();
    public int TotalCount { get; set; }
}

/// <summary>
/// Admin controller that displays RAG quality evaluation metrics,
/// including KPI cards, daily trend charts, and a filterable results table.
/// </summary>
[Authorize]
public class EvaluationController : Controller
{
    private readonly IEvaluationHistoryService _historyService;
    private readonly ILogger<EvaluationController> _logger;

    public EvaluationController(
        IEvaluationHistoryService historyService,
        ILogger<EvaluationController> logger)
    {
        _historyService = historyService;
        _logger = logger;
    }

    // GET /Evaluation
    public async Task<IActionResult> Index()
    {
        var stats = await _historyService.GetStatsAsync();
        var recent = await _historyService.GetRecentAsync(page: 1, pageSize: 50);
        var total = await _historyService.GetTotalCountAsync();

        var vm = new EvaluationDashboardViewModel
        {
            Stats = stats,
            Recent = recent,
            TotalCount = total
        };

        return View(vm);
    }

    // GET /Evaluation/Details/5
    public async Task<IActionResult> Details(int id)
    {
        var result = await _historyService.GetByIdAsync(id);
        if (result is null) return NotFound();
        return View(result);
    }
}
