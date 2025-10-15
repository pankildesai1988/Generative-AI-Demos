using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArNir.Core.DTOs.AI
{
    public class PredictRequestDto
    {
        public string? Provider { get; set; }
        public string? MetricType { get; set; } // e.g., SLA, Latency, Feedback
        public List<double>? Values { get; set; }
        public int ForecastPoints { get; set; } = 3;
        public bool UseGPT { get; set; } = false;
    }

}
