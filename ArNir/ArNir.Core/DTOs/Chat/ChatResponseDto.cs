using ArNir.Core.DTOs.Analytics;

namespace ArNir.Core.DTOs.Chat
{
    public class ChatResponseDto
    {
        // Replaces “Insight” naming mismatch
        public string ResponseText { get; set; } = string.Empty;

        // Optional summary field for short insights
        public string? InsightSummary { get; set; }

        // Inline chart or KPI data
        public ChartItemDto? Chart { get; set; }

        // Suggested actions (converted to array)
        public string[] SuggestedActions { get; set; } = Array.Empty<string>();

        // Add missing IsError flag
        public bool IsError { get; set; } = false;
    }
}
