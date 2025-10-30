using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Linq;
using ArNir.Services.Interfaces;

namespace ArNir.Services.AI
{
    public class NaturalLanguageCommandService : INaturalLanguageCommandService
    {
        private readonly IInsightHistoryService _historyService;
        private readonly IAnalyticsService _analyticsService;

        public NaturalLanguageCommandService(
            IInsightHistoryService historyService,
            IAnalyticsService analyticsService)
        {
            _historyService = historyService;
            _analyticsService = analyticsService;
        }

        public async Task<string?> TryParseCommandAsync(string userInput)
        {
            if (string.IsNullOrWhiteSpace(userInput))
                return null;

            userInput = userInput.ToLower().Trim();

            // 🔹 NLP Pattern 1: "show last 5 responses" or "last 3 insights"
            if (Regex.IsMatch(userInput, @"last\s+\d+\s+(responses|insights)"))
            {
                var match = Regex.Match(userInput, @"last\s+(\d+)");
                int count = int.Parse(match.Groups[1].Value);
                var insights = await _historyService.GetRecentInsightsAsync(count);

                if (!insights.Any())
                    return "No recent insights found.";

                var formatted = string.Join("\n\n", insights.Select(i =>
                    $"🧠 **{i.Title}** — {i.InsightText}\n📅 {i.GeneratedAt:g}"));
                return formatted;
            }

            // 🔹 NLP Pattern 2: "max token" or "maximum token usage"
            if (userInput.Contains("max token"))
            {
                var kpis = await _analyticsService.GetKpisAsync(null, null, null);
                var maxTokens = kpis.FirstOrDefault(k => k.Label.Contains("Token"))?.Value ?? 0;
                return $"The maximum token usage recorded is **{maxTokens}** tokens.";
            }

            // 🔹 NLP Pattern 3: "compare latency" or "compare providers"
            if (userInput.Contains("compare") && userInput.Contains("latency"))
            {
                var charts = await _analyticsService.GetChartsAsync(null, null, null);
                if (!charts.Any()) return "No latency data available for comparison.";

                var providers = charts.SelectMany(c => c.Data)
                    .GroupBy(d => d.Provider)
                    .Select(g => $"{g.Key}: avg {g.Average(x => x.AvgLatency):0.0}ms");

                return "📊 **Latency Comparison:**\n" + string.Join("\n", providers);
            }

            return null; // not a recognized command
        }
    }
}
