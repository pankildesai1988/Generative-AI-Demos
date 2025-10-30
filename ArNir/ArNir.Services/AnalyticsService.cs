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
    public class AnalyticsService : IAnalyticsService
    {
        private readonly IDbContextFactory<ArNirDbContext> _factory;

        public AnalyticsService(IDbContextFactory<ArNirDbContext> factory)
        {
            _factory = factory;
        }

        /// <summary>
        /// Returns KPI metrics filtered by provider/date range.
        /// </summary>
        public async Task<List<KpiMetricDto>> GetKpisAsync(string? provider = null, DateTime? start = null, DateTime? end = null)
        {
            using var ctx = _factory.CreateDbContext();
            var q = ctx.RagComparisonHistories.AsQueryable();

            if (!string.IsNullOrEmpty(provider))
                q = q.Where(x => x.Provider == provider);
            if (start.HasValue)
                q = q.Where(x => x.CreatedAt >= start.Value);
            if (end.HasValue)
                q = q.Where(x => x.CreatedAt <= end.Value);

            var totalRuns = await q.CountAsync();

            if (totalRuns == 0)
            {
                return new List<KpiMetricDto>
                {
                    new() { Label = "Total Runs", Value = 0 },
                    new() { Label = "Avg Latency", Value = 0, Unit = "ms" },
                    new() { Label = "SLA Compliance", Value = 0, Unit = "%" }
                };
            }

            var avgLatency = await q.AverageAsync(x => (double)x.TotalLatencyMs);
            var slaRate = (await q.CountAsync(x => x.IsWithinSla) * 100.0 / totalRuns);

            return new List<KpiMetricDto>
            {
                new() { Label = "Total Runs", Value = totalRuns },
                new() { Label = "Avg Latency", Value = Math.Round(avgLatency, 2), Unit = "ms" },
                new() { Label = "SLA Compliance", Value = Math.Round(slaRate, 2), Unit = "%" }
            };
        }

        /// <summary>
        /// Returns average latency chart data per provider over time.
        /// </summary>
        public async Task<List<ChartDataDto>> GetChartsAsync(string? provider = null, DateTime? start = null, DateTime? end = null)
        {
            using var ctx = _factory.CreateDbContext();
            var q = ctx.RagComparisonHistories.AsQueryable();

            if (!string.IsNullOrEmpty(provider))
                q = q.Where(x => x.Provider == provider);
            if (start.HasValue)
                q = q.Where(x => x.CreatedAt >= start.Value);
            if (end.HasValue)
                q = q.Where(x => x.CreatedAt <= end.Value);

            var grouped = await q
                .GroupBy(x => new { x.Provider, Date = x.CreatedAt.Date })
                .Select(g => new
                {
                    g.Key.Provider,
                    g.Key.Date,
                    AvgLatency = g.Average(x => (double)x.TotalLatencyMs)
                })
                .OrderBy(g => g.Date)
                .ToListAsync();

            // Convert anonymous objects → DTO
            var chart = new ChartDataDto
            {
                Title = provider != null ? $"Latency Trend - {provider}" : "Average Latency by Provider",
                Data = grouped.Select(x => new ChartSeriesItemDto
                {
                    Date = x.Date,
                    Provider = x.Provider,
                    AvgLatency = Math.Round(x.AvgLatency, 2)
                }).ToList()
            };

            return new List<ChartDataDto> { chart };
        }

    }
}
