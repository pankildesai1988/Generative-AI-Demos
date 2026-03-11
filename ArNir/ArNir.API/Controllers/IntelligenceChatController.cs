using Microsoft.AspNetCore.Mvc;
using ArNir.Services.Interfaces;
using ArNir.Core.DTOs.Chat;

namespace ArNir.Api.Controllers
{
    //[ApiController]
    //[Route("api/intelligence/chatgpy")]
    //public class IntelligenceChatController : ControllerBase
    //{
    //    private readonly IChatInsightService _chatService;

    //    public IntelligenceChatController(IChatInsightService chatService)
    //    {
    //        _chatService = chatService;
    //    }

    //    [HttpPost]
    //    public async Task<IActionResult> Post([FromBody] ChatRequest request)
    //    {
    //        if (string.IsNullOrWhiteSpace(request.Prompt))
    //            return BadRequest("Prompt cannot be empty.");

    //        // 🧠 Use new method with DTO
    //        var query = new ChatQueryDto
    //        {
    //            SessionId = Guid.NewGuid().ToString(),
    //            UserQuery = request.Prompt
    //        };

    //        var response = await _chatService.ProcessUserQueryAsync(query);
    //        return Ok(new
    //        {
    //            insight = response.ResponseText,
    //            chart = response.Chart,
    //            summary = response.InsightSummary
    //        });
    //    }
    //}

    //public class ChatRequest
    //{
    //    public string Prompt { get; set; } = string.Empty;
    //}
}
