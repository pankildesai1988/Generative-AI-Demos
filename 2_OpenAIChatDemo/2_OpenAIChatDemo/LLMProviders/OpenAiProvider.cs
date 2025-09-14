using _2_OpenAIChatDemo.DTOs;
using _2_OpenAIChatDemo.Services;

namespace _2_OpenAIChatDemo.LLMProviders
{
    public class OpenAiProvider : ILlmProvider
    {
        private readonly IOpenAiService _openAiService;
        public string ProviderName => "OpenAI";

        // Add available models here
        public List<string> GetAvailableModels() => new List<string>
    {
        "gpt-4o",
        "gpt-4o-mini",
        "gpt-3.5-turbo"
    };

        public OpenAiProvider(IOpenAiService openAiService)
        {
            _openAiService = openAiService;
        }

        public async Task<string> GetResponseAsync(string model, string inputText)
        {
            var chatRequest = new ChatRequestDto
            {
                SessionId = 0,
                Model = model,
                Messages = new List<ChatMessageDto>
            {
                new ChatMessageDto { Role = "user", Content = inputText }
            }
            };

            int retries = 3;
            for (int attempt = 1; attempt <= retries; attempt++)
            {
                try
                {
                    return await _openAiService.GetChatResponseAsync(chatRequest);
                }
                catch (Exception ex) when (attempt < retries)
                {
                    await Task.Delay(1000 * attempt);
                }
            }

            return "[Error: OpenAI provider failed after retries]";
        }
    }

}
