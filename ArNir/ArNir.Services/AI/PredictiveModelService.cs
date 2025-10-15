using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using ArNir.Core.DTOs.AI;

namespace ArNir.Services.AI
{
    public class PredictiveModelService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _config;
        private readonly string _apiKey;
        private readonly string _model;

        public PredictiveModelService(HttpClient httpClient, IConfiguration config)
        {
            _httpClient = httpClient;
            _config = config;
            _apiKey = _config["OpenAI:ApiKey"]!;
            _model = _config["OpenAI:Model"] ?? "gpt-4o-mini";
        }

        public async Task<PredictResponseDto> GeneratePredictionAsync(PredictRequestDto request)
        {
            var response = new PredictResponseDto();

            // 🧮 1️⃣ Perform statistical regression
            if (request.Values == null || request.Values.Count < 3)
            {
                response.TrendSummary = "⚠️ Not enough data for regression analysis.";
                return response;
            }

            var predicted = PerformLinearRegression(request.Values, request.ForecastPoints);
            response.PredictedValues = predicted;

            // 🧠 2️⃣ Optionally enhance with GPT summary
            if (request.UseGPT)
            {
                var prompt = $@"
                    You are an AI data analyst.
                    Given the following historical metrics and predicted values, provide a concise trend summary.

                    Historical Values: {string.Join(", ", request.Values)}
                    Predicted Values: {string.Join(", ", predicted)}

                    Explain in one paragraph what this trend means for {request.MetricType} for provider {request.Provider}.
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
                            new { role = "system", content = "You are a data analytics assistant." },
                            new { role = "user", content = prompt }
                        }
                    };

                    var result = await _httpClient.PostAsJsonAsync("https://api.openai.com/v1/chat/completions", body);
                    result.EnsureSuccessStatusCode();

                    var json = await result.Content.ReadAsStringAsync();
                    dynamic parsed = JsonConvert.DeserializeObject(json)!;
                    response.TrendSummary = parsed.choices[0].message.content;
                }
                catch (Exception ex)
                {
                    response.TrendSummary = $"⚠️ GPT forecast error: {ex.Message}";
                }
            }
            else
            {
                response.TrendSummary = "Prediction generated using statistical regression only.";
            }

            response.GeneratedAt = DateTime.UtcNow;
            return response;
        }

        // 📊 Simple linear regression (y = a + b*x)
        private List<double> PerformLinearRegression(List<double> values, int forecastPoints)
        {
            int n = values.Count;
            var x = Enumerable.Range(1, n).Select(i => (double)i).ToList();
            var y = values;

            double meanX = x.Average();
            double meanY = y.Average();

            double numerator = x.Zip(y, (xi, yi) => (xi - meanX) * (yi - meanY)).Sum();
            double denominator = x.Sum(xi => Math.Pow(xi - meanX, 2));

            double b = numerator / denominator;
            double a = meanY - b * meanX;

            var predictions = new List<double>();
            for (int i = n + 1; i <= n + forecastPoints; i++)
                predictions.Add(a + b * i);

            return predictions;
        }
    }
}
