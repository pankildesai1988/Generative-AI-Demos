using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArNir.Services.Interfaces;
using ArNir.Core.DTOs.Intelligence;
using Microsoft.Extensions.Logging;

namespace ArNir.Services
{
    public class InsightEngineService : IInsightEngineService
    {
        private readonly IAnalyticsService _analyticsService;
        private readonly IPredictiveTrendService _predictiveTrendService;
        private readonly INaturalLanguageCommandService _nlpCommandService;
        private readonly ILlmService _llmService;
        private readonly ILogger<InsightEngineService> _logger;

        public InsightEngineService(
            IAnalyticsService analyticsService,
            IPredictiveTrendService predictiveTrendService,
            INaturalLanguageCommandService nlpCommandService,
            ILlmService llmService,
            ILogger<InsightEngineService> logger)
        {
            _analyticsService = analyticsService;
            _predictiveTrendService = predictiveTrendService;
            _nlpCommandService = nlpCommandService;
            _llmService = llmService;
            _logger = logger;
        }

        /// <summary>
        /// Unified AI summary endpoint with NLP pre-processing.
        /// </summary>
        public async Task<string> GenerateSummaryAsync(string? provider, DateTime? start, DateTime? end)
        {
            try
            {
                var prompt = $"Summarize performance trends for provider={provider ?? "all"} between {start:d} and {end:d}";
                _logger.LogInformation("Generating AI Summary: {prompt}", prompt);

                // 🧠 Step 1 — Check for NLP Command
                var nlpResult = await _nlpCommandService.TryParseCommandAsync(prompt);
                if (!string.IsNullOrEmpty(nlpResult))
                {
                    _logger.LogInformation("NLP Command matched. Returning precomputed result.");
                    return nlpResult;
                }

                // 🧮 Step 2 — Get KPIs + Trend data
                var kpis = await _analyticsService.GetKpisAsync(provider, start, end);
                var chartData = await _predictiveTrendService.GetForecastAsync(provider, start, end);

                // 🧠 Step 3 — Build contextual prompt
                var kpiSummary = string.Join(", ", kpis.Select(k => $"{k.Label}: {k.Value}{k.Unit}"));
                var forecastSummary = chartData?.Data?.Any() == true
                    ? $"Forecast avg latency: {chartData.Data.Average(d => d.Predicted):0.0}ms"
                    : "No forecast data available.";

                var gptPrompt = $@"
You are an AI Insight Engine analyzing service performance.
Summarize trends and anomalies for provider '{provider ?? "all"}' between {start:d} and {end:d}.

Metrics: {kpiSummary}.
Forecast Summary: {forecastSummary}.

Provide a 3-4 bullet point summary with insights and recommendations.
";

                var result = await _llmService.GenerateCompletionAsync(gptPrompt);
                return result ?? "No insights generated.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating AI summary.");
                return "⚠️ Unable to generate insights due to an internal error.";
            }
        }

        /// <summary>
        /// Compare current week vs last week and summarize changes.
        /// </summary>
        public async Task<string> CompareWeekToWeekAsync(string? provider)
        {
            try
            {
                var end = DateTime.UtcNow.Date;
                var start = end.AddDays(-7);
                var prevStart = start.AddDays(-7);
                var prevEnd = start;

                var currentWeekData = await _analyticsService.GetChartsAsync(provider, start, end);
                var lastWeekData = await _analyticsService.GetChartsAsync(provider, prevStart, prevEnd);

                if (!currentWeekData.Any() || !lastWeekData.Any())
                    return "Insufficient data to compare week-over-week trends.";

                // Flatten both sets into comparable values
                var currentLatencies = currentWeekData
                    .SelectMany(c => c.Data)
                    .Where(d => d.AvgLatency > 0)
                    .Select(d => d.AvgLatency)
                    .ToList();

                var previousLatencies = lastWeekData
                    .SelectMany(c => c.Data)
                    .Where(d => d.AvgLatency > 0)
                    .Select(d => d.AvgLatency)
                    .ToList();

                if (!currentLatencies.Any() || !previousLatencies.Any())
                    return "No latency data available for week-over-week comparison.";

                double currentAvg = currentLatencies.Average().Value;
                double prevAvg = previousLatencies.Average().Value;
                double delta = currentAvg - prevAvg;
                double deltaPercent = (delta / prevAvg) * 100;

                var trend = delta > 0 ? "increased" : "decreased";
                var emoji = delta > 0 ? "⚠️" : "✅";

                var summary = new StringBuilder();
                summary.AppendLine($"{emoji} **Week-over-Week Comparison for {provider ?? "all providers"}**");
                summary.AppendLine($"- Average Latency last week: {prevAvg:0.0} ms");
                summary.AppendLine($"- Average Latency this week: {currentAvg:0.0} ms");
                summary.AppendLine($"- Change: {trend} by {Math.Abs(deltaPercent):0.0}%");
                summary.AppendLine();
                summary.AppendLine(delta > 0
                    ? "🧠 Recommendation: Investigate latency increases; consider optimizing provider routing or concurrency limits."
                    : "🚀 Great improvement! Maintain this performance pattern and review scaling strategy.");

                return summary.ToString();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in CompareWeekToWeekAsync");
                return "⚠️ Unable to perform week-over-week comparison.";
            }
        }

        /// <summary>
        /// Extended NLP command integration — includes week-over-week delta support
        /// </summary>
        public async Task<string> ProcessQueryAsync(string userPrompt)
        {
            // Step 1: Try to handle NLP command first
            var nlpResult = await _nlpCommandService.TryParseCommandAsync(userPrompt);
            if (!string.IsNullOrEmpty(nlpResult))
                return nlpResult;

            // Step 2: Week-over-week comparison trigger
            if (userPrompt.ToLower().Contains("compare last week") ||
                userPrompt.ToLower().Contains("week over week") ||
                userPrompt.ToLower().Contains("performance change"))
            {
                return await CompareWeekToWeekAsync(null);
            }

            // Step 3: Fall back to LLM-generated contextual insights
            var result = await _llmService.GenerateCompletionAsync(
                $"Analyze and summarize performance insights for this query: {userPrompt}");

            return result ?? "No insights generated.";
        }
    }
}
