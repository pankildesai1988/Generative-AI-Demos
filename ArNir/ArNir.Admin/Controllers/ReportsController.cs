using ArNir.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using ClosedXML.Excel;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ArNir.Admin.Controllers
{
    public class ReportsController : Controller
    {
        private readonly IRagService _ragService;

        public ReportsController(IRagService ragService)
        {
            _ragService = ragService;
        }

        public async Task<IActionResult> Index(DateTime? startDate, DateTime? endDate)
        {
            startDate ??= DateTime.MinValue;
            endDate ??= DateTime.MaxValue;

            var analytics = await _ragService.GetProviderAnalyticsAsync(startDate, endDate, null);
            return View(analytics?.Data ?? Enumerable.Empty<object>());
        }

        [HttpGet]
        public async Task<IActionResult> ExportProviderAnalytics(DateTime? startDate, DateTime? endDate, string format = "excel")
        {
            startDate ??= DateTime.MinValue;
            endDate ??= DateTime.MaxValue;

            var analytics = await _ragService.GetProviderAnalyticsAsync(startDate, endDate, null);

            if (analytics?.Data == null || !analytics.Data.Any())
                return BadRequest("No analytics data found.");

            if (format.Equals("excel", StringComparison.OrdinalIgnoreCase))
            {
                using var workbook = new XLWorkbook();
                var worksheet = workbook.Worksheets.Add("Provider Analytics");

                // Header row
                worksheet.Cell(1, 1).Value = "Provider";
                worksheet.Cell(1, 2).Value = "Model";
                worksheet.Cell(1, 3).Value = "Avg Latency (ms)";
                worksheet.Cell(1, 4).Value = "SLA (%)";
                worksheet.Cell(1, 5).Value = "Avg Rating";
                worksheet.Cell(1, 6).Value = "Total Runs";

                int row = 2;
                foreach (var item in analytics.Data)
                {
                    worksheet.Cell(row, 1).Value = item.Provider;
                    worksheet.Cell(row, 2).Value = item.Model;
                    worksheet.Cell(row, 3).Value = Math.Round(item.AvgTotalLatencyMs, 2);
                    worksheet.Cell(row, 4).Value = Math.Round(item.SlaComplianceRate, 2);
                    worksheet.Cell(row, 5).Value = Math.Round(item.AvgRating, 2);
                    worksheet.Cell(row, 6).Value = item.TotalRuns;
                    row++;
                }

                worksheet.Columns().AdjustToContents();

                using var stream = new MemoryStream();
                workbook.SaveAs(stream);
                stream.Seek(0, SeekOrigin.Begin);

                var fileName = $"Provider_Analytics_{DateTime.Now:yyyyMMddHHmm}.xlsx";
                return File(stream.ToArray(),
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    fileName);
            }

            // CSV Fallback
            var csv = "Provider,Model,AvgLatency,SLA,AvgRating,TotalRuns\n";
            foreach (var item in analytics.Data)
            {
                csv += $"{item.Provider},{item.Model},{item.AvgTotalLatencyMs:F2},{item.SlaComplianceRate:F2},{item.AvgRating:F2},{item.TotalRuns}\n";
            }

            var csvBytes = System.Text.Encoding.UTF8.GetBytes(csv);
            var csvFileName = $"Provider_Analytics_{DateTime.Now:yyyyMMddHHmm}.csv";
            return File(csvBytes, "text/csv", csvFileName);
        }
    }
}
