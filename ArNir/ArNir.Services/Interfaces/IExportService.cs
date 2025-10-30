using System;
using ArNir.Core.DTOs.Intelligence;

namespace ArNir.Services.Interfaces
{
    public interface IExportService
    {
        /// <summary>
        /// Exports the dashboard data to Excel format.
        /// </summary>
        (byte[] file, string contentType, string fileName) ExportToExcel(DashboardExportDto dto);

        /// <summary>
        /// Exports the dashboard data to CSV format.
        /// </summary>
        (byte[] file, string contentType, string fileName) ExportToCsv(DashboardExportDto dto);

        /// <summary>
        /// Exports the dashboard data to PDF format using QuestPDF.
        /// </summary>
        (byte[] file, string contentType, string fileName) ExportToPdf(DashboardExportDto dto);
    }
}
