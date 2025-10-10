using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArNir.Core.DTOs.Feedback
{
    public class FeedbackDto
    {
        public int HistoryId { get; set; }      // Link to RagComparisonHistory
        public int Rating { get; set; }         // 1–5 stars
        public string? Comments { get; set; }
    }
}
