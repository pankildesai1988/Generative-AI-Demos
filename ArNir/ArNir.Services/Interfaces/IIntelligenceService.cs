using System;
using System.Threading.Tasks;
using ArNir.Core.DTOs.Intelligence;

namespace ArNir.Services.Interfaces
{
    public interface IIntelligenceService
    {
        /// <summary>
        /// Gets the unified dashboard metrics, charts, and alerts.
        /// </summary>
        Task<UnifiedDashboardDto> GetUnifiedDashboardAsync(string? provider, DateTime? startDate, DateTime? endDate);

        /// <summary>
        /// Prepares a single dashboard export payload for the given filters.
        /// </summary>
        Task<DashboardExportDto> GetDashboardExportAsync(string? provider, DateTime? startDate, DateTime? endDate);
    }
}
