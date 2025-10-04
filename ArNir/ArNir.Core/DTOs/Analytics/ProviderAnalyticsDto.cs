using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArNir.Core.DTOs.Analytics
{
    public class ProviderAnalyticsDto
    {
        public string Provider { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public double AvgRetrievalLatencyMs { get; set; }
        public double AvgLlmLatencyMs { get; set; }
        public double AvgTotalLatencyMs { get; set; }
        public int TotalRuns { get; set; }
        public int WithinSlaCount { get; set; }

        // ✅ Explicitly set from RagService to ensure JSON serialization works
        public double SlaComplianceRate { get; set; }
    }
}
