using System.Collections.Generic;

namespace ArNir.Core.DTOs.Analytics
{
    /// <summary>
    /// Represents a single visualization/chart returned from AI insights.
    /// Supports both numeric and textual summaries.
    /// </summary>
    public class ChartItemDto
    {
        /// <summary>
        /// The title of the chart (e.g., “SLA Trends”, “Latency Breakdown”)
        /// </summary>
        public string Title { get; set; } = "AI Analytics Chart";

        /// <summary>
        /// The chart type — e.g., “bar”, “line”, “text”, “bar+text”
        /// </summary>
        public string Type { get; set; } = "bar";

        /// <summary>
        /// The dataset for this chart.
        /// </summary>
        public List<ChartPointDto> Data { get; set; } = new();
    }

    /// <summary>
    /// Represents a single datapoint (bar segment, line node, or text item).
    /// </summary>
    public class ChartPointDto
    {
        /// <summary>
        /// The label for the datapoint (e.g., “OpenAI”, “Gemini”, “Claude”)
        /// </summary>
        public string Label { get; set; } = string.Empty;

        /// <summary>
        /// The numeric value, if applicable (e.g., latency, SLA %)
        /// </summary>
        public double Value { get; set; } = 0.0;

        /// <summary>
        /// The textual description or AI insight linked to this datapoint.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// The optional category or metric group name.
        /// </summary>
        public string? Category { get; set; }

        /// <summary>
        /// Optional color tag (for frontend chart rendering).
        /// </summary>
        public string? Color { get; set; }
    }
}
