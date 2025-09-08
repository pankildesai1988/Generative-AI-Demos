using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using _2_OpenAIChatDemo.DTOs;
using _2_OpenAIChatDemo.Services;

namespace _2_OpenAIChatDemo.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChatController : ControllerBase
    {
        private readonly IChatHistoryService _historyService;
        private readonly IOpenAiService _openAiService;

        public ChatController(IChatHistoryService historyService, IOpenAiService openAiService)
        {
            _historyService = historyService;
            _openAiService = openAiService;
        }

        /// <summary>
        /// Non-streaming message endpoint
        /// </summary>
        [HttpPost("send")]
        public async Task<IActionResult> SendMessage([FromBody] ChatRequestDto input)
        {
            try
            {
                // ✅ Create or load session
                var session = await _historyService.GetOrCreateSessionAsync(input.SessionId, input.Model, input.Messages);

                // ✅ Save user message
                var userMessage = input.Messages.LastOrDefault(m => m.Role == "user")?.Content;
                if (!string.IsNullOrEmpty(userMessage))
                {
                    await _historyService.SaveUserMessageAsync(session, userMessage);
                }

                input.SessionId = session.Id; // Ensure session ID is set

                // Get AI response
                var response = await _openAiService.GetChatResponseAsync(input);

                // Save assistant message
                await _historyService.SaveAssistantMessageAsync(session, response);

                return Ok(new ChatResponseDto
                {
                    Success = true,
                    Data = response,
                    SessionId = session.Id
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ChatResponseDto
                {
                    Success = false,
                    Error = ex.Message
                });
            }
        }

        /// <summary>
        /// Streaming message endpoint
        /// </summary>
        [HttpPost("stream")]
        public async Task StreamMessage([FromBody] ChatRequestDto input)
        {
            Response.Headers.Add("Content-Type", "text/event-stream");

            var session = await _historyService.GetOrCreateSessionAsync(input.SessionId, input.Model, input.Messages);

            // ✅ Save user message before streaming
            var userMessage = input.Messages.LastOrDefault(m => m.Role == "user")?.Content;
            if (!string.IsNullOrEmpty(userMessage))
            {
                await _historyService.SaveUserMessageAsync(session, userMessage);
            }

            // ✅ Send sessionId immediately
            await Response.WriteAsync($"data: {JsonSerializer.Serialize(new { sessionId = session.Id })}\n\n");
            await Response.Body.FlushAsync();

            string finalResponse = "";

            input.SessionId = session.Id; // Ensure session ID is set

            await foreach (var chunk in _openAiService.GetStreamingResponseAsync(input, HttpContext.RequestAborted))
            {
                finalResponse += chunk;
                await Response.WriteAsync($"data: {JsonSerializer.Serialize(new { text = chunk })}\n\n");
                await Response.Body.FlushAsync();
            }

            // Save the full assistant message
            if (!string.IsNullOrWhiteSpace(finalResponse))
            {
                await _historyService.SaveAssistantMessageAsync(session, finalResponse);
            }

            await Response.WriteAsync("data: [DONE]\n\n");
            await Response.Body.FlushAsync();
        }

        /// <summary>
        /// Get chat history for a session
        /// </summary>
        [HttpGet("history/{sessionId}")]
        public async Task<IActionResult> GetHistory(int sessionId)
        {
            var history = await _historyService.GetHistoryAsync(sessionId);
            if (history == null) return NotFound();

            // ✅ Always wrap in { messages: [...] }
            return Ok(new
            {
                success = true,
                data = new { messages = history }
            });
        }

        /// <summary>
        /// Get all sessions
        /// </summary>
        [HttpGet("sessions")]
        public async Task<IActionResult> GetSessions()
        {
            var sessions = await _historyService.GetSessionsAsync();
            return Ok(new { success = true, data = sessions });
        }

        /// <summary>
        /// Create a new empty session
        /// </summary>
        [HttpPost("new")]
        public async Task<IActionResult> NewSession([FromQuery] string model)
        {
            var session = await _historyService.CreateSessionAsync(model);
            return Ok(new { success = true, data = session });
        }

        /// <summary>
        /// Delete all sessions
        /// </summary>
        [HttpDelete("sessions")]
        public async Task<IActionResult> ClearSessions()
        {
            await _historyService.ClearSessionsAsync();
            return Ok(new { success = true });
        }

        /// <summary>
        /// Delete a single session
        /// </summary>
        [HttpDelete("sessions/{sessionId}")]
        public async Task<IActionResult> DeleteSession(int sessionId)
        {
            await _historyService.DeleteSessionAsync(sessionId);
            return Ok(new { success = true });
        }

        /// <summary>
        /// Duplicate a session to a different model
        /// </summary>
        [HttpPost("duplicate-session")]
        public async Task<IActionResult> DuplicateSession([FromQuery] int sessionId, [FromQuery] string newModel)
        {
            var session = await _historyService.DuplicateSessionAsync(sessionId, newModel);
            return Ok(new { success = true, data = session });
        }
    }
}
