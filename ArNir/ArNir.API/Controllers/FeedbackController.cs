using ArNir.Core.DTOs.Feedback;
using ArNir.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ArNir.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FeedbackController : ControllerBase
    {
        private readonly IFeedbackService _feedbackService;

        public FeedbackController(IFeedbackService feedbackService)
        {
            _feedbackService = feedbackService;
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] FeedbackDto dto)
        {
            if (dto == null)
                return BadRequest("Invalid feedback data.");

            var result = await _feedbackService.AddFeedbackAsync(dto);
            return Ok(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _feedbackService.GetAllAsync();
            return Ok(result);
        }

        [HttpGet("average")]
        public async Task<IActionResult> GetAverageRating()
        {
            var avg = await _feedbackService.GetAverageRatingAsync();
            return Ok(new { averageRating = avg });
        }
    }
}
