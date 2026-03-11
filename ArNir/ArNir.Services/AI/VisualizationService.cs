using ArNir.Core.DTOs.Analytics;
using ArNir.Services.AI.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ArNir.Services.AI
{
    public class VisualizationService : IVisualizationService
    {
        public Task<ChartItemDto> BuildChartDto(object analyticsData)
        {
            var chart = new ChartItemDto
            {
                Title = "AI-Generated Analytics",
                Type = "bar+text",
                Data = new List<ChartPointDto>()
            };

            switch (analyticsData)
            {
                case null:
                    chart.Type = "text";
                    chart.Data.Add(new ChartPointDto
                    {
                        Label = "No Data",
                        Description = "No analytics results available.",
                        Value = 0
                    });
                    break;

                case IEnumerable<object> enumerable:
                    int i = 1;
                    foreach (var item in enumerable)
                    {
                        chart.Data.Add(new ChartPointDto
                        {
                            Label = $"Item {i++}",
                            Description = item.ToString(),
                            Value = 1,
                            Category = "Dataset"
                        });
                    }
                    break;

                case string text:
                    chart.Type = "text";
                    chart.Data.Add(new ChartPointDto
                    {
                        Label = "AI Insight",
                        Description = text,
                        Value = 1,
                        Category = "Insight"
                    });
                    break;

                default:
                    chart.Type = "text";
                    chart.Data.Add(new ChartPointDto
                    {
                        Label = analyticsData.GetType().Name,
                        Description = analyticsData.ToString(),
                        Value = 0
                    });
                    break;
            }

            return Task.FromResult(chart);
        }
    }
}
