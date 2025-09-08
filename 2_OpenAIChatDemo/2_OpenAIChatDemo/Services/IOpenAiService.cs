using _2_OpenAIChatDemo.DTOs;

namespace _2_OpenAIChatDemo.Services
{
    public interface IOpenAiService
    {
        Task<string> GetChatResponseAsync(ChatRequestDto request);
        IAsyncEnumerable<string> GetStreamingResponseAsync(ChatRequestDto request, CancellationToken cancellationToken = default);
    }
}
