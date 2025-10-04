using ArNir.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArNir.Data.Repositories
{
    public interface IRagHistoryRepository
    {
        Task<List<RagComparisonHistory>> GetAllAsync();
        Task<RagComparisonHistory?> GetByIdAsync(int id);
        Task<List<RagComparisonHistory>> FilterAsync(
            string? slaStatus, DateTime? startDate, DateTime? endDate, string? queryText, string? promptStyle, string? provider, string? model);
    }
}
