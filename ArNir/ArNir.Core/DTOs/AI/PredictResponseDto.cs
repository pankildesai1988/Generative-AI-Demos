using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArNir.Core.DTOs.AI
{
    public class PredictResponseDto
    {
        public List<double>? PredictedValues { get; set; }
        public string? TrendSummary { get; set; }
        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    }
}
