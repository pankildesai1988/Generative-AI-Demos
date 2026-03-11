using ArNir.Core.DTOs.Analytics;
using ArNir.Core.DTOs.Intelligence;
using ArNir.Core.DTOs.RAG;
using ArNir.Core.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ArNir.Services.Interfaces
{
    public interface IRagService
    {
        Task<RagResultDto> RunRagAsync(
            string query,
            int topK = 5,
            bool useHybrid = true,
            string promptStyle = "rag",
            bool saveAsNew = true,
            string provider = "OpenAI",
            string model = "gpt-4"
            );

        Task<AnalyticsResponse<AvgLatencyDto>> GetAverageLatenciesAsync(
            DateTime? startDate, DateTime? endDate, string? slaStatus, string? promptStyle);
        Task<AnalyticsResponse<SlaComplianceDto>> GetSlaComplianceAsync(
            DateTime? startDate, DateTime? endDate, string? slaStatus, string? promptStyle);
        Task<AnalyticsResponse<PromptStyleUsageDto>> GetPromptStyleUsageAsync(
            DateTime? startDate, DateTime? endDate, string? slaStatus, string? promptStyle);
        Task<AnalyticsResponse<TrendDto>> GetTrendsAsync(
            DateTime startDate, DateTime endDate, string? slaStatus, string? promptStyle);

        Task<AnalyticsResponse<ProviderAnalyticsDto>> GetProviderAnalyticsAsync(
            DateTime? startDate = null, DateTime? endDate = null, string? promptStyle = null);

        Task<IEnumerable<RagComparisonHistory>> GetRagHistoryAsync(DateTime? startDate = null, DateTime? endDate = null);

        Task<IEnumerable<RelatedInsightDto>> GetRelatedInsightsAsync(string prompt, int topK = 5);
    }
}
