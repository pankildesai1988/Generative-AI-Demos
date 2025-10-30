using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ArNir.Core.DTOs.Intelligence;
using ArNir.Data;
using ArNir.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ArNir.Services
{
    public class NotificationService : INotificationService
    {
        private readonly IDbContextFactory<ArNirDbContext> _factory;

        public NotificationService(IDbContextFactory<ArNirDbContext> factory)
        {
            _factory = factory;
        }

        public async Task<List<AlertDto>> GetActiveAlertsAsync(string? provider = null)
        {
            using var ctx = _factory.CreateDbContext();
            var recent = DateTime.UtcNow.AddDays(-7);
            var q = ctx.RagComparisonHistories
                .Where(x => x.CreatedAt >= recent);

            if (!string.IsNullOrEmpty(provider))
                q = q.Where(x => x.Provider == provider);

            var alerts = new List<AlertDto>();

            var latencyAlerts = await q
                .Where(x => x.TotalLatencyMs > 4000)
                .Select(x => new AlertDto
                {
                    Type = "Latency",
                    Message = $"{x.Provider} exceeded {x.TotalLatencyMs}ms latency on {x.CreatedAt:yyyy-MM-dd}",
                    CreatedAt = x.CreatedAt
                })
                .ToListAsync();

            alerts.AddRange(latencyAlerts);

            var slaAlerts = await q
                .Where(x => !x.IsWithinSla)
                .Select(x => new AlertDto
                {
                    Type = "SLA",
                    Message = $"{x.Provider} SLA violation for model {x.Model} ({x.PromptStyle})",
                    CreatedAt = x.CreatedAt
                })
                .ToListAsync();

            alerts.AddRange(slaAlerts);

            return alerts
                .OrderByDescending(a => a.CreatedAt)
                .Take(50)
                .ToList();
        }
    }
}
