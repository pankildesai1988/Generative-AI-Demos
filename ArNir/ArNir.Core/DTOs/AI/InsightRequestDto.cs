using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArNir.Core.DTOs.AI
{
    public class InsightRequestDto
    {
        public string? Provider { get; set; }
        public string? Model { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? MetricType { get; set; } // e.g. SLA, Latency, Feedback
        public string? DataJson { get; set; }   // Raw aggregated data as JSON
    }
}
