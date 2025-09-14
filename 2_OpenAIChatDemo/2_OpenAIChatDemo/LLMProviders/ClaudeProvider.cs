using _2_OpenAIChatDemo.DTOs;
using System.Text;
using System.Text.Json;

namespace _2_OpenAIChatDemo.LLMProviders
{
    public class ClaudeProvider : ILlmProvider
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        public string ProviderName => "Claude";

        // Add available models here
        public List<string> GetAvailableModels() => new List<string>
    {
        "claude-3-opus-20240229",
        "claude-3-sonnet-20240229",
        "claude-3-haiku-20240307"
    };

        public ClaudeProvider(IHttpClientFactory factory, IConfiguration config)
        {
            _httpClient = factory.CreateClient();
            _apiKey = config["Anthropic:ApiKey"];
        }

        public async Task<string> GetResponseAsync(string model, string inputText)
        {
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("x-api-key", _apiKey);
            _httpClient.DefaultRequestHeaders.Add("anthropic-version", "2023-06-01");

            var body = new
            {
                model = model,
                messages = new[] { new { role = "user", content = inputText } },
                max_tokens = 1024
            };

            int retries = 3;
            for (int attempt = 1; attempt <= retries; attempt++)
            {
                var response = await _httpClient.PostAsync(
                    "https://api.anthropic.com/v1/messages",
                    new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json")
                );

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var parsed = JsonSerializer.Deserialize<JsonElement>(json);

                    if (parsed.TryGetProperty("content", out var contentArray) &&
                        contentArray.ValueKind == JsonValueKind.Array &&
                        contentArray[0].TryGetProperty("text", out var text))
                    {
                        return text.GetString() ?? "";
                    }

                    return "[Error: Claude response format unexpected]";
                }

                if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests && attempt < retries)
                {
                    await Task.Delay(1000 * attempt);
                    continue;
                }

                if (!response.IsSuccessStatusCode && attempt == retries)
                    return $"[Error: Claude API failed → {response.StatusCode}]";
            }

            return "[Error: Claude provider failed after retries]";
        }
    }
}
