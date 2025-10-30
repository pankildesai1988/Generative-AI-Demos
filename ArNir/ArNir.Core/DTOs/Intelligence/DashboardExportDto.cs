using System;
using System.Collections.Generic;

namespace ArNir.Core.DTOs.Intelligence
{
    /// <summary>
    /// DTO used for exporting unified dashboard data.
    /// </summary>
    public class DashboardExportDto
    {
        public List<KpiMetricDto> Kpis { get; set; } = new();
        public List<ChartDataDto> Charts { get; set; } = new();
        public string? GptSummary { get; set; }
        public string? Provider { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}
