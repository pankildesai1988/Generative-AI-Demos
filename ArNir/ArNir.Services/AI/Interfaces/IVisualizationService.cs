using ArNir.Core.DTOs.Analytics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArNir.Services.AI.Interfaces
{
    public interface IVisualizationService
    {
        //Task<ChartDto> BuildChartDto(object analyticsData);
        Task<ChartItemDto> BuildChartDto(object analyticsData);
    }
}
