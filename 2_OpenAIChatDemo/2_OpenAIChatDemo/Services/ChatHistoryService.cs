using _2_OpenAIChatDemo.Data;
using _2_OpenAIChatDemo.DTOs;
using _2_OpenAIChatDemo.Models;
using Microsoft.EntityFrameworkCore;

namespace _2_OpenAIChatDemo.Services
{
    public class ChatHistoryService : IChatHistoryService
    {
        private readonly ChatDbContext _db;

        public ChatHistoryService(ChatDbContext db)
        {
            _db = db;
        }

        public async Task<ChatSession> GetOrCreateSessionAsync(ChatRequestDto input)
        {
            var session = await _db.ChatSessions
                .Include(s => s.Messages)
                .FirstOrDefaultAsync(s => s.Id == input.SessionId);

            if (session == null)
            {
                session = new ChatSession
                {
                    Title = input.Messages.First().Content.Length > 30
                        ? input.Messages.First().Content.Substring(0, 30) + "..."
                        : input.Messages.First().Content,
                    Model = input.Model,
                    CreatedAt = DateTime.UtcNow,
                    Messages = new List<ChatMessage>()
                };
                _db.ChatSessions.Add(session);
                await _db.SaveChangesAsync();
            }

            return session;
        }

        public async Task<ChatSession> GetOrCreateSessionAsync(int sessionId, string model, List<ChatMessageDto> messages)
        {
            ChatSession session;

            if (sessionId > 0)
            {
                session = await _db.ChatSessions
                    .Include(s => s.Messages)
                    .FirstOrDefaultAsync(s => s.Id == sessionId);

                if (session == null)
                {
                    // Session not found → create new
                    session = new ChatSession
                    {
                        Model = model,
                        Title = messages.FirstOrDefault()?.Content ?? "New Session",
                        CreatedAt = DateTime.UtcNow,
                        LastMessageAt = DateTime.UtcNow
                    };
                    _db.ChatSessions.Add(session);
                    await _db.SaveChangesAsync();
                }
            }
            else
            {
                // ✅ Create new session if sessionId = 0
                session = new ChatSession
                {
                    Model = model,
                    Title = messages.FirstOrDefault()?.Content ?? "New Session",
                    CreatedAt = DateTime.UtcNow,
                    LastMessageAt = DateTime.UtcNow
                };
                _db.ChatSessions.Add(session);
                await _db.SaveChangesAsync();
            }

            return session;
        }

        public async Task<ChatSession> CreateNewSessionAsync(string model, string? title = null)
        {
            var session = new ChatSession
            {
                Title = title ?? "New Chat",
                Model = model,
                CreatedAt = DateTime.UtcNow,
                Messages = new List<ChatMessage>()
            };

            _db.ChatSessions.Add(session);
            await _db.SaveChangesAsync();
            return session;
        }

        public async Task SaveUserMessageAsync(ChatSession session, string message)
        {
            var chatMessage = new ChatMessage
            {
                Role = "user",
                Content = message,
                ChatSessionId = session.Id,
                CreatedAt = DateTime.UtcNow
            };

            _db.ChatMessages.Add(chatMessage);
            session.LastMessageAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
        }

        public async Task SaveAssistantMessageAsync(ChatSession session, string content)
        {
            var msg = new ChatMessage
            {
                Role = "assistant",
                Content = content,
                CreatedAt = DateTime.UtcNow,
                ChatSessionId = session.Id
            };
            _db.ChatMessages.Add(msg);
            await _db.SaveChangesAsync();
        }

        // ✅ Implement GetSessionsAsync
        public async Task<IEnumerable<ChatSessionDto>> GetSessionsAsync()
        {
            return await _db.ChatSessions
                .OrderByDescending(s => s.LastMessageAt)
                .Select(s => new ChatSessionDto
                {
                    SessionId = s.Id,
                    Title = s.Title,
                    Model = s.Model,
                    CreatedAt = s.CreatedAt,
                    LastMessageAt = s.LastMessageAt
                })
                .ToListAsync();
        }

        public async Task<ChatSession?> GetSessionWithHistoryAsync(int sessionId)
        {
            return await _db.ChatSessions
                .Include(s => s.Messages.OrderBy(m => m.CreatedAt))
                .FirstOrDefaultAsync(s => s.Id == sessionId);
        }        

        public async Task DeleteSessionAsync(int sessionId)
        {
            var session = await _db.ChatSessions.FindAsync(sessionId);
            if (session != null)
            {
                _db.ChatSessions.Remove(session);
                await _db.SaveChangesAsync();
            }
        }

        public async Task DeleteAllSessionsAsync()
        {
            _db.ChatSessions.RemoveRange(_db.ChatSessions);
            await _db.SaveChangesAsync();
        }

        // ✅ Implement DuplicateSessionAsync
        public async Task<ChatSessionDto> DuplicateSessionAsync(int sessionId, string newModel)
        {
            var original = await _db.ChatSessions
                .Include(s => s.Messages)
                .FirstOrDefaultAsync(s => s.Id == sessionId);

            if (original == null) return null;

            var newSession = new ChatSession
            {
                Model = newModel,
                Title = original.Title + " (Clone)",
                CreatedAt = DateTime.UtcNow,
                LastMessageAt = DateTime.UtcNow
            };

            _db.ChatSessions.Add(newSession);
            await _db.SaveChangesAsync();

            // ✅ Copy all messages (user + assistant) into new session
            foreach (var msg in original.Messages.OrderBy(m => m.CreatedAt))
            {
                _db.ChatMessages.Add(new ChatMessage
                {
                    Role = msg.Role,            // "user" or "assistant"
                    Content = msg.Content,
                    ChatSessionId = newSession.Id,
                    CreatedAt = msg.CreatedAt   // keep original timestamp
                });
            }

            await _db.SaveChangesAsync();

            return new ChatSessionDto
            {
                SessionId = newSession.Id,
                Title = newSession.Title,
                Model = newSession.Model,
                CreatedAt = newSession.CreatedAt,
                LastMessageAt = newSession.LastMessageAt
            };
        }


        // ✅ Implement GetHistoryAsync
        public async Task<IEnumerable<ChatMessageDto>> GetHistoryAsync(int sessionId)
        {
            return await _db.ChatMessages
                .Where(m => m.ChatSessionId == sessionId)
                .OrderBy(m => m.CreatedAt)
                .Select(m => new ChatMessageDto { Role = m.Role, Content = m.Content })
                .ToListAsync();
        }

        // ✅ Implement CreateSessionAsync
        public async Task<ChatSessionDto> CreateSessionAsync(string model)
        {
            var session = new ChatSession
            {
                Model = model,
                Title = "New Session",
                CreatedAt = DateTime.UtcNow,
                LastMessageAt = DateTime.UtcNow
            };

            _db.ChatSessions.Add(session);
            await _db.SaveChangesAsync();

            return new ChatSessionDto
            {
                SessionId = session.Id,
                Title = session.Title,
                Model = session.Model,
                CreatedAt = session.CreatedAt,
                LastMessageAt = session.LastMessageAt
            };
        }

        // ✅ Implement ClearSessionsAsync
        public async Task ClearSessionsAsync()
        {
            _db.ChatSessions.RemoveRange(_db.ChatSessions);
            await _db.SaveChangesAsync();
        }

    }
}
