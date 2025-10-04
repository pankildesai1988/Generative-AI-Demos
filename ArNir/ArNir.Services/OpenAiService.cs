using ArNir.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace ArNir.Services
{
    public class OpenAiService : ILlmService
    {
        private readonly HttpClient _httpClient;

        public OpenAiService(IConfiguration configuration)
        {
            _httpClient = new HttpClient();
            var apiKey = configuration["OpenAI:ApiKey"];
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", apiKey);
        }

        public async Task<string> GetCompletionAsync(string prompt, string model = "gpt-4o")
        {
            var requestBody = new
            {
                model = model,
                messages = new[] { new { role = "user", content = prompt } },
                temperature = 0.2
            };

            var content = new StringContent(
                JsonSerializer.Serialize(requestBody),
                Encoding.UTF8,
                "application/json"
            );

            var response = await _httpClient.PostAsync("https://api.openai.com/v1/chat/completions", content);
            response.EnsureSuccessStatusCode();

            var responseString = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(responseString);

            return doc.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString();
        }
    }
}
