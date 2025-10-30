using System;

namespace ArNir.Core.DTOs.Intelligence
{
    public class InsightItemDto
    {
        public string Title { get; set; } = string.Empty;
        public string InsightText { get; set; } = string.Empty;
        public string Severity { get; set; } = "info";
        public DateTime GeneratedAt { get; set; }
    }
}
