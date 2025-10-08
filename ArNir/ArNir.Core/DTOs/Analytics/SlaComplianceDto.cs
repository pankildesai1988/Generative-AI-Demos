using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArNir.Core.DTOs.Analytics
{
    public class SlaComplianceDto
    {
        public string PromptStyle { get; set; } = string.Empty;
        public int TotalRuns { get; set; }
        public int WithinSlaCount { get; set; }

        // ✅ Explicitly set from RagService
        public double ComplianceRate { get; set; }
    }
}
