using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ArNir.Core.DTOs.Intelligence;

namespace ArNir.Services.Interfaces
{
    public interface IExportHistoryService
    {
        Task LogExportAsync(string? userName, string? provider, string? format, DateTime? start, DateTime? end);
        Task<List<ExportHistoryDto>> GetExportHistoryAsync(string? userName = null);
    }
}
