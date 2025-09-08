using _2_OpenAIChatDemo.DTOs;
using _2_OpenAIChatDemo.Settings;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;

namespace _2_OpenAIChatDemo.Services
{
    public class OpenAiService : IOpenAiService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly OpenAISettings _settings;

        public OpenAiService(IHttpClientFactory httpClientFactory, IOptions<OpenAISettings> settings)
        {
            _httpClientFactory = httpClientFactory;
            _settings = settings.Value;
        }

        public async Task<string> GetChatResponseAsync(ChatRequestDto request)
        {
            using var httpClient = _httpClientFactory.CreateClient();
            httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", _settings.ApiKey);

            var body = new
            {
                model = request.Model,
                messages = request.Messages.Select(m => new { role = m.Role, content = m.Content })
            };

            var response = await httpClient.PostAsync(
                "https://api.openai.com/v1/chat/completions",
                new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json")
            );

            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var parsed = JsonSerializer.Deserialize<JsonElement>(json);

            return parsed.GetProperty("choices")[0]
                         .GetProperty("message")
                         .GetProperty("content")
                         .GetString() ?? "";
        }

        public async IAsyncEnumerable<string> GetStreamingResponseAsync(ChatRequestDto request, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            using var httpClient = _httpClientFactory.CreateClient();
            httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", _settings.ApiKey);

            var body = new
            {
                model = request.Model,
                messages = request.Messages.Select(m => new { role = m.Role, content = m.Content }),
                stream = true
            };

            using var response = await httpClient.PostAsync(
                "https://api.openai.com/v1/chat/completions",
                new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json"),
                cancellationToken
            );

            response.EnsureSuccessStatusCode();

            using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var reader = new StreamReader(stream);

            while (!reader.EndOfStream)
            {
                var line = await reader.ReadLineAsync();
                if (string.IsNullOrWhiteSpace(line) || !line.StartsWith("data:")) continue;

                if (line == "data: [DONE]")
                    yield break;

                var json = line.Substring("data:".Length).Trim();
                var parsed = JsonSerializer.Deserialize<JsonElement>(json);

                if (parsed.TryGetProperty("choices", out var choices) &&
                    choices[0].TryGetProperty("delta", out var delta) &&
                    delta.TryGetProperty("content", out var content))
                {
                    yield return content.GetString() ?? "";
                }
            }
        }

    }
}
