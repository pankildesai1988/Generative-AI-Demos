using _2_OpenAIChatDemo.DTOs;

namespace _2_OpenAIChatDemo.LLMProviders
{
    public interface ILlmProvider
    {
        string ProviderName { get; }  // e.g. "OpenAI", "Claude-3", "Gemini-1.5"
        Task<string> GetResponseAsync(string model, string inputText);
        List<string> GetAvailableModels();
    }
}
