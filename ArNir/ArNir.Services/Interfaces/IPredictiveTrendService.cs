using System;
using System.Threading.Tasks;
using ArNir.Core.DTOs.Intelligence;

namespace ArNir.Services.Interfaces
{
    public interface IPredictiveTrendService
    {
        Task<ChartDataDto?> GetForecastAsync(string? provider = null, DateTime? start = null, DateTime? end = null);
    }
}
