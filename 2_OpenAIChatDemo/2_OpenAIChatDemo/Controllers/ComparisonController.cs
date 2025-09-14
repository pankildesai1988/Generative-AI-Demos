using Microsoft.AspNetCore.Mvc;
using _2_OpenAIChatDemo.DTOs;
using _2_OpenAIChatDemo.Services;

namespace _2_OpenAIChatDemo.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ComparisonController : ControllerBase
    {
        private readonly IComparisonService _comparisonService;
        private readonly ILogger<ComparisonController> _logger;

        public ComparisonController(IComparisonService comparisonService, ILogger<ComparisonController> logger)
        {
            _comparisonService = comparisonService;
            _logger = logger;
        }

        /// <summary>
        /// Run a comparison across multiple models
        /// </summary>
        [HttpPost("run")]
        public async Task<IActionResult> RunComparison([FromBody] ComparisonRequestDto request)
        {
            try
            {
                var result = await _comparisonService.RunComparisonAsync(request);

                return Ok(new
                {
                    success = true,
                    data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error running comparison");
                return BadRequest(new
                {
                    success = false,
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Get a comparison result by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetComparison(int id)
        {
            try
            {
                var result = await _comparisonService.GetComparisonAsync(id);

                if (result == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        error = "Comparison not found"
                    });
                }

                return Ok(new
                {
                    success = true,
                    data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching comparison {ComparisonId}", id);
                return BadRequest(new
                {
                    success = false,
                    error = ex.Message
                });
            }
        }

        [HttpGet("history")]
        public async Task<IActionResult> GetHistory()
        {
            try
            {
                var history = await _comparisonService.GetHistoryAsync(50);
                return Ok(new { success = true, data = history });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching history");
                return BadRequest(new { success = false, error = ex.Message });
            }
        }

        [HttpGet("history/{id}")]
        public async Task<IActionResult> GetHistoryById(int id)
        {
            try
            {
                var comparison = await _comparisonService.GetHistoryByIdAsync(id);

                if (comparison == null)
                {
                    return NotFound(new { success = false, error = "Comparison not found" });
                }

                return Ok(new { success = true, data = comparison });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching comparison {ComparisonId}", id);
                return BadRequest(new { success = false, error = ex.Message });
            }
        }

    }
}
