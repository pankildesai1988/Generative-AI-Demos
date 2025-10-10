using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArNir.Core.Entities
{
    public class Feedback
    {
        public int Id { get; set; }
        public int HistoryId { get; set; }          // FK to RagComparisonHistory
        public int Rating { get; set; }             // 1–5 stars
        public string? Comments { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
