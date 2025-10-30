using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using ArNir.Services.Interfaces;

namespace ArNir.Services.AI
{
    public class ChatInsightService : IChatInsightService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly string _model;

        public ChatInsightService(HttpClient httpClient, IConfiguration config)
        {
            _httpClient = httpClient;
            _apiKey = config["OpenAI:ApiKey"] ?? string.Empty;
            _model = config["OpenAI:Model"] ?? "gpt-4o-mini";
        }

        public async Task<string> GenerateInsightAsync(string userPrompt)
        {
            var body = new
            {
                model = _model,
                messages = new[]
                {
                    new { role = "system", content = "You are an AI analytics assistant summarizing RAG provider performance." },
                    new { role = "user", content = userPrompt }
                }
            };

            using var req = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/chat/completions");
            req.Headers.Add("Authorization", $"Bearer {_apiKey}");
            req.Content = JsonContent.Create(body);

            var res = await _httpClient.SendAsync(req);
            var json = await res.Content.ReadAsStringAsync();

            using var doc = JsonDocument.Parse(json);
            return doc.RootElement
                      .GetProperty("choices")[0]
                      .GetProperty("message")
                      .GetProperty("content")
                      .GetString() ?? "(No response)";
        }
    }
}
