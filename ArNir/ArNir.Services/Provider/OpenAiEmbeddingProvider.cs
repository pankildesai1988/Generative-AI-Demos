using ArNir.Services.Provider;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace ArNir.Services
{
    public class OpenAiEmbeddingProvider : IEmbeddingProvider
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        public OpenAiEmbeddingProvider(HttpClient httpClient, IConfiguration config)
        {
            _httpClient = httpClient;
            _apiKey = config["OpenAI:ApiKey"]!;
        }

        public async Task<float[]> GenerateEmbeddingAsync(string text, string model)
        {
            var request = new
            {
                input = text,
                model = model
            };

            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", _apiKey);

            var response = await _httpClient.PostAsJsonAsync("https://api.openai.com/v1/embeddings", request);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var doc = JsonDocument.Parse(json);
            var arr = doc.RootElement.GetProperty("data")[0].GetProperty("embedding");

            return arr.EnumerateArray().Select(x => (float)x.GetDouble()).ToArray();
        }
    }
}
