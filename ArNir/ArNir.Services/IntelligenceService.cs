using ArNir.Core.DTOs.Intelligence;
using ArNir.Data;
using ArNir.Services.AI;
using ArNir.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
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

        public IntelligenceService(
            IAnalyticsService analyticsService,
            IInsightEngineService insightEngineService,
            IPredictiveTrendService predictiveTrendService,
            INotificationService notificationService,
            ArNirDbContext context,
            IAIInsightService aiInsightService)
        {
            _analyticsService = analyticsService;
            _insightEngineService = insightEngineService;
            _predictiveTrendService = predictiveTrendService;
            _notificationService = notificationService;
            _context = context;
            _aiInsightService = aiInsightService;
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

    }
}
