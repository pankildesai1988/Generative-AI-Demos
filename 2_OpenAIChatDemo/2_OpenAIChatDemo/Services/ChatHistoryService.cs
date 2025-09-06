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

        public async Task SaveUserMessageAsync(ChatSession session, string content)
        {
            var msg = new ChatMessage
            {
                Role = "user",
                Content = content,
                CreatedAt = DateTime.UtcNow,
                ChatSessionId = session.Id
            };
            _db.ChatMessages.Add(msg);
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

        public async Task<List<ChatSession>> GetSessionsAsync()
        {
            return await _db.ChatSessions.OrderByDescending(s => s.CreatedAt).ToListAsync();
        }

        public async Task<ChatSession?> GetSessionWithHistoryAsync(int sessionId)
        {
            return await _db.ChatSessions
                .Include(s => s.Messages.OrderBy(m => m.CreatedAt))
                .FirstOrDefaultAsync(s => s.Id == sessionId);
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

        public async Task<ChatSession?> DuplicateSessionAsync(int sessionId, string newModel)
        {
            var session = await _db.ChatSessions
                .Include(s => s.Messages)
                .FirstOrDefaultAsync(s => s.Id == sessionId);

            if (session == null) return null;

            var newSession = new ChatSession
            {
                Title = session.Title + " (Copy)",
                Model = newModel,
                CreatedAt = DateTime.UtcNow,
                Messages = new List<ChatMessage>()
            };

            foreach (var msg in session.Messages)
            {
                newSession.Messages.Add(new ChatMessage
                {
                    Role = msg.Role,
                    Content = msg.Content,
                    CreatedAt = msg.CreatedAt
                });
            }

            _db.ChatSessions.Add(newSession);
            await _db.SaveChangesAsync();

            return newSession;
        }

    }
}
