using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArNir.Core.DTOs.Analytics
{
    public class SlaComplianceDto
    {
        public int TotalRuns { get; set; }
        public int WithinSlaCount { get; set; }
        public double ComplianceRate => TotalRuns == 0 ? 0 : (double)WithinSlaCount / TotalRuns * 100;
    }
}
