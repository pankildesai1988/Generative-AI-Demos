using ArNir.Core.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArNir.Data.Repositories
{
    public class RagHistoryRepository : IRagHistoryRepository
    {
        private readonly ArNirDbContext _context;

        public RagHistoryRepository(ArNirDbContext context)
        {
            _context = context;
        }

        public async Task<List<RagComparisonHistory>> GetAllAsync()
        {
            return await _context.RagComparisonHistories
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();
        }

        public async Task<RagComparisonHistory?> GetByIdAsync(int id)
        {
            return await _context.RagComparisonHistories.FindAsync(id);
        }

        public async Task<List<RagComparisonHistory>> FilterAsync(
            string? slaStatus, DateTime? startDate, DateTime? endDate, string? queryText, string? promptStyle, string? provider, string? model)
        {
            var query = _context.RagComparisonHistories.AsQueryable();

            if (!string.IsNullOrEmpty(slaStatus))
            {
                bool isOk = slaStatus == "OK";
                query = query.Where(x => x.IsWithinSla == isOk);
            }

            if (startDate.HasValue)
                query = query.Where(x => x.CreatedAt >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(x => x.CreatedAt <= endDate.Value);

            if (!string.IsNullOrEmpty(queryText))
                query = query.Where(x => x.UserQuery.Contains(queryText));
            
            if (!string.IsNullOrEmpty(promptStyle))
                query = query.Where(x => x.PromptStyle == promptStyle);

            if (!string.IsNullOrEmpty(provider)) query = query.Where(x => x.Provider == provider);   // ✅ new
            if (!string.IsNullOrEmpty(model)) query = query.Where(x => x.Model == model);           // ✅ new

            return await query.OrderByDescending(x => x.CreatedAt).ToListAsync();
        }
    }
}
