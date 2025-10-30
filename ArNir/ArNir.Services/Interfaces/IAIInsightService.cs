using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ArNir.Core.DTOs.Intelligence;

namespace ArNir.Services.Interfaces
{
    public interface IAIInsightService
    {
        /// <summary>
        /// Generates predictive AI insights based on latency/SLA trends.
        /// </summary>
        Task<List<AIInsightDto>> GenerateInsightsAsync(string? provider = null, DateTime? start = null, DateTime? end = null);
    }
}
