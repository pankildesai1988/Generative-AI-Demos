using System;
using System.Collections.Generic;

namespace ArNir.Core.DTOs.Intelligence
{
    public class IntelligenceDashboardDto
    {
        public List<KpiMetricDto> Kpis { get; set; } = new();
        public List<ChartDataDto> Charts { get; set; } = new();
        public string GptSummary { get; set; } = string.Empty;
        public List<AlertDto> ActiveAlerts { get; set; } = new();
    }
}
