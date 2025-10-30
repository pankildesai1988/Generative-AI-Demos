using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ArNir.Core.DTOs.Intelligence;
using ArNir.Data;
using ArNir.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ArNir.Services
{
    public class AIInsightService : IAIInsightService
    {
        private readonly IDbContextFactory<ArNirDbContext> _factory;
        private readonly ILogger<AIInsightService> _logger;
        private readonly IExportHistoryService _exportHistoryService;

        public AIInsightService(
            IDbContextFactory<ArNirDbContext> factory,
            ILogger<AIInsightService> logger,
            IExportHistoryService exportHistoryService)
        {
            _factory = factory;
            _logger = logger;
            _exportHistoryService = exportHistoryService;
        }

        public async Task<List<AIInsightDto>> GenerateInsightsAsync(
            string? provider = null,
            DateTime? start = null,
            DateTime? end = null)
        {
            var insights = new List<AIInsightDto>();

            try
            {
                using var ctx = _factory.CreateDbContext();
                var query = ctx.RagComparisonHistories.AsQueryable();

                if (!string.IsNullOrEmpty(provider))
                    query = query.Where(x => x.Provider == provider);

                if (start.HasValue)
                    query = query.Where(x => x.CreatedAt >= start.Value);

                if (end.HasValue)
                    query = query.Where(x => x.CreatedAt <= end.Value);

                var data = await query
                    .Select(x => new { x.Provider, x.TotalLatencyMs, x.CreatedAt })
                    .OrderBy(x => x.CreatedAt)
                    .ToListAsync();

                if (!data.Any())
                {
                    insights.Add(new AIInsightDto
                    {
                        Title = "No Data Available",
                        InsightText = "No latency or SLA records were found for the selected filters.",
                        Severity = "info",
                        GeneratedAt = DateTime.UtcNow
                    });

                    // 🧾 Log insight generation
                    await _exportHistoryService.LogExportAsync("system", provider, "insight", start, end);
                    return insights;
                }

                // 📈 Calculate latency statistics
                var avgLatency = data.Average(d => d.TotalLatencyMs);
                var maxLatency = data.Max(d => d.TotalLatencyMs);
                var minLatency = data.Min(d => d.TotalLatencyMs);

                // 📊 Detect anomaly (spike)
                var recent = data.TakeLast(5).Select(d => d.TotalLatencyMs).ToList();
                var recentAvg = recent.Average();
                var deviation = Math.Abs(recentAvg - avgLatency) / avgLatency * 100;

                if (deviation > 25)
                {
                    insights.Add(new AIInsightDto
                    {
                        Title = "Latency Spike Detected",
                        InsightText = $"Latency increased by {deviation:F1}% compared to average ({avgLatency:F0} ms). Recent avg: {recentAvg:F0} ms.",
                        Severity = "warning",
                        GeneratedAt = DateTime.UtcNow
                    });
                }

                if (maxLatency > avgLatency * 1.5)
                {
                    insights.Add(new AIInsightDto
                    {
                        Title = "High Latency Outlier",
                        InsightText = $"An outlier latency of {maxLatency:F0} ms was observed — 50% above average.",
                        Severity = "critical",
                        GeneratedAt = DateTime.UtcNow
                    });
                }

                // 💡 Summary insight
                var summary = provider is null
                    ? $"Average latency across providers: {avgLatency:F0} ms."
                    : $"{provider} average latency: {avgLatency:F0} ms, peak: {maxLatency:F0} ms, lowest: {minLatency:F0} ms.";

                insights.Add(new AIInsightDto
                {
                    Title = "Performance Summary",
                    InsightText = summary,
                    Severity = "info",
                    GeneratedAt = DateTime.UtcNow
                });

                // 🧾 Log insight generation in ExportHistory
                await _exportHistoryService.LogExportAsync("system", provider, "insight", start, end);

                return insights;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AIInsightService failed to generate insights");
                insights.Add(new AIInsightDto
                {
                    Title = "Insight Generation Error",
                    InsightText = $"Error generating insights: {ex.Message}",
                    Severity = "error",
                    GeneratedAt = DateTime.UtcNow
                });

                // 🧾 Log failure as insight event
                await _exportHistoryService.LogExportAsync("system", provider, "insight-error", start, end);
                return insights;
            }
        }
    }
}
