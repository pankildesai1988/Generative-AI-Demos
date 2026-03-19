using ArNir.Core.DTOs.Evaluation;

namespace ArNir.Services.Interfaces;

/// <summary>
/// CRUD service for persisted evaluation results.
/// Separate from <c>IEvaluationService</c> (which performs the actual LLM-based evaluation).
/// </summary>
public interface IEvaluationHistoryService
{
    /// <summary>Returns a paginated list of recent evaluations with optional filters.</summary>
    Task<List<EvaluationResultDto>> GetRecentAsync(
        int page = 1,
        int pageSize = 20,
        DateTime? startDate = null,
        DateTime? endDate = null,
        double? minRelevance = null,
        double? minFaithfulness = null);

    /// <summary>Returns aggregate statistics and daily trend data.</summary>
    Task<EvaluationStatsDto> GetStatsAsync(DateTime? startDate = null, DateTime? endDate = null);

    /// <summary>Returns a single evaluation by its ID.</summary>
    Task<EvaluationResultDto?> GetByIdAsync(int id);

    /// <summary>Returns the total count of evaluation records.</summary>
    Task<int> GetTotalCountAsync();
}
