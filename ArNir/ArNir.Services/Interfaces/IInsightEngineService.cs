using System;
using System.Threading.Tasks;

namespace ArNir.Services.Interfaces
{
    public interface IInsightEngineService
    {
        Task<string> GenerateSummaryAsync(
            string? provider, DateTime? startDate, DateTime? endDate, string? userPrompt = null);
    }
}
