using _2_OpenAIChatDemo.DTOs;
using _2_OpenAIChatDemo.Models;

namespace _2_OpenAIChatDemo.Services
{
    public interface IChatHistoryService
    {
        Task<ChatSession> GetOrCreateSessionAsync(ChatRequestDto input);
        Task<ChatSession> GetOrCreateSessionAsync(int sessionId, string model, List<ChatMessageDto> messages);
        Task<ChatSession> CreateNewSessionAsync(string model, string? title = null);
        Task SaveUserMessageAsync(ChatSession session, string message);
        Task SaveAssistantMessageAsync(ChatSession session, string content);
        Task<IEnumerable<ChatSessionDto>> GetSessionsAsync();
        Task<ChatSession?> GetSessionWithHistoryAsync(int sessionId);
        Task DeleteSessionAsync(int sessionId);
        Task DeleteAllSessionsAsync();
        Task<ChatSessionDto> DuplicateSessionAsync(int sessionId, string newModel);
        Task<IEnumerable<ChatMessageDto>> GetHistoryAsync(int sessionId);
        Task<ChatSessionDto> CreateSessionAsync(string model);
        Task ClearSessionsAsync();
    }

}
