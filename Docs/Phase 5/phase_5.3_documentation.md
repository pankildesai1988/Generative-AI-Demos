# Phase 5.3 – Predictive Model Service

## ✅ Overview
Phase 5.3 introduces the **Predictive Model Service** to the AirNir platform, enabling forward-looking insights. This phase extends the Insight Engine (Phase 5.1) and Anomaly Detection Layer (Phase 5.2) by adding the ability to **forecast trends** and **predict future values** for key metrics like latency, SLA compliance, and feedback scores.

The service combines **statistical regression** and **GPT-powered reasoning** to generate both numerical forecasts and natural-language trend explanations.

---

## 🎯 Objectives
- Implement **PredictiveModelService** for statistical + GPT-assisted predictions.
- Create the `/api/insights/predict` API endpoint.
- Define DTOs for standardized forecasting input/output.
- Extend the **React Insights Dashboard** with a “Predict Trend” feature.
- Add visual trend analysis using **Recharts line charts**.

---

## 🧱 Key Deliverables
| Component | Description | Status |
|------------|--------------|--------|
| `PredictiveModelService.cs` | Core regression + GPT prediction logic | ✅ |
| `PredictRequestDto.cs` / `PredictResponseDto.cs` | Input/output data models | ✅ |
| `InsightsController` | `/predict` endpoint for forecasts | ✅ |
| `InsightsPage.jsx` | Added Predict button + chart visualization | ✅ |
| `Phase5.3_PredictiveModel_Architecture.png` | Architecture diagram for docs | ✅ |

---

## ⚙️ Technical Implementation

### 1️⃣ DTOs
**PredictRequestDto.cs**
```csharp
public class PredictRequestDto
{
    public string? Provider { get; set; }
    public string? MetricType { get; set; }
    public List<double>? Values { get; set; }
    public int ForecastPoints { get; set; } = 3;
    public bool UseGPT { get; set; } = false;
}
```

**PredictResponseDto.cs**
```csharp
public class PredictResponseDto
{
    public List<double>? PredictedValues { get; set; }
    public string? TrendSummary { get; set; }
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
}
```

---

### 2️⃣ Predictive Model Service
**PredictiveModelService.cs** combines:
- **Linear Regression** for statistical predictions.
- **GPT-based summaries** for contextual interpretation.

```csharp
public async Task<PredictResponseDto> GeneratePredictionAsync(PredictRequestDto request)
{
    var response = new PredictResponseDto();
    if (request.Values == null || request.Values.Count < 3)
    {
        response.TrendSummary = "⚠️ Not enough data for regression analysis.";
        return response;
    }

    // Linear regression
    var predicted = PerformLinearRegression(request.Values, request.ForecastPoints);
    response.PredictedValues = predicted;

    if (request.UseGPT)
    {
        var prompt = $@"You are an AI data analyst.\nGiven historical metrics: {string.Join(", ", request.Values)}\nand predicted values: {string.Join(", ", predicted)}\nExplain the trend for {request.MetricType} ({request.Provider}).";

        var body = new { model = _model, messages = new[] {
            new { role = "system", content = "You are a data analytics assistant." },
            new { role = "user", content = prompt }
        }};

        var res = await _httpClient.PostAsJsonAsync("https://api.openai.com/v1/chat/completions", body);
        var json = await res.Content.ReadAsStringAsync();
        dynamic parsed = JsonConvert.DeserializeObject(json)!;
        response.TrendSummary = parsed.choices[0].message.content;
    }
    else
    {
        response.TrendSummary = "Prediction generated using regression only.";
    }

    response.GeneratedAt = DateTime.UtcNow;
    return response;
}
```

**Linear Regression Formula:**
```csharp
y = a + b * x
where
b = Cov(x, y) / Var(x)
a = mean(y) − b * mean(x)
```

---

### 3️⃣ API Controller
```csharp
[HttpPost("predict")]
public async Task<IActionResult> Predict([FromBody] PredictRequestDto request)
{
    var result = await _predictiveModelService.GeneratePredictionAsync(request);
    return Ok(result);
}
```

Dependency injection (in `Program.cs`):
```csharp
builder.Services.AddHttpClient<PredictiveModelService>();
```

---

### 4️⃣ React Frontend Enhancement
**New Predict Trend Button + Visualization:**
```jsx
<button
  onClick={predictTrend}
  disabled={loading}
  className="bg-green-600 text-white px-4 py-2 rounded hover:bg-green-700"
>
  {loading ? "Predicting..." : "Predict Trend"}
</button>
```

**Visualization (Recharts):**
```jsx
<ResponsiveContainer width="100%" height={300}>
  <LineChart data={chartData}>
    <CartesianGrid strokeDasharray="3 3" />
    <XAxis dataKey="index" label={{ value: "Data Points", position: "insideBottom" }} />
    <YAxis />
    <Tooltip />
    <Legend />
    <Line type="monotone" dataKey="actual" stroke="#2563eb" name="Historical" strokeWidth={2} />
    <Line type="monotone" dataKey="predicted" stroke="#16a34a" name="Forecast" strokeDasharray="5 5" strokeWidth={2} />
  </LineChart>
</ResponsiveContainer>
```

---

## 📊 Example Test
**Request:**
```json
{
  "provider": "OpenAI",
  "metricType": "Latency",
  "values": [2100, 2400, 3300, 3900],
  "forecastPoints": 3,
  "useGPT": true
}
```

**Response:**
```json
{
  "predictedValues": [4500, 5100, 5700],
  "trendSummary": "Latency is expected to increase steadily, suggesting a mild performance decline.",
  "generatedAt": "2025-10-12T10:15:42Z"
}
```

---

## 📂 Updated Project Structure
```
/AirNir
├── Library
│   └── ArNir.Services.AI
│       ├── InsightEngineService.cs
│       ├── AnomalyDetectionService.cs
│       ├── PredictiveModelService.cs
│       ├── DTOs
│       │   ├── PredictRequestDto.cs
│       │   ├── PredictResponseDto.cs
│       │   └── Insight DTOs
│
├── Presentation
│   ├── ArNir.Api
│   │   └── Controllers/InsightsController.cs
│   └── ArNir.Frontend.React
│       └── src/pages/InsightsPage.jsx
│
└── docs/Phase5.3_PredictiveModel_Architecture.png
```

---

## ✅ Outcomes
| Feature | Result |
|----------|--------|
| 🔮 Statistical Forecasting | Predicts next N values using regression |
| 🧠 GPT Forecasting | Explains predicted trend in natural language |
| 📈 React Chart | Displays actual vs predicted data visually |
| 🧩 Integration | Unified under Insight Engine workflow |
| 🧾 Docs | Architecture + implementation blueprint ready |

---

## 🚀 Next Phase: **5.4 – Narrative Insight Summarizer**
**Goals:**
- Auto-generate a human-readable analytics report combining all insights, anomalies, and forecasts.
- Output a formatted markdown or PDF report.
- Introduce `/api/insights/report` endpoint.

**Deliverables:**
- `NarrativeReportService.cs`
- `/api/insights/report` endpoint
- React export option (Download PDF)
- `Phase5.4_NarrativeSummarizer_Architecture.png`

