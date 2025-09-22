using ArNir.Core.DTOs.RAG;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArNir.Services.Interfaces
{
    public interface IRagHistoryService
    {
        Task<List<RagHistoryListDto>> GetHistoryListAsync(
            string? slaStatus, DateTime? startDate, DateTime? endDate, string? queryText, string? promptStyle);

        Task<RagHistoryDetailDto?> GetHistoryDetailsAsync(int id);
    }
}
