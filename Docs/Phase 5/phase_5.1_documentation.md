# Phase 5.1 – Insight Engine Implementation

## ✅ Overview
Phase 5.1 marks the first step in the **AI-Driven Insights & Automation (Phase 5.0)** roadmap. This phase introduces the **Insight Engine Service**, an OpenAI-powered component that converts analytics data (SLA, latency, feedback) into narrative insights. It transforms raw metrics into meaningful summaries and recommendations viewable via the new `/insights` React route.

---

## 🎯 Objectives
- Integrate **OpenAI GPT models** into the analytics stack.
- Generate **textual summaries** explaining provider/model performance.
- Support **prompt-based reasoning** for latency, SLA, and feedback datasets.
- Deliver insights via both **API** and **React UI**.
- Establish the foundation for anomaly detection and predictive modules (5.2–5.3).

---

## 🧱 Key Deliverables

| Component | Description | Status |
|------------|--------------|--------|
| `InsightEngineService.cs` | Calls OpenAI API, builds prompts, returns narrative insights | ✅ |
| `InsightRequestDto` / `InsightResponseDto` | Standardized data exchange for analytics input/output | ✅ |
| `InsightsController.cs` | `/api/insights/analyze` endpoint for frontend + API clients | ✅ |
| `InsightsPage.jsx` | React interface for manual insight generation | ✅ |
| `vite.config.js` update | Proxy `/api` → `.NET Backend` to bypass CORS | ✅ |

---

## ⚙️ Technical Implementation

### 1️⃣ DTOs
```csharp
public class InsightRequestDto {
    public string? Provider { get; set; }
    public string? Model { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? MetricType { get; set; }
    public string? DataJson { get; set; }
}

public class InsightResponseDto {
    public string Summary { get; set; } = string.Empty;
    public List<string>? Anomalies { get; set; }
    public List<string>? Recommendations { get; set; }
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
}
```

### 2️⃣ Service Logic (`InsightEngineService.cs`)
- Injected via `HttpClient` + `IConfiguration`.
- Builds structured GPT prompt with provider/model context.
- Sends `POST` request to `https://api.openai.com/v1/chat/completions`.
- Extracts narrative from `choices[0].message.content`.

### 3️⃣ API Controller
```csharp
[HttpPost("analyze")]
public async Task<IActionResult> Analyze([FromBody] InsightRequestDto request)
{
    if (string.IsNullOrWhiteSpace(request.DataJson))
        return BadRequest("DataJson is required.");

    var result = await _insightEngine.GenerateInsightsAsync(request);
    return Ok(result);
}
```

### 4️⃣ React UI (`InsightsPage.jsx`)
- Simple text-area input for analytics JSON.
- Calls `/api/insights/analyze` via Axios.
- Displays generated summary in a formatted panel.

---

## 🔗 Data Flow
```
[React Insights Page]
   ↓
[Axios → /api/insights/analyze]
   ↓
[InsightsController → InsightEngineService]
   ↓
[OpenAI GPT → Insight Summary]
   ↓
[JSON Response → UI Display]
```

---

## 🧪 Testing Summary
- ✅ Local API verified at `https://localhost:5001/api/insights/analyze`.
- ✅ End-to-end test from React UI successful.
- ✅ Example input:
  ```json
  [{"provider":"OpenAI","avgLatency":2100},{"provider":"Claude","avgLatency":3300}]
  ```
  **Output:** "OpenAI shows lower latency and stronger SLA compliance. Claude exhibits slower responses; optimize context length."

---

## 📂 Project Structure (Updated)
```
/AirNir
├── Library
│   └── ArNir.Services.AI
│       ├── InsightEngineService.cs
│       ├── InsightRequestDto.cs
│       └── InsightResponseDto.cs
│
├── Presentation
│   ├── ArNir.Api
│   │   └── Controllers/InsightsController.cs
│   └── ArNir.Frontend.React
│       └── src/pages/InsightsPage.jsx
│
└── docs/Phase5.1_InsightEngine_Architecture.png
```

---

## ✅ Outcomes
- 🔹 Fully functional OpenAI integration for analytics interpretation.
- 🔹 Configurable via `appsettings.json` (API Key + Model).
- 🔹 React UI and API both operational.
- 🔹 Base layer established for:
  - Phase 5.2 – Anomaly Detection Service.
  - Phase 5.3 – Predictive Model Service.
  - Phase 5.4 – Narrative Report Integration.
  - Phase 5.5 – Alert Notifications.

---

## 🚀 Next Phase: **5.2 – Anomaly Detection Layer**
**Goals:**
- Identify outliers in SLA/latency using moving averages.
- Mark anomalies in Insight responses.
- Extend database schema for storing flagged events.

**Deliverables:** `AnomalyDetectionService.cs`, `/api/insights/anomalies`, and React anomaly chart visualization.