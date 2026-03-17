using ArNir.Core.DTOs.Chat;
using ArNir.Core.DTOs.Intelligence;
using ArNir.Observability.Interfaces;
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
#pragma warning disable CS0618 // IAIInsightService is obsolete — bridge kept until migration completes
        private readonly IAIInsightService _aiInsightService;
#pragma warning restore CS0618
        private readonly IInsightEngineService _insightEngineService;
        private readonly IChatInsightService _chatInsightService;
        private readonly ILogger<IntelligenceController> _logger;
        private readonly IRagService _ragService;
        private readonly IAIInsightGenerator? _insightGenerator;

#pragma warning disable CS0618
        public IntelligenceController(IIntelligenceService intelligenceService,
                IExportService exportService,
                IAIInsightService aiInsightService,
                IChatInsightService chatInsightService,
                IInsightEngineService insightEngineService,
                IRagService ragService,
                ILogger<IntelligenceController> logger,
                IAIInsightGenerator? insightGenerator = null)
        {
            _intelligenceService = intelligenceService;
            _exportService = exportService;
            _aiInsightService = aiInsightService;
            _insightEngineService = insightEngineService;
            _chatInsightService = chatInsightService;
            _ragService = ragService;
            _logger = logger;
            _insightGenerator = insightGenerator;
        }
#pragma warning restore CS0618

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
            if (_insightGenerator != null)
            {
                var insights = await _insightGenerator.GenerateInsightsAsync(provider, startDate, endDate);
                return Ok(insights);
            }
#pragma warning disable CS0618
            var legacyInsights = await _aiInsightService.GenerateInsightsAsync(provider, startDate, endDate);
#pragma warning restore CS0618
            return Ok(legacyInsights);
        }

        //Old Code for chat endpoint - kept for reference
        //[HttpPost("chat")]
        //public async Task<IActionResult> Chat([FromBody] ChatRequestDto request)
        //{
        //    if (string.IsNullOrWhiteSpace(request?.Prompt))
        //        return BadRequest("Prompt is required.");

        //    try
        //    {
        //        //// 🧠 Use Insight Engine first
        //        //var response = await _insightEngineService.ProcessQueryAsync(request.Prompt);

        //        //// 🔁 Fallback to direct GPT if no insight
        //        //if (string.IsNullOrWhiteSpace(response) || response == "No insights generated.")
        //        //    response = await _chatInsightService.GenerateInsightAsync(request.Prompt);

        //        var response = await _insightEngineService.GenerateSummaryAsync(
        //                provider: null,
        //                startDate: null,
        //                endDate: null,
        //                userPrompt: request.Prompt
        //            );

        //        // fallback if needed
        //        if (string.IsNullOrWhiteSpace(response) || response == "No insights generated.")
        //        {
        //            var chatQuery = new ChatQueryDto
        //            {
        //                SessionId = Guid.NewGuid().ToString(),
        //                UserQuery = request.Prompt
        //            };
        //            var chatResponse = await _chatInsightService.ProcessUserQueryAsync(chatQuery);
        //            response = chatResponse.ResponseText;
        //        }

        //        return Ok(new { content = response });
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error processing chat request.");
        //        return StatusCode(500, "Error processing chat request.");
        //    }
        //}

        /// <summary>
        /// Primary intelligence chat endpoint.
        /// </summary>
        [HttpPost("chat")]
        public async Task<IActionResult> Chat([FromBody] ChatRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Query))
                return BadRequest(new { error = "Prompt cannot be empty." });

            try
            {
                var query = new ChatQueryDto
                {
                    SessionId = request.SessionId ?? Guid.NewGuid().ToString(),
                    UserQuery = request.Query
                };

                var result = await _chatInsightService.ProcessUserQueryAsync(query);

                return Ok(new
                {
                    responseText = result.ResponseText,
                    chart = result.Chart,
                    insightSummary = result.InsightSummary,
                    suggestedActions = result.SuggestedActions,
                    isError = result.IsError
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing chat request.");
                return StatusCode(500, new
                {
                    error = "Internal Server Error while processing chat query.",
                    details = ex.Message
                });
            }
        }

        /// <summary>
        /// Get related insights from RAG history (Semantic Recall Panel - Phase 7.2)
        /// </summary>
        [HttpPost("related")]
        public async Task<IActionResult> GetRelatedInsights([FromBody] RelatedQueryDto request)
        {
            try
            {
                var insights = await _ragService.GetRelatedInsightsAsync(request.Query, 5);
                return Ok(insights ?? Enumerable.Empty<RelatedInsightDto>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving related insights.");
                return StatusCode(500, new
                {
                    error = "Error retrieving related insights.",
                    details = ex.Message
                });
            }
        }

        /// <summary>
        /// Executes contextual backend action (e.g., Compare Models, View Trends).
        /// </summary>
        [HttpPost("action")]
        public async Task<IActionResult> ExecuteAction([FromBody] ActionRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Action))
                return BadRequest(new { error = "Action cannot be empty." });

            try
            {
                var result = await _chatInsightService.ExecuteActionAsync(request.Action);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing contextual action.");
                return StatusCode(500, new
                {
                    error = "Failed to execute contextual action.",
                    details = ex.Message
                });
            }
        }

    }
    // DTOs for request mapping
    public class ChatRequest
    {
        public string? SessionId { get; set; }

        // ✅ Use 'Query' instead of 'Prompt' to match frontend
        public string? Query { get; set; }

        // (Optional) Add this if you support contextual mode
        //public string? Context { get; set; }
    }

    public class RelatedQueryDto
    {
        public string Query { get; set; } = string.Empty;
    }

    public class ActionRequest
    {
        public string Action { get; set; } = string.Empty;
    }
}
