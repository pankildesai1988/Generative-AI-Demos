using System;
using System.Collections.Generic;

namespace ArNir.Core.DTOs.Intelligence
{
    /// <summary>
    /// DTO representing unified dashboard analytics data
    /// (used by frontend intelligence dashboard API).
    /// </summary>
    public class UnifiedDashboardDto
    {
        public List<KpiMetricDto> Kpis { get; set; } = new();
        public List<ChartDataDto> Charts { get; set; } = new();
        public List<AlertDto> ActiveAlerts { get; set; } = new();
        public string? GptSummary { get; set; }
        public List<AIInsightDto> AiInsights { get; set; } = new();
    }

    /// <summary>
    /// Individual KPI metric with label, value, and unit.
    /// </summary>
    public class KpiMetricDto
    {
        public string Label { get; set; } = string.Empty;
        public double Value { get; set; }
        public string? Unit { get; set; }
    }

    /// <summary>
    /// Chart-level data used for visualization.
    /// </summary>
    public class ChartDataDto
    {
        public string Title { get; set; } = string.Empty;
        public List<ChartSeriesItemDto> Data { get; set; } = new();
    }

    /// <summary>
     /// Unified alert structure for latency/SLA notifications.
     /// </summary>
    public class AlertDto
    {
        public string Type { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>
    /// Individual chart data entry (per date or provider).
    /// </summary>
    public class ChartSeriesItemDto
    {
        public DateTime Date { get; set; }
        public string? Provider { get; set; }
        public double? AvgLatency { get; set; }
        public double? Predicted { get; set; }

        // ✅ New fields for forecast confidence intervals
        public double? LowerBound { get; set; }
        public double? UpperBound { get; set; }

        // Optional SLA or metric value if reused for multiple chart types
        public double? SlaValue { get; set; }
    }

    /// <summary>
    /// Active alert information for SLA/latency.
    /// </summary>
    public class AlertItemDto
    {
        public string Type { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
