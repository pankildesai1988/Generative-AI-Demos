using _2_OpenAIChatDemo.DTOs;
using System.Text;
using System.Text.Json;

namespace _2_OpenAIChatDemo.LLMProviders
{
    public class GeminiProvider : ILlmProvider
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        public string ProviderName => "Gemini";

        // Add available models here
        public List<string> GetAvailableModels() => new List<string>
    {
        "gemini-2.0-flash",
        "gemini-1.5-pro",
        "gemini-1.5-flash"
    };

        public GeminiProvider(IHttpClientFactory factory, IConfiguration config)
        {
            _httpClient = factory.CreateClient();
            _apiKey = config["GoogleAI:ApiKey"];
        }

        public async Task<string> GetResponseAsync(string model, string inputText)
        {
            var body = new
            {
                contents = new[]
                {
                new { role = "user", parts = new[] { new { text = inputText } } }
            }
            };

            int retries = 3;
            for (int attempt = 1; attempt <= retries; attempt++)
            {
                var response = await _httpClient.PostAsync(
                    $"https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent?key={_apiKey}",
                    new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json")
                );

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var parsed = JsonSerializer.Deserialize<JsonElement>(json);

                    return parsed.GetProperty("candidates")[0]
                                 .GetProperty("content")
                                 .GetProperty("parts")[0]
                                 .GetProperty("text")
                                 .GetString() ?? "";
                }

                if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests && attempt < retries)
                {
                    await Task.Delay(1000 * attempt);
                    continue;
                }

                if (!response.IsSuccessStatusCode && attempt == retries)
                    return $"[Error: Gemini API failed → {response.StatusCode}]";
            }

            return "[Error: Gemini provider failed after retries]";
        }
    }
}
