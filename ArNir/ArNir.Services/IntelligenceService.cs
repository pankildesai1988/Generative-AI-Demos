using ArNir.Core.DTOs.Intelligence;
using ArNir.Data;
using ArNir.Services.AI;
using ArNir.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ArNir.Services
{
    public class IntelligenceService : IIntelligenceService
    {
        private readonly IAnalyticsService _analyticsService;
        private readonly IInsightEngineService _insightEngineService;
        private readonly IPredictiveTrendService _predictiveTrendService;
        private readonly INotificationService _notificationService;
        private readonly ArNirDbContext _context;
        private readonly IAIInsightService _aiInsightService;
        private readonly IRagService _ragService;
        private readonly ILogger<IntelligenceService> _logger;
        public IntelligenceService(
            IAnalyticsService analyticsService,
            IInsightEngineService insightEngineService,
            IPredictiveTrendService predictiveTrendService,
            INotificationService notificationService,
            ArNirDbContext context,
            IAIInsightService aiInsightService,
            IRagService ragService,
            ILogger<IntelligenceService> logger)
        {
            _analyticsService = analyticsService;
            _insightEngineService = insightEngineService;
            _predictiveTrendService = predictiveTrendService;
            _notificationService = notificationService;
            _context = context;
            _aiInsightService = aiInsightService;
            _ragService = ragService;
            _logger = logger;
        }

        /// <summary>
        /// Aggregates KPIs, chart data, GPT summary, and active alerts
        /// into a unified dashboard DTO.
        /// </summary>
        public async Task<UnifiedDashboardDto> GetUnifiedDashboardAsync(string? provider, DateTime? start, DateTime? end)
        {
            var dto = new UnifiedDashboardDto();

            // Existing logic for KPIs, charts, alerts, GPT summary...
            dto.Kpis = await _analyticsService.GetKpisAsync(provider, start, end);
            var charts = await _analyticsService.GetChartsAsync(provider, start, end);


            // 🧠 Add Forecast Chart from Predictive Trend Service
            var forecastChart = await _predictiveTrendService.GetForecastAsync(provider, start, end);
            if (forecastChart != null && forecastChart.Data.Any())
                charts.Add(forecastChart);

            dto.Charts = charts;

            dto.ActiveAlerts = await _notificationService.GetActiveAlertsAsync(provider);
            dto.GptSummary = await _insightEngineService.GenerateSummaryAsync(provider, start, end);

            // 🧠 NEW — Generate AI insights (latency & anomaly patterns)
            dto.AiInsights = await _aiInsightService.GenerateInsightsAsync(provider, start, end);

            return dto;
        }


        public async Task<DashboardExportDto> GetDashboardExportAsync(string? provider, DateTime? startDate, DateTime? endDate)
        {
            var dashboard = await GetUnifiedDashboardAsync(provider, startDate, endDate);

            return new DashboardExportDto
            {
                Provider = provider,
                StartDate = startDate,
                EndDate = endDate,
                Kpis = dashboard.Kpis ?? new(),
                Charts = dashboard.Charts ?? new(),
                GptSummary = dashboard.GptSummary
            };
        }


        #region === Phase 7.2 – Semantic Recall (Keyword Mode) ===

        /// <summary>
        /// Fetch related insights by matching prompt text with historical RAG queries/answers.
        /// Fallback keyword mode (no embedding vector required yet).
        /// </summary>
        public async Task<IEnumerable<RelatedInsightDto>> GetRelatedInsightsAsync(string prompt, int topK = 5)
        {
            if (string.IsNullOrWhiteSpace(prompt))
                return Enumerable.Empty<RelatedInsightDto>();

            try
            {
                // Broader fuzzy matching (ILIKE across multiple fields)
                var results = await _context.RagComparisonHistories
                    .Where(x =>
                        (!string.IsNullOrEmpty(x.UserQuery) && EF.Functions.ILike(x.UserQuery, $"%{prompt}%")) ||
                        (!string.IsNullOrEmpty(x.RagAnswer) && EF.Functions.ILike(x.RagAnswer, $"%{prompt}%")) ||
                        (!string.IsNullOrEmpty(x.Provider) && EF.Functions.ILike(x.Provider, $"%{prompt}%")) ||
                        (!string.IsNullOrEmpty(x.Model) && EF.Functions.ILike(x.Model, $"%{prompt}%"))
                    )
                    .OrderByDescending(x => x.CreatedAt)
                    .Take(topK)
                    .Select(x => new RelatedInsightDto
                    {
                        Summary = x.UserQuery ?? x.RagAnswer ?? "(No summary)",
                        CreatedAt = x.CreatedAt,
                        Source = $"{x.Provider} ({x.Model})"
                    })
                    .ToListAsync();

                // Fallback – last N records if no match
                if (results.Count == 0)
                {
                    results = await _context.RagComparisonHistories
                        .OrderByDescending(x => x.CreatedAt)
                        .Take(topK)
                        .Select(x => new RelatedInsightDto
                        {
                            Summary = x.UserQuery ?? x.RagAnswer ?? "(No summary)",
                            CreatedAt = x.CreatedAt,
                            Source = $"{x.Provider} ({x.Model})"
                        })
                        .ToListAsync();
                }

                _logger.LogInformation("Semantic recall returned {Count} insights for '{Prompt}'", results.Count, prompt);
                return results;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving related insights for prompt: {Prompt}", prompt);
                return Enumerable.Empty<RelatedInsightDto>();
            }
        }

        #endregion
    }
}   