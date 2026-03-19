using ArNir.Core.DTOs.Evaluation;
using ArNir.Core.Entities;
using ArNir.Data;
using ArNir.Observability.Interfaces;
using ArNir.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ArNir.Api.Controllers;

/// <summary>
/// REST API endpoints for RAG quality evaluation:
/// history, on-demand evaluation, and aggregate statistics.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class EvaluationController : ControllerBase
{
    private readonly IEvaluationHistoryService _historyService;
    private readonly IEvaluationService _evaluationService;
    private readonly IDbContextFactory<ArNirDbContext> _dbFactory;
    private readonly ILogger<EvaluationController> _logger;

    public EvaluationController(
        IEvaluationHistoryService historyService,
        IEvaluationService evaluationService,
        IDbContextFactory<ArNirDbContext> dbFactory,
        ILogger<EvaluationController> logger)
    {
        _historyService = historyService;
        _evaluationService = evaluationService;
        _dbFactory = dbFactory;
        _logger = logger;
    }

    /// <summary>GET /api/evaluation/history — paginated evaluation history with optional filters.</summary>
    [HttpGet("history")]
    public async Task<IActionResult> GetHistory(
        int page = 1, int pageSize = 20,
        DateTime? startDate = null, DateTime? endDate = null,
        double? minRelevance = null, double? minFaithfulness = null)
    {
        var results = await _historyService.GetRecentAsync(
            page, pageSize, startDate, endDate, minRelevance, minFaithfulness);
        var total = await _historyService.GetTotalCountAsync();

        return Ok(new { data = results, total, page, pageSize });
    }

    /// <summary>POST /api/evaluation/evaluate — run an on-demand LLM evaluation.</summary>
    [HttpPost("evaluate")]
    public async Task<IActionResult> Evaluate([FromBody] EvaluationRequestDto request)
    {
        if (request is null || string.IsNullOrWhiteSpace(request.Question))
            return BadRequest("Question is required.");

        var evalResult = await _evaluationService.EvaluateAsync(
            request.Question, request.Answer, request.Context);

        // Persist the evaluation
        await using var db = await _dbFactory.CreateDbContextAsync();
        var entity = new EvaluationResultEntity
        {
            Question          = request.Question,
            Answer            = request.Answer,
            Context           = request.Context,
            RelevanceScore    = evalResult.RelevanceScore,
            FaithfulnessScore = evalResult.FaithfulnessScore,
            Reasoning         = evalResult.Reasoning,
            EvaluatedAt       = evalResult.EvaluatedAt
        };
        db.EvaluationResults.Add(entity);
        await db.SaveChangesAsync();

        return Ok(new EvaluationResultDto
        {
            Id                = entity.Id,
            Question          = entity.Question,
            Answer            = entity.Answer,
            RelevanceScore    = entity.RelevanceScore,
            FaithfulnessScore = entity.FaithfulnessScore,
            Reasoning         = entity.Reasoning,
            EvaluatedAt       = entity.EvaluatedAt
        });
    }

    /// <summary>GET /api/evaluation/stats — aggregate evaluation statistics with trends.</summary>
    [HttpGet("stats")]
    public async Task<IActionResult> GetStats(DateTime? startDate = null, DateTime? endDate = null)
    {
        var stats = await _historyService.GetStatsAsync(startDate, endDate);
        return Ok(stats);
    }
}
