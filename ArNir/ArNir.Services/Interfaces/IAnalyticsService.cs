using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ArNir.Core.DTOs.Intelligence;

namespace ArNir.Services.Interfaces
{
    public interface IAnalyticsService
    {
        Task<List<KpiMetricDto>> GetKpisAsync(string? provider = null, DateTime? start = null, DateTime? end = null);
        Task<List<ChartDataDto>> GetChartsAsync(string? provider = null, DateTime? start = null, DateTime? end = null);
    }
}
