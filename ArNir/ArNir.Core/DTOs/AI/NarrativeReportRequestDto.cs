using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArNir.Core.DTOs.AI
{
    public class NarrativeReportRequestDto
    {
        public string? Provider { get; set; }
        public string? MetricType { get; set; }
        public string? Insights { get; set; }
        public List<string>? Anomalies { get; set; }
        public List<double>? Predictions { get; set; }
        public string? TrendSummary { get; set; }
    }
}
