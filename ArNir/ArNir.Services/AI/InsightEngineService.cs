using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using ArNir.Core.DTOs.AI;

namespace ArNir.Services.AI
{
    public class InsightEngineService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _config;
        private readonly AnomalyDetectionService _anomalyDetection;
        private readonly string _apiKey;
        private readonly string _model;

        public InsightEngineService(
            HttpClient httpClient,
            IConfiguration config,
            AnomalyDetectionService anomalyDetection)
        {
            _httpClient = httpClient;
            _config = config;
            _anomalyDetection = anomalyDetection;
            _apiKey = _config["OpenAI:ApiKey"]!;
            _model = _config["OpenAI:Model"] ?? "gpt-4o-mini";
        }

        /// <summary>
        /// Generates textual analytics insights using OpenAI GPT
        /// and performs statistical anomaly detection on the dataset.
        /// </summary>
        public async Task<InsightResponseDto> GenerateInsightsAsync(InsightRequestDto request)
        {
            var responseDto = new InsightResponseDto();

            // 1️⃣ Build GPT Prompt
            var prompt = $@"
                You are an AI analyst summarizing system analytics.
                Given the dataset below, explain patterns, anomalies, and recommendations.

                Provider: {request.Provider}
                Model: {request.Model}
                Metric: {request.MetricType}
                Date Range: {request.StartDate:yyyy-MM-dd} → {request.EndDate:yyyy-MM-dd}

                Dataset:
                {request.DataJson}
            ";

            var body = new
            {
                model = _model,
                messages = new[]
                {
                    new { role = "system", content = "You are an expert analytics assistant." },
                    new { role = "user", content = prompt }
                }
            };

            // 2️⃣ Call OpenAI API
            try
            {
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");

                var response = await _httpClient.PostAsJsonAsync("https://api.openai.com/v1/chat/completions", body);
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                dynamic result = JsonConvert.DeserializeObject(json)!;
                string summary = result.choices[0].message.content ?? "No insight generated.";

                responseDto.Summary = summary;
            }
            catch (Exception ex)
            {
                responseDto.Summary = $"❌ Insight generation failed: {ex.Message}";
            }

            // 3️⃣ Local Anomaly Detection (via dedicated service)
            try
            {
                if (!string.IsNullOrEmpty(request.DataJson))
                {
                    var anomalies = _anomalyDetection.AnalyzeJson(request.DataJson);
                    responseDto.Anomalies = anomalies;
                }
                else
                {
                    responseDto.Anomalies = new List<string> { "ℹ️ No data provided for anomaly detection." };
                }
            }
            catch (Exception ex)
            {
                responseDto.Anomalies = new List<string> { $"⚠️ Anomaly detection error: {ex.Message}" };
            }

            responseDto.GeneratedAt = DateTime.UtcNow;
            return responseDto;
        }
    }
}
