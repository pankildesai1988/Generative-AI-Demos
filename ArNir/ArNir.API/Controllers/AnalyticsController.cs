using ArNir.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ArNir.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AnalyticsController : ControllerBase
    {
        private readonly IRagService _ragService;
        public AnalyticsController(IRagService ragService) => _ragService = ragService;

        [HttpGet("average-latencies")]
        public async Task<IActionResult> GetAverageLatencies([FromQuery] DateTime? start, [FromQuery] DateTime? end)
            => Ok(await _ragService.GetAverageLatenciesAsync(start, end, null, null));

        [HttpGet("provider")]
        public async Task<IActionResult> GetProviderAnalytics(
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] string? promptStyle)
        {
            var result = await _ragService.GetProviderAnalyticsAsync(startDate, endDate, promptStyle);
            return Ok(result);
        }


        [HttpGet("sla-compliance")]
        public async Task<IActionResult> GetSlaCompliance()
            => Ok(await _ragService.GetSlaComplianceAsync(null, null, null, null));

        [HttpGet("prompt-style-usage")]
        public async Task<IActionResult> GetPromptStyleUsage()
            => Ok(await _ragService.GetPromptStyleUsageAsync(null, null, null, null));

        [HttpGet("trends")]
        public async Task<IActionResult> GetTrends([FromQuery] DateTime start, [FromQuery] DateTime end)
            => Ok(await _ragService.GetTrendsAsync(start, end, null, null));
    }
}
