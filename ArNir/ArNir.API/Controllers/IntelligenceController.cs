using ArNir.Services;
using ArNir.Services.AI;
using ArNir.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ArNir.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class IntelligenceController : ControllerBase
    {
        private readonly IIntelligenceService _intelligenceService;
        private readonly IExportService _exportService;
        private readonly IAIInsightService _aiInsightService;

        public IntelligenceController(IIntelligenceService intelligenceService, IExportService exportService, IAIInsightService aiInsightService)
        {
            _intelligenceService = intelligenceService;
            _exportService = exportService;
            _aiInsightService = aiInsightService;
        }

        /// <summary>
        /// Unified endpoint for dashboard data aggregation
        /// </summary>
        [HttpGet("dashboard")]
        public async Task<IActionResult> GetDashboard(
            [FromQuery] string? provider,
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate)
        {
            var result = await _intelligenceService.GetUnifiedDashboardAsync(provider, startDate, endDate);
            return Ok(result);
        }


        [HttpGet("export")]
        public async Task<IActionResult> ExportDashboard(
            [FromQuery] string? provider,
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] string format = "excel")
        {
            try
            {
                var exportData = await _intelligenceService.GetDashboardExportAsync(provider, startDate, endDate);
                if (exportData == null || !exportData.Kpis.Any())
                    return BadRequest("No data available for export.");


                byte[] fileBytes;
                string fileType, fileName;

                switch (format.ToLower())
                {
                    case "csv":
                        (fileBytes, fileType, fileName) = _exportService.ExportToCsv(exportData);
                        break;

                    case "pdf":
                        (fileBytes, fileType, fileName) = _exportService.ExportToPdf(exportData);
                        break;

                    default:
                        (fileBytes, fileType, fileName) = _exportService.ExportToExcel(exportData);
                        break;
                }

                return File(fileBytes, fileType, fileName);
            }
            catch (Exception ex)
            {
                //_logger.LogError(ex, "Export failed.");
                return StatusCode(500, "Error exporting dashboard data.");
            }
        }

        [HttpGet("insights")]
        public async Task<IActionResult> GetInsights([FromQuery] string? provider, [FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
        {
            var insights = await _aiInsightService.GenerateInsightsAsync(provider, startDate, endDate);
            return Ok(insights);
        }

        [HttpPost("chat")]
        public async Task<IActionResult> Chat([FromBody] ChatRequestDto request)
        {
            if (string.IsNullOrWhiteSpace(request?.Prompt))
                return BadRequest("Prompt is required.");

            try
            {
                // 🧠 Use Insight Engine first
                var response = await _insightEngineService.ProcessQueryAsync(request.Prompt);

                // 🔁 Fallback to direct GPT if no insight
                if (string.IsNullOrWhiteSpace(response) || response == "No insights generated.")
                    response = await _chatInsightService.GenerateInsightAsync(request.Prompt);

                return Ok(new { content = response });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing chat request.");
                return StatusCode(500, "Error processing chat request.");
            }
        }

    }
}
