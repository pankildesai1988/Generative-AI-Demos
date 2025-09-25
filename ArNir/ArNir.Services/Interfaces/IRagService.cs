using ArNir.Core.DTOs.Analytics;
using ArNir.Core.DTOs.RAG;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ArNir.Services.Interfaces
{
    public interface IRagService
    {
        Task<RagResultDto> RunRagAsync(string query, int topK = 5, bool useHybrid = true, string promptStyle = "rag", bool saveAsNew = true);

        Task<AvgLatencyDto> GetAverageLatenciesAsync(DateTime? startDate = null, DateTime? endDate = null, string? slaStatus = null, string? promptStyle = null);
        Task<SlaComplianceDto> GetSlaComplianceAsync(DateTime? startDate = null, DateTime? endDate = null, string? slaStatus = null, string? promptStyle = null);
        Task<List<PromptStyleUsageDto>> GetPromptStyleUsageAsync(DateTime? startDate = null, DateTime? endDate = null, string? slaStatus = null, string? promptStyle = null);
        Task<List<TrendDto>> GetTrendsAsync(DateTime startDate, DateTime endDate, string? slaStatus = null, string? promptStyle = null);
    }
}
