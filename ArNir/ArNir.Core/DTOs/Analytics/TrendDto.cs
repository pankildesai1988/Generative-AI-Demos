using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArNir.Core.DTOs.Analytics
{
    public class TrendDto
    {
        public DateTime Date { get; set; }
        public double AvgTotalLatencyMs { get; set; }
    }
}
