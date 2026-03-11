using System.Collections.Generic;
using ArNir.Core.DTOs.Analytics;

namespace ArNir.Core.DTOs.Chat
{
    public class ChartDto
    {
        public string Title { get; set; } = string.Empty;
        public string Type { get; set; } = "bar";
        public List<ChartDataPointDto> Data { get; set; } = new();
    }

    public class ChartDataPointDto
    {
        public string Label { get; set; } = string.Empty;
        public double Value { get; set; }
        public string? Description { get; set; }
        public string? Category { get; set; }
        public string? Color { get; set; }
    }

    /// <summary>
    /// Extension method to map from Analytics.ChartItemDto to Chat.ChartDto
    /// </summary>
    public static class ChartDtoMapper
    {
        public static ChartDto ToChatChart(this ChartItemDto analyticsChart)
        {
            var chatChart = new ChartDto
            {
                Title = analyticsChart.Title,
                Type = analyticsChart.Type
            };

            foreach (var pt in analyticsChart.Data)
            {
                chatChart.Data.Add(new ChartDataPointDto
                {
                    Label = pt.Label,
                    Value = pt.Value,
                    Description = pt.Description,
                    Category = pt.Category,
                    Color = pt.Color
                });
            }

            return chatChart;
        }
    }
}
