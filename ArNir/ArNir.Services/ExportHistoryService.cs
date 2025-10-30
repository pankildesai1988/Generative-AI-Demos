using ArNir.Core.DTOs.Intelligence;
using ArNir.Core.Entities;
using ArNir.Data;
using ArNir.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ArNir.Services
{
    public class ExportHistoryService : IExportHistoryService
    {
        private readonly IDbContextFactory<ArNirDbContext> _factory;
        private readonly ILogger<ExportHistoryService> _logger;

        public ExportHistoryService(IDbContextFactory<ArNirDbContext> factory, ILogger<ExportHistoryService> logger)
        {
            _factory = factory;
            _logger = logger;
        }

        public async Task LogExportAsync(string? userName, string? provider, string? format, DateTime? start, DateTime? end)
        {
            try
            {
                using var ctx = _factory.CreateDbContext();
                var range = $"{start?.ToString("yyyy-MM-dd")} to {end?.ToString("yyyy-MM-dd")}";
                var record = new ExportHistory
                {
                    UserName = userName ?? "system",
                    Provider = provider,
                    Format = format,
                    DateRange = range,
                    CreatedAt = DateTime.UtcNow
                };
                ctx.ExportHistories.Add(record);
                await ctx.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error logging export history");
            }
        }

        public async Task<List<ExportHistoryDto>> GetExportHistoryAsync(string? userName = null)
        {
            using var ctx = _factory.CreateDbContext();
            var query = ctx.ExportHistories.AsQueryable();

            if (!string.IsNullOrEmpty(userName))
                query = query.Where(e => e.UserName == userName);

            return await query
                .OrderByDescending(e => e.CreatedAt)
                .Select(e => new ExportHistoryDto
                {
                    Id = e.Id,
                    UserName = e.UserName,
                    Provider = e.Provider,
                    Format = e.Format,
                    DateRange = e.DateRange,
                    CreatedAt = e.CreatedAt
                })
                .ToListAsync();
        }
    }
}
