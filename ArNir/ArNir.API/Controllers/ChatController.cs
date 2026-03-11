using ArNir.Core.DTOs.Chat;
using ArNir.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ArNir.API.Controllers
{
    [ApiController]
    [Route("api/chat")]
    public class ChatController : ControllerBase
    {
        private readonly IChatInsightService _chatService;

        public ChatController(IChatInsightService chatService)
        {
            _chatService = chatService;
        }

        [HttpPost("query")]
        public async Task<IActionResult> Query([FromBody] ChatQueryDto query)
            => Ok(await _chatService.ProcessUserQueryAsync(query));

        [HttpGet("context/{sessionId}")]
        public async Task<IActionResult> Context(string sessionId)
            => Ok(await _chatService.GetSessionContextAsync(sessionId));
    }

}
