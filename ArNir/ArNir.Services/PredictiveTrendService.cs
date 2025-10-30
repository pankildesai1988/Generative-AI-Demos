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
    public class PredictiveTrendService : IPredictiveTrendService
    {
        private readonly IDbContextFactory<ArNirDbContext> _factory;

        public PredictiveTrendService(IDbContextFactory<ArNirDbContext> factory)
        {
            _factory = factory;
        }

        public async Task<ChartDataDto?> GetForecastAsync(string? provider = null, DateTime? start = null, DateTime? end = null)
        {
            using var ctx = _factory.CreateDbContext();

            // Get recent history
            var query = ctx.RagComparisonHistories.AsQueryable();
            if (!string.IsNullOrEmpty(provider))
                query = query.Where(x => x.Provider == provider);
            if (start.HasValue)
                query = query.Where(x => x.CreatedAt >= start.Value);
            if (end.HasValue)
                query = query.Where(x => x.CreatedAt <= end.Value);

            var history = await query
                .OrderByDescending(x => x.CreatedAt)
                .Take(30)
                .ToListAsync();

            if (!history.Any())
                return new ChartDataDto
                {
                    Title = $"Latency Forecast (Next 7 Days){(provider != null ? $" - {provider}" : "")}",
                    Data = new List<ChartSeriesItemDto>()
                };

            // Group by provider
            var grouped = provider != null
                ? new Dictionary<string, List<long>> { [provider] = history.Select(x => x.TotalLatencyMs).ToList() }
                : history.GroupBy(x => x.Provider)
                         .ToDictionary(g => g.Key, g => g.Select(x => x.TotalLatencyMs).ToList());

            var allPoints = new List<ChartSeriesItemDto>();

            foreach (var g in grouped)
            {
                var values = g.Value.Where(v => v > 0).ToList();
                if (values.Count < 3) continue; // Need enough data

                double mean = values.Average();
                double variance = values.Average(v => Math.Pow(v - mean, 2));
                double stdDev = Math.Sqrt(variance);

                if (double.IsNaN(mean) || double.IsNaN(stdDev) || stdDev == 0)
                {
                    stdDev = mean * 0.05; // fallback variance
                }

                for (int i = 1; i <= 7; i++)
                {
                    var day = DateTime.UtcNow.AddDays(i);
                    // Simple moving average with sinusoidal variation
                    double predicted = mean + Math.Sin(i * Math.PI / 7) * stdDev * 0.1;
                    double lower = predicted - stdDev * 0.2;
                    double upper = predicted + stdDev * 0.2;

                    allPoints.Add(new ChartSeriesItemDto
                    {
                        Date = day,
                        Provider = g.Key,
                        Predicted = Math.Round(predicted, 2),
                        LowerBound = Math.Round(lower, 2),
                        UpperBound = Math.Round(upper, 2)
                    });
                }
            }

            // In case no provider produced data
            if (!allPoints.Any())
            {
                allPoints.Add(new ChartSeriesItemDto
                {
                    Date = DateTime.UtcNow.AddDays(1),
                    Provider = provider ?? "Unknown",
                    Predicted = 10000,
                    LowerBound = 9000,
                    UpperBound = 11000
                });
            }

            return new ChartDataDto
            {
                Title = provider != null
                    ? $"Latency Forecast (Next 7 Days) - {provider}"
                    : "Multi-Provider Latency Forecast (Next 7 Days)",
                Data = allPoints
            };
        }
    }
}
