using ArNir.Core.DTOs.Evaluation;
using ArNir.Data;
using ArNir.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ArNir.Services;

/// <summary>
/// Provides CRUD and aggregate queries for persisted <c>EvaluationResults</c>.
/// Uses <see cref="IDbContextFactory{ArNirDbContext}"/> for short-lived DB contexts.
/// </summary>
public class EvaluationHistoryService : IEvaluationHistoryService
{
    private readonly IDbContextFactory<ArNirDbContext> _dbFactory;
    private readonly ILogger<EvaluationHistoryService> _logger;

    /// <summary>Initialises the service.</summary>
    public EvaluationHistoryService(
        IDbContextFactory<ArNirDbContext> dbFactory,
        ILogger<EvaluationHistoryService> logger)
    {
        _dbFactory = dbFactory;
        _logger    = logger;
    }

    /// <inheritdoc />
    public async Task<List<EvaluationResultDto>> GetRecentAsync(
        int page = 1, int pageSize = 20,
        DateTime? startDate = null, DateTime? endDate = null,
        double? minRelevance = null, double? minFaithfulness = null)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        var query = db.EvaluationResults.AsNoTracking().AsQueryable();

        if (startDate.HasValue) query = query.Where(e => e.EvaluatedAt >= startDate.Value);
        if (endDate.HasValue)   query = query.Where(e => e.EvaluatedAt <= endDate.Value);
        if (minRelevance.HasValue)    query = query.Where(e => e.RelevanceScore >= minRelevance.Value);
        if (minFaithfulness.HasValue) query = query.Where(e => e.FaithfulnessScore >= minFaithfulness.Value);

        return await query
            .OrderByDescending(e => e.EvaluatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(e => new EvaluationResultDto
            {
                Id                = e.Id,
                Question          = e.Question,
                Answer            = e.Answer,
                RelevanceScore    = e.RelevanceScore,
                FaithfulnessScore = e.FaithfulnessScore,
                Reasoning         = e.Reasoning,
                EvaluatedAt       = e.EvaluatedAt,
                RelatedHistoryId  = e.RelatedHistoryId
            })
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<EvaluationStatsDto> GetStatsAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        var query = db.EvaluationResults.AsNoTracking().AsQueryable();

        if (startDate.HasValue) query = query.Where(e => e.EvaluatedAt >= startDate.Value);
        if (endDate.HasValue)   query = query.Where(e => e.EvaluatedAt <= endDate.Value);

        var all = await query.ToListAsync();
        if (all.Count == 0)
        {
            return new EvaluationStatsDto
            {
                AvgRelevance     = 0,
                AvgFaithfulness  = 0,
                TotalEvaluations = 0,
                Trends           = new()
            };
        }

        var trends = all
            .GroupBy(e => e.EvaluatedAt.Date)
            .OrderBy(g => g.Key)
            .Select(g => new EvaluationTrendPoint
            {
                Date            = g.Key,
                AvgRelevance    = g.Average(e => e.RelevanceScore),
                AvgFaithfulness = g.Average(e => e.FaithfulnessScore),
                Count           = g.Count()
            })
            .ToList();

        return new EvaluationStatsDto
        {
            AvgRelevance     = all.Average(e => e.RelevanceScore),
            AvgFaithfulness  = all.Average(e => e.FaithfulnessScore),
            TotalEvaluations = all.Count,
            Trends           = trends
        };
    }

    /// <inheritdoc />
    public async Task<EvaluationResultDto?> GetByIdAsync(int id)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        var e = await db.EvaluationResults.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        if (e is null) return null;

        return new EvaluationResultDto
        {
            Id                = e.Id,
            Question          = e.Question,
            Answer            = e.Answer,
            RelevanceScore    = e.RelevanceScore,
            FaithfulnessScore = e.FaithfulnessScore,
            Reasoning         = e.Reasoning,
            EvaluatedAt       = e.EvaluatedAt,
            RelatedHistoryId  = e.RelatedHistoryId
        };
    }

    /// <inheritdoc />
    public async Task<int> GetTotalCountAsync()
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        return await db.EvaluationResults.CountAsync();
    }
}
