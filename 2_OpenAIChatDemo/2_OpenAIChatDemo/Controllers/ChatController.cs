using _2_OpenAIChatDemo.Data;
using _2_OpenAIChatDemo.DTOs;
using _2_OpenAIChatDemo.Models;
using _2_OpenAIChatDemo.Services;
using _2_OpenAIChatDemo.Settings;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace _2_OpenAIChatDemo.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChatController : ControllerBase
    {
        private readonly IOpenAiService _openAiService;
        private readonly IChatHistoryService _historyService;

        public ChatController(IOpenAiService openAiService, IChatHistoryService historyService)
        {
            _openAiService = openAiService;
            _historyService = historyService;
        }

        [HttpPost("send")]
        public async Task<IActionResult> SendMessage([FromBody] ChatRequestDto input)
        {
            try
            {
                var session = await _historyService.GetOrCreateSessionAsync(input);

                await _historyService.SaveUserMessageAsync(session, input.Messages.First().Content);

                var response = await _openAiService.GetChatResponseAsync(input);

                await _historyService.SaveAssistantMessageAsync(session, response);

                return Ok(new { success = true, data = response, sessionId = session.Id });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, error = ex.Message });
            }
        }

        [HttpPost("stream")]
        public async Task StreamMessage([FromBody] ChatRequestDto request, CancellationToken cancellationToken)
        {
            Response.Headers.Add("Content-Type", "text/event-stream");
            Response.Headers.Add("Cache-Control", "no-cache");
            Response.Headers.Add("Connection", "keep-alive");

            try
            {
                var session = await _historyService.GetOrCreateSessionAsync(request);

                await _historyService.SaveUserMessageAsync(session, request.Messages.First().Content);

                string fullAssistantResponse = "";

                await foreach (var chunk in _openAiService.GetStreamingResponseAsync(request, cancellationToken))
                {
                    fullAssistantResponse += chunk;

                    var json = JsonSerializer.Serialize(new { text = chunk });
                    await Response.WriteAsync($"data: {json}\n\n", cancellationToken);
                    await Response.Body.FlushAsync(cancellationToken);
                }

                // ✅ Save assistant's full response after streaming is done
                await _historyService.SaveAssistantMessageAsync(session, fullAssistantResponse);

                await Response.WriteAsync("data: [DONE]\n\n", cancellationToken);
                await Response.Body.FlushAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                var errorJson = JsonSerializer.Serialize(new { error = ex.Message });
                await Response.WriteAsync($"data: {errorJson}\n\n", cancellationToken);
                await Response.Body.FlushAsync(cancellationToken);
            }
        }

        [HttpGet("sessions")]
        public async Task<IActionResult> GetSessions()
        {
            var sessions = await _historyService.GetSessionsAsync();
            return Ok(new { success = true, data = sessions.Select(s => new { s.Id, s.Title, s.Model }) });
        }

        [HttpPost("new")]
        public async Task<IActionResult> CreateNewSession([FromQuery] string model)
        {
            var session = await _historyService.CreateNewSessionAsync(model);
            return Ok(new { success = true, sessionId = session.Id, model = session.Model });
        }

        [HttpGet("history/{sessionId}")]
        public async Task<IActionResult> GetSessionHistory(int sessionId)
        {
            var session = await _historyService.GetSessionWithHistoryAsync(sessionId);
            if (session == null)
                return NotFound(new { success = false, error = "Session not found" });

            return Ok(new
            {
                success = true,
                data = new
                {
                    session.Id,
                    session.Title,
                    session.Model,
                    messages = session.Messages.Select(m => new { m.Role, m.Content, m.CreatedAt })
                }
            });
        }

        [HttpDelete("sessions/{sessionId}")]
        public async Task<IActionResult> DeleteSession(int sessionId)
        {
            await _historyService.DeleteSessionAsync(sessionId);
            return Ok(new { success = true });
        }

        [HttpDelete("sessions")]
        public async Task<IActionResult> DeleteAllSessions()
        {
            await _historyService.DeleteAllSessionsAsync();
            return Ok(new { success = true });
        }

        [HttpPost("duplicate-session")]
        public async Task<IActionResult> DuplicateSession([FromQuery] int sessionId, [FromQuery] string newModel)
        {
            var newSession = await _historyService.DuplicateSessionAsync(sessionId, newModel);
            if (newSession == null)
                return NotFound(new { success = false, error = "Session not found" });

            return Ok(new { success = true, newSessionId = newSession.Id, model = newModel });
        }

    }
}
