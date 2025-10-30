using Microsoft.AspNetCore.Mvc;
using ArNir.Services.Interfaces;

namespace ArNir.Api.Controllers
{
    [ApiController]
    [Route("api/intelligence/chat")]
    public class IntelligenceChatController : ControllerBase
    {
        private readonly IChatInsightService _chatService;

        public IntelligenceChatController(IChatInsightService chatService)
        {
            _chatService = chatService;
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] ChatRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Prompt))
                return BadRequest("Prompt cannot be empty.");

            var response = await _chatService.GenerateInsightAsync(request.Prompt);
            return Ok(new { insight = response });
        }
    }

    public class ChatRequest
    {
        public string Prompt { get; set; } = string.Empty;
    }
}
