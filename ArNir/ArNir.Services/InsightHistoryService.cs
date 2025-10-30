using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ArNir.Core.DTOs.Intelligence;
using ArNir.Services.Interfaces;

namespace ArNir.Services
{
    public class InsightHistoryService : IInsightHistoryService
    {
        private static readonly List<InsightItemDto> MockInsights = new()
        {
            new InsightItemDto { Title = "Latency Spike Detected", InsightText = "OpenAI latency exceeded 18,000ms on Oct 10.", Severity = "critical", GeneratedAt = DateTime.UtcNow.AddDays(-1) },
            new InsightItemDto { Title = "SLA Violation", InsightText = "Gemini failed SLA compliance for 2 models.", Severity = "warning", GeneratedAt = DateTime.UtcNow.AddDays(-2) },
            new InsightItemDto { Title = "Token Usage Alert", InsightText = "Maximum token usage observed: 48,532.", Severity = "info", GeneratedAt = DateTime.UtcNow.AddDays(-3) },
            new InsightItemDto { Title = "Latency Improvement", InsightText = "Claude improved latency by 12% week-over-week.", Severity = "success", GeneratedAt = DateTime.UtcNow.AddDays(-4) },
            new InsightItemDto { Title = "Forecast Stability", InsightText = "Predicted latency remains stable through next 7 days.", Severity = "info", GeneratedAt = DateTime.UtcNow.AddDays(-5) },
        };

        public Task<List<InsightItemDto>> GetRecentInsightsAsync(int count = 5)
        {
            var insights = MockInsights
                .OrderByDescending(i => i.GeneratedAt)
                .Take(count)
                .ToList();

            return Task.FromResult(insights);
        }
    }
}
