using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArNir.Core.DTOs.Analytics
{
    public class AvgLatencyDto
    {
        public double AvgRetrievalLatencyMs { get; set; }
        public double AvgLlmLatencyMs { get; set; }
        public double AvgTotalLatencyMs { get; set; }
    }
}
