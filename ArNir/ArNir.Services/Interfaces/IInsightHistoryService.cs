using System.Collections.Generic;
using System.Threading.Tasks;
using ArNir.Core.DTOs.Intelligence;

namespace ArNir.Services.Interfaces
{
    public interface IInsightHistoryService
    {
        Task<List<InsightItemDto>> GetRecentInsightsAsync(int count = 5);
    }
}
