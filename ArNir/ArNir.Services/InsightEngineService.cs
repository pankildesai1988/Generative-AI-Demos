using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArNir.Services.Interfaces;
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
        /// Now supports userPrompt (custom text input for GPT insight).
        /// </summary>
        public async Task<string> GenerateSummaryAsync(string? provider, DateTime? startDate, DateTime? endDate, string? userPrompt = null)
        {
            try
            {
                var basePrompt = userPrompt ??
                                 $"Summarize performance trends for provider={provider ?? "all"} between {startDate:d} and {endDate:d}";

                _logger.LogInformation("Generating AI Summary: {prompt}", basePrompt);

                // 🧠 Step 1 — NLP Command handling
                var nlpResult = await _nlpCommandService.TryParseCommandAsync(basePrompt);
                if (!string.IsNullOrEmpty(nlpResult))
                {
                    _logger.LogInformation("NLP Command matched. Returning precomputed result.");
                    return nlpResult;
                }

                // 🧮 Step 2 — Get KPIs + Forecast
                var kpis = await _analyticsService.GetKpisAsync(provider, startDate, endDate);
                var chartData = await _predictiveTrendService.GetForecastAsync(provider, startDate, endDate);

                // 🧠 Step 3 — Build contextual GPT prompt
                var kpiSummary = string.Join(", ", kpis.Select(k => $"{k.Label}: {k.Value}{k.Unit}"));
                var forecastSummary = chartData?.Data?.Any() == true
                    ? $"Forecast avg latency: {chartData.Data.Average(d => d.Predicted):0.0}ms"
                    : "No forecast data available.";

                var gptPrompt = $@"
You are an AI Insight Engine analyzing system performance.
Summarize trends and anomalies for provider '{provider ?? "all"}' between {startDate:d} and {endDate:d}.

Metrics: {kpiSummary}
Forecast Summary: {forecastSummary}

User Context: {userPrompt ?? "(none)"}

Provide a 3-4 bullet point summary with insights and recommendations.
";

                // 🧩 Step 4 — Generate LLM response
                var result = await _llmService.GetCompletionAsync(gptPrompt, "gpt-4o");
                return result ?? "No insights generated.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating AI summary.");
                return "⚠️ Unable to generate insights due to an internal error.";
            }
        }
    }
}
