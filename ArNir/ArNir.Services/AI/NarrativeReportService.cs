using ArNir.Core.DTOs.AI;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ArNir.Services.AI
{
    public class NarrativeReportService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _config;
        private readonly string _apiKey;
        private readonly string _model;

        public NarrativeReportService(HttpClient httpClient, IConfiguration config)
        {
            _httpClient = httpClient;
            _config = config;
            _apiKey = _config["OpenAI:ApiKey"]!;
            _model = _config["OpenAI:Model"] ?? "gpt-4o-mini";
        }

        public async Task<NarrativeReportResponseDto> GenerateAsync(NarrativeReportRequestDto dto)
        {
            var response = new NarrativeReportResponseDto();

            // Build structured prompt
            var prompt = $@"
You are an AI analytics assistant. Combine the following sections into a clear, professional report.

### Context
Provider: {dto.Provider}
Metric Type: {dto.MetricType}

### Insights
{dto.Insights}

### Detected Anomalies
{string.Join("\n", dto.Anomalies ?? new List<string>())}

### Forecast
Predicted Values: {string.Join(", ", dto.Predictions ?? new List<double>())}
Summary: {dto.TrendSummary}

Generate a narrative Markdown report with:
- Section headers (##)
- Bullet points for key findings
- Short executive summary at the top
";

            try
            {
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");

                var body = new
                {
                    model = _model,
                    messages = new[]
                    {
                        new { role = "system", content = "You are a professional AI report generator." },
                        new { role = "user", content = prompt }
                    }
                };

                var result = await _httpClient.PostAsJsonAsync("https://api.openai.com/v1/chat/completions", body);
                result.EnsureSuccessStatusCode();

                var json = await result.Content.ReadAsStringAsync();
                dynamic parsed = JsonConvert.DeserializeObject(json)!;
                response.ReportMarkdown = parsed.choices[0].message.content;
            }
            catch (Exception ex)
            {
                response.ReportMarkdown = $"⚠️ Error generating report: {ex.Message}";
            }

            response.GeneratedAt = DateTime.UtcNow;
            return response;
        }
    }
}
