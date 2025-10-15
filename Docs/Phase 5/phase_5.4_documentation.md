# Phase 5.4 – Narrative Insight Summarizer

## ✅ Overview
Phase 5.4 introduces the **Narrative Insight Summarizer** — a natural‑language reporting layer that converts analytical outputs (insights, anomalies, and predictions) into **readable AI‑generated reports**.  This phase finalizes the Insight Engine pipeline, producing coherent Markdown or PDF summaries suitable for business reviews and executive dashboards.

---

## 🎯 Objectives
- Aggregate data from previous analytical phases.
- Use GPT models to generate structured Markdown reports.
- Provide `/api/insights/report` endpoint for AI‑generated narrative summaries.
- Integrate the React frontend with a **“Generate Report”** button and PDF export.

---

## 🧱 Key Deliverables
| Component | Description | Status |
|------------|-------------|--------|
| `NarrativeReportService.cs` | GPT‑driven report composer | ✅ |
| `NarrativeReportDtos.cs` | DTO definitions | ✅ |
| `/api/insights/report` | API route for narrative generation | ✅ |
| `InsightsPage.jsx` | Frontend integration + PDF export | ✅ |
| `Phase5.4_NarrativeSummarizer_Architecture.png` | Architecture diagram | ✅ |

---

## ⚙️ Technical Implementation

### 1️⃣ DTOs
```csharp
public class NarrativeReportRequestDto {
    public string? Provider { get; set; }
    public string? MetricType { get; set; }
    public string? Insights { get; set; }
    public List<string>? Anomalies { get; set; }
    public List<double>? Predictions { get; set; }
    public string? TrendSummary { get; set; }
}

public class NarrativeReportResponseDto {
    public string? ReportMarkdown { get; set; }
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
}
```

---

### 2️⃣ Service Logic – `NarrativeReportService.cs`
```csharp
public async Task<NarrativeReportResponseDto> GenerateAsync(NarrativeReportRequestDto dto)
{
    var response = new NarrativeReportResponseDto();
    var prompt = $@"You are an AI analytics assistant. Combine insights, anomalies, and predictions below into a structured Markdown report.

Provider: {dto.Provider}
Metric Type: {dto.MetricType}
Insights: {dto.Insights}
Anomalies: {string.Join("\n", dto.Anomalies ?? new())}
Predictions: {string.Join(", ", dto.Predictions ?? new())}
Trend Summary: {dto.TrendSummary}";

    _httpClient.DefaultRequestHeaders.Clear();
    _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");

    var body = new {
        model = _model,
        messages = new[] {
            new { role = "system", content = "You are a professional AI report generator." },
            new { role = "user", content = prompt }
        }
    };

    var result = await _httpClient.PostAsJsonAsync("https://api.openai.com/v1/chat/completions", body);
    var json = await result.Content.ReadAsStringAsync();
    dynamic parsed = JsonConvert.DeserializeObject(json)!;
    response.ReportMarkdown = parsed.choices[0].message.content;
    response.GeneratedAt = DateTime.UtcNow;
    return response;
}
```

---

### 3️⃣ API Endpoint
```csharp
[HttpPost("report")]
public async Task<IActionResult> GenerateReport([FromBody] NarrativeReportRequestDto dto)
{
    var result = await _narrativeReportService.GenerateAsync(dto);
    return Ok(result);
}
```

`Program.cs`
```csharp
builder.Services.AddHttpClient<NarrativeReportService>();
```

---

### 4️⃣ React Frontend Integration (`InsightsPage.jsx`)
Key Additions:
- **Generate Report** button → calls `/api/insights/report`.
- **Markdown preview** with `id="report-preview"`.
- **Export PDF** feature via `jspdf` + `html2canvas`.

```jsx
<button onClick={generateReport} className="bg-purple-600 text-white px-4 py-2 rounded hover:bg-purple-700">
  {loading ? "Generating..." : "Generate Report"}
</button>
{report && (
  <button onClick={exportToPDF} className="bg-gray-700 text-white px-4 py-2 rounded hover:bg-gray-800">
    Export PDF
  </button>
)}
```

---

## 🧪 Example Test
**Request:**
```json
{
  "provider": "OpenAI",
  "metricType": "Latency",
  "insights": "OpenAI and Claude maintained stable performance.",
  "anomalies": ["Gemini latency spike detected."],
  "predictions": [4600, 5500, 6400],
  "trendSummary": "Latency expected to increase gradually."
}
```

**Response:**
```markdown
# AI Latency Metrics Report
## Executive Summary
This report analyzes latency metrics for OpenAI, Claude, and Gemini.
...
```

---

## 📈 Example Output (from test)
*(Excerpt from your live result)*

```markdown
# AI Latency Metrics Report

## Executive Summary
This report analyzes the average latency metrics for leading AI models ...

## Detected Anomalies
- Gemini latency (3900 ms) lies outside expected range (2012.60–3587.40).

## Forecast
Predicted latencies: 4600 ms, 5500 ms, 6400 ms.

## Recommendations
1. Optimize Gemini performance.
2. Benchmark against OpenAI.
3. Upgrade infrastructure.
```

---

## 📂 Updated Project Structure
```
/AirNir
├── Library/ArNir.Services.AI
│   ├── InsightEngineService.cs
│   ├── AnomalyDetectionService.cs
│   ├── PredictiveModelService.cs
│   └── NarrativeReportService.cs
├── Core/DTOs/AI
│   ├── PredictRequestDto.cs
│   ├── PredictResponseDto.cs
│   └── NarrativeReportDtos.cs
├── Presentation/ArNir.Api
│   └── Controllers/InsightsController.cs
└── Presentation/ArNir.Frontend.React/src/pages/InsightsPage.jsx
```

---

## ✅ Outcomes
| Feature | Result |
|----------|---------|
| 🧠 GPT‑based Narrative Generator | Converts analytics into professional Markdown |
| 📄 Markdown + PDF Export | Executives can download formatted reports |
| 🔗 Unified Insight Workflow | Combines Phases 5.1 → 5.4 seamlessly |
| 🧩 Modular Design | Each service isolated, testable, and reusable |
| 📊 Visual + Textual Output | Completes full analytical feedback loop |

---

## 🚀 Next Phase – 5.5 : Automated Insight Delivery
**Goals:**
- Schedule automated report generation (daily/weekly).
- Email or Slack delivery of generated Markdown/PDF.
- SLA alerting & insight trend summaries.

**Deliverables:**
- `InsightDeliveryService.cs`
- `/api/insights/schedule` endpoint
- Notification integration (SMTP + Slack Webhooks)
- `Phase5.5_InsightDelivery_Architecture.png`

