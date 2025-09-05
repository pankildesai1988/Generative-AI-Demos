using _2_OpenAIChatDemo.Data;
using _2_OpenAIChatDemo.DTOs;
using _2_OpenAIChatDemo.Models;
using _2_OpenAIChatDemo.Settings;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace _2_OpenAIChatDemo.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChatController : ControllerBase
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly ChatDbContext _db;
        private readonly ILogger<ChatController> _logger;
        private readonly OpenAISettings _settings;
        public ChatController(ILogger<ChatController> logger, IHttpClientFactory httpClientFactory, IOptions<OpenAISettings> settings, ChatDbContext db)
        {
            _logger = logger;
            _httpClient = httpClientFactory.CreateClient();
            _apiKey = settings.Value.ApiKey;
            _settings = settings.Value;
            _db = db;
        }

        [HttpPost("send")]
        public async Task<IActionResult> SendMessage([FromBody] ChatRequestDto input)
        {
            if (input.Messages == null || input.Messages.Count == 0)
                return BadRequest(new { success = false, error = "Messages cannot be empty" });

            ChatSession? session = _db.ChatSessions.FirstOrDefault(s => s.Id == input.SessionId && s.UserId == "default");


            if (session == null)
            {
                session = new ChatSession
                {
                    UserId = "default",
                    Model = string.IsNullOrEmpty(input.Model) ? _settings.DefaultModel : input.Model
                };
                _db.ChatSessions.Add(session);
            }
            else
            {
                input.Model = session.Model; // enforce session's model
            }

            // Build OpenAI payload
            var payload = new
            {
                model = string.IsNullOrEmpty(input.Model) ? _settings.DefaultModel : input.Model,
                stream = false,
                messages = input.Messages
            };

            // Call OpenAI API
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
            var response = await _httpClient.PostAsync("https://api.openai.com/v1/chat/completions",
                new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json"));

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);

            string? message = null;
            if (doc.RootElement.TryGetProperty("choices", out var choices) &&
                choices.GetArrayLength() > 0 &&
                choices[0].TryGetProperty("message", out var msg) &&
                msg.TryGetProperty("content", out var content))
            {
                message = content.GetString();
            }

            if (message == null)
                return BadRequest(new { success = false, error = "No AI response", raw = json });

            // ✅ Save to DB (map DTO → Entity)
            if (string.IsNullOrEmpty(session.Title))
            {
                var firstUserMessage = input.Messages.FirstOrDefault(m => m.Role == "user")?.Content;
                if (!string.IsNullOrEmpty(firstUserMessage))
                {
                    session.Title = firstUserMessage.Length > 50
                    ? firstUserMessage.Substring(0, 50) + "..."
                    : firstUserMessage;
                }
            }


            foreach (var m in input.Messages)
                _db.ChatMessages.Add(new ChatMessage { ChatSession = session, Role = m.Role, Content = m.Content });


            _db.ChatMessages.Add(new ChatMessage { ChatSession = session, Role = "assistant", Content = message });
            _db.SaveChanges();


            return Ok(new { success = true, data = message, sessionId = session.Id, model = session.Model });
        }


        //streaming endpoint (POST)

        // ✅ Streaming endpoint (GET, EventSource-compatible)
        //[HttpGet("stream")]
        //public async Task StreamMessage([FromBody] ChatRequest input)
        //{
        //    Response.ContentType = "text/event-stream";

        //    _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);

        //    var payload = new
        //    {
        //        model = "gpt-4o-mini",
        //        stream = true,
        //        messages = input.Messages
        //    };

        //    using var request = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/chat/completions")
        //    {
        //        Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json")
        //    };

        //    using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
        //    using var streamResponse = await response.Content.ReadAsStreamAsync();
        //    using var reader = new StreamReader(streamResponse);

        //    while (!reader.EndOfStream)
        //    {
        //        var line = await reader.ReadLineAsync();
        //        if (string.IsNullOrWhiteSpace(line)) continue;

        //        if (line.StartsWith("data: "))
        //        {
        //            var jsonLine = line.Substring(6);
        //            if (jsonLine == "[DONE]") break;

        //            try
        //            {
        //                using var docLine = JsonDocument.Parse(jsonLine);
        //                var content = docLine.RootElement
        //                                     .GetProperty("choices")[0]
        //                                     .GetProperty("delta")
        //                                     .GetProperty("content")
        //                                     .GetString();

        //                if (!string.IsNullOrEmpty(content))
        //                {
        //                    await Response.WriteAsync($"data: {content}\n\n");
        //                    await Response.Body.FlushAsync();
        //                }
        //            }
        //            catch { }
        //        }
        //    }
        //}

        [HttpPost("stream")]
        public async Task StreamMessage([FromBody] ChatRequestDto input)
        {
            _logger.LogInformation("Received request with {MessageCount} messages", input.Messages.Count);

            Response.ContentType = "text/event-stream";

            ChatSession? session = _db.ChatSessions.FirstOrDefault(s => s.Id == input.SessionId && s.UserId == "default");


            if (session == null)
            {
                session = new ChatSession
                {
                    UserId = "default",
                    Model = string.IsNullOrEmpty(input.Model) ? _settings.DefaultModel : input.Model
                };
                _db.ChatSessions.Add(session);
                _db.SaveChanges();
                input.SessionId = session.Id;
            }
            else
            {
                input.Model = session.Model; // enforce session's model
            }


            var payload = new
            {
                model = string.IsNullOrEmpty(input.Model) ? _settings.DefaultModel : input.Model,
                stream = true,
                messages = input.Messages
            };

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);

            using var request = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/chat/completions")
            {
                Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json")
            };

            using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
            using var streamResponse = await response.Content.ReadAsStreamAsync();
            using var reader = new StreamReader(streamResponse);

            string fullResponse = "";

            while (!reader.EndOfStream)
            {
                var line = await reader.ReadLineAsync();
                if (string.IsNullOrWhiteSpace(line)) continue;

                if (line.StartsWith("data: "))
                {
                    var jsonLine = line.Substring(6);
                    if (jsonLine == "[DONE]") break;

                    try
                    {
                        using var docLine = JsonDocument.Parse(jsonLine);
                        var content = docLine.RootElement
                                             .GetProperty("choices")[0]
                                             .GetProperty("delta")
                                             .GetProperty("content")
                                             .GetString();

                        if (!string.IsNullOrEmpty(content))
                        {
                            fullResponse += content;
                            await Response.WriteAsync(content);
                            await Response.Body.FlushAsync();
                        }
                    }
                    catch { }
                }
            }

            // ✅ Save to DB after full response
            if (string.IsNullOrEmpty(session.Title))
            {
                var firstUserMessage = input.Messages.FirstOrDefault(m => m.Role == "user")?.Content;
                if (!string.IsNullOrEmpty(firstUserMessage))
                {
                    session.Title = firstUserMessage.Length > 50
                    ? firstUserMessage.Substring(0, 50) + "..."
                    : firstUserMessage;
                }
            }


            foreach (var m in input.Messages)
            {
                _db.ChatMessages.Add(new ChatMessage
                {
                    ChatSession = session,
                    Role = m.Role,
                    Content = m.Content
                });
            }


            _db.ChatMessages.Add(new ChatMessage
            {
                ChatSession = session,
                Role = "assistant",
                Content = fullResponse
            });


            _db.SaveChanges();


            await Response.WriteAsync("[DONE]");
            await Response.Body.FlushAsync();
        }

        [HttpGet("history")]
        public IActionResult GetHistory()
        {
            var session = _db.ChatSessions
                .Where(s => s.UserId == "default")
                .OrderByDescending(s => s.CreatedAt)
                .Select(s => new ChatHistoryDto
                {
                    SessionId = s.Id,
                    Messages = s.Messages
                        .OrderBy(m => m.CreatedAt)
                        .Select(m => new ChatMessageDto
                        {
                            Role = m.Role,
                            Content = m.Content
                        }).ToList()
                })
                .FirstOrDefault();

            if (session == null)
                return Ok(new { success = true, data = new ChatHistoryDto { SessionId = 0, Messages = new List<ChatMessageDto>() } });

            return Ok(new { success = true, data = session });
        }

        [HttpPost("new")]
        public IActionResult StartNewChat([FromQuery] string? model = null)
        {
            var session = new ChatSession { UserId = "default", Model = string.IsNullOrEmpty(model) ? _settings.DefaultModel : model };
            _db.ChatSessions.Add(session);
            _db.SaveChanges();


            return Ok(new { success = true, sessionId = session.Id, model = session.Model });
        }

        [HttpGet("sessions")]
        public IActionResult GetSessions()
        {
            var sessions = _db.ChatSessions
                .Where(s => s.UserId == "default")
                .OrderByDescending(s => s.CreatedAt)
                .Select(s => new
                {
                    sessionId = s.Id,
                    createdAt = s.CreatedAt,
                    title = s.Title ?? $"Session {s.Id}",
                    model = s.Model
                })
                .ToList();

            return Ok(new { success = true, data = sessions });
        }


        [HttpGet("history/{sessionId}")]
        public IActionResult GetHistoryBySession(int sessionId)
        {
            var session = _db.ChatSessions
                .Where(s => s.UserId == "default" && s.Id == sessionId)
                .Select(s => new ChatHistoryDto
                {
                    SessionId = s.Id,
                    Messages = s.Messages
                        .OrderBy(m => m.CreatedAt)
                        .Select(m => new ChatMessageDto
                        {
                            Role = m.Role,
                            Content = m.Content
                        }).ToList()
                })
                .FirstOrDefault();

            if (session == null)
                return NotFound(new { success = false, error = "Session not found" });

            return Ok(new { success = true, data = session });
        }

        [HttpDelete("sessions/{sessionId}")]
        public IActionResult DeleteSession(int sessionId)
        {
            var session = _db.ChatSessions.FirstOrDefault(s => s.Id == sessionId && s.UserId == "default");

            if (session == null)
                return NotFound(new { success = false, error = "Session not found" });

            _db.ChatSessions.Remove(session); // ✅ cascade deletes messages
            _db.SaveChanges();

            return Ok(new { success = true, message = "Session deleted" });
        }

        [HttpDelete("sessions")]
        public IActionResult DeleteAllSessions()
        {
            var sessions = _db.ChatSessions.Where(s => s.UserId == "default").ToList();

            if (!sessions.Any())
                return Ok(new { success = true, message = "No sessions to delete" });

            _db.ChatSessions.RemoveRange(sessions); // ✅ cascade deletes messages
            _db.SaveChanges();

            return Ok(new { success = true, message = "All sessions deleted" });
        }

        [HttpPost("duplicate-session")]
        public IActionResult DuplicateSession([FromQuery] int sessionId, [FromQuery] string newModel)
        {
            var existing = _db.ChatSessions.FirstOrDefault(s => s.Id == sessionId && s.UserId == "default");
            if (existing == null) return NotFound(new { success = false, error = "Session not found" });

            var clone = new ChatSession
            {
                UserId = existing.UserId,
                Title = (existing.Title ?? $"Session {existing.Id}") + $" ({newModel})", // ✅ add model name in title
                Model = string.IsNullOrEmpty(newModel) ? _settings.DefaultModel : newModel,
                CreatedAt = DateTime.UtcNow
            };

            foreach (var msg in _db.ChatMessages.Where(m => m.ChatSessionId == existing.Id))
            {
                clone.Messages.Add(new ChatMessage
                {
                    Role = msg.Role,
                    Content = msg.Content,
                    CreatedAt = msg.CreatedAt
                });
            }

            _db.ChatSessions.Add(clone);
            _db.SaveChanges();

            return Ok(new { success = true, newSessionId = clone.Id, model = clone.Model, title = clone.Title });
        }

    }
}
