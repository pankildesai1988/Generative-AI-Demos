using ArNir.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ArNir.Services
{
    public class ClaudeService : ILlmService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        public ClaudeService(IConfiguration configuration)
        {
            _httpClient = new HttpClient();
            _apiKey = configuration["Claude:ApiKey"];
            _httpClient.DefaultRequestHeaders.Add("x-api-key", _apiKey);
            _httpClient.DefaultRequestHeaders.Add("anthropic-version", "2023-06-01");
        }

        public async Task<string> GetCompletionAsync(string prompt, string model = "claude-3-opus-20240229")
        {
            var requestBody = new
            {
                model = model,
                max_tokens = 512,
                messages = new[]
                {
                    new { role = "user", content = prompt }
                }
            };

            var content = new StringContent(
                JsonSerializer.Serialize(requestBody),
                Encoding.UTF8,
                "application/json"
            );

            var response = await _httpClient.PostAsync("https://api.anthropic.com/v1/messages", content);
            response.EnsureSuccessStatusCode();

            var responseString = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(responseString);

            return doc.RootElement
                .GetProperty("content")[0]
                .GetProperty("text")
                .GetString();
        }
    }
}
