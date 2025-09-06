using _2_OpenAIChatDemo.DTOs;
using _2_OpenAIChatDemo.Models;

namespace _2_OpenAIChatDemo.Services
{
    public interface IChatHistoryService
    {
        Task<ChatSession> GetOrCreateSessionAsync(ChatRequestDto input);
        Task<ChatSession> CreateNewSessionAsync(string model, string? title = null);
        Task SaveUserMessageAsync(ChatSession session, string content);
        Task SaveAssistantMessageAsync(ChatSession session, string content);
        Task<List<ChatSession>> GetSessionsAsync();
        Task<ChatSession?> GetSessionWithHistoryAsync(int sessionId);
        Task DeleteSessionAsync(int sessionId);
        Task DeleteAllSessionsAsync();
        Task<ChatSession?> DuplicateSessionAsync(int sessionId, string newModel);
    }

}
