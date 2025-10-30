using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ArNir.Core.DTOs.Intelligence;
using ArNir.Services.Interfaces;
using ClosedXML.Excel;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace ArNir.Services
{
    public class ExportService : IExportService
    {
        public (byte[] file, string contentType, string fileName) ExportToExcel(DashboardExportDto dto)
        {
            using var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add("Dashboard Export");

            ws.Cell(1, 1).Value = "Metric";
            ws.Cell(1, 2).Value = "Value";
            ws.Cell(1, 3).Value = "Unit";

            for (int i = 0; i < dto.Kpis.Count; i++)
            {
                ws.Cell(i + 2, 1).Value = dto.Kpis[i].Label;
                ws.Cell(i + 2, 2).Value = dto.Kpis[i].Value;
                ws.Cell(i + 2, 3).Value = dto.Kpis[i].Unit;
            }

            using var ms = new MemoryStream();
            wb.SaveAs(ms);
            return (ms.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"Dashboard_Export_{DateTime.UtcNow:yyyyMMddHHmm}.xlsx");
        }

        public (byte[] file, string contentType, string fileName) ExportToCsv(DashboardExportDto dto)
        {
            var lines = new List<string> { "Metric,Value,Unit" };
            lines.AddRange(dto.Kpis.Select(x => $"{x.Label},{x.Value},{x.Unit}"));
            var csv = string.Join(Environment.NewLine, lines);
            return (System.Text.Encoding.UTF8.GetBytes(csv),
                "text/csv",
                $"Dashboard_Export_{DateTime.UtcNow:yyyyMMddHHmm}.csv");
        }

        public (byte[] file, string contentType, string fileName) ExportToPdf(DashboardExportDto dto)
        {
            var stream = new MemoryStream();

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(30);

                    // --- Header ---
                    page.Header().Column(col =>
                    {
                        col.Item().Text("Unified Intelligence Dashboard Export")
                            .SemiBold().FontSize(16).AlignCenter();

                        col.Item().Text(text =>
                        {
                            text.Span("Provider: ").SemiBold();
                            text.Span(string.IsNullOrEmpty(dto.Provider) ? "All Providers" : dto.Provider);
                        });

                        col.Item().Text(text =>
                        {
                            text.Span("Date Range: ").SemiBold();
                            text.Span(
                                $"{(dto.StartDate?.ToString("yyyy-MM-dd") ?? "N/A")} → {(dto.EndDate?.ToString("yyyy-MM-dd") ?? "N/A")}");
                        });

                        if (!string.IsNullOrEmpty(dto.GptSummary))
                        {
                            col.Item().PaddingTop(5).Text($"AI Summary: {dto.GptSummary}")
                                .FontSize(10).Italic();
                        }
                    });

                    // --- KPI Table ---
                    page.Content().PaddingVertical(10).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(2);
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                        });

                        // Table Header
                        table.Header(header =>
                        {
                            header.Cell().Element(CellHeader).Text("Metric");
                            header.Cell().Element(CellHeader).Text("Value");
                            header.Cell().Element(CellHeader).Text("Unit");
                        });

                        static IContainer CellHeader(IContainer container) =>
                            container.DefaultTextStyle(x => x.SemiBold())
                                .Padding(5)
                                .Background(Colors.Grey.Lighten3)
                                .BorderBottom(1)
                                .BorderColor(Colors.Grey.Lighten2);

                        // Table Rows
                        foreach (var kpi in dto.Kpis)
                        {
                            table.Cell().Element(CellData).Text(kpi.Label);
                            table.Cell().Element(CellData).Text(kpi.Value.ToString("N2"));
                            table.Cell().Element(CellData).Text(kpi.Unit ?? "");
                        }

                        static IContainer CellData(IContainer container) =>
                            container.Padding(5).BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2);
                    });

                    // --- Footer ---
                    page.Footer().AlignCenter().Text(
                        $"Generated on {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC"
                    );
                });
            });

            document.GeneratePdf(stream);
            return (stream.ToArray(), "application/pdf", $"Dashboard_Export_{DateTime.UtcNow:yyyyMMddHHmm}.pdf");
        }
    }
}
