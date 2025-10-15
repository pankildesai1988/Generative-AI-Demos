using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArNir.Core.DTOs.AI
{
    public class InsightResponseDto
    {
        public string Summary { get; set; } = string.Empty;
        public List<string>? Anomalies { get; set; }
        public List<string>? Recommendations { get; set; }
        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    }
}
