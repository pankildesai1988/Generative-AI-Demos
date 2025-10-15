# Phase 5.2 – Anomaly Detection Layer

## ✅ Overview
Phase 5.2 introduces the **Anomaly Detection Layer** into the AirNir analytics pipeline. It extends the Phase 5.1 Insight Engine by performing **statistical anomaly detection** on numeric analytics data (e.g., SLA, latency, feedback). This layer identifies outliers using mean and standard deviation metrics and automatically adjusts sensitivity based on dataset size.

---

## 🎯 Objectives
- Implement centralized **AnomalyDetectionService** for reuse across modules.
- Integrate anomaly detection with **InsightEngineService** results.
- Expose `/api/insights/anomalies` endpoint for direct testing.
- Support **auto-sensitivity** (tighter thresholds for small datasets).
- Display detected anomalies in the **InsightsPage React UI**.

---

## 🧱 Key Deliverables
| Component | Description | Status |
|------------|--------------|--------|
| `AnomalyDetectionService.cs` | Core statistical anomaly detection logic | ✅ |
| `InsightEngineService` update | Integrated anomaly analysis via dependency injection | ✅ |
| `InsightsController` update | Added `/api/insights/anomalies` endpoint | ✅ |
| `InsightsPage.jsx` | Visualized anomalies under GPT summary | ✅ |
| `Phase5.2_AnomalyDetection_Architecture.png` | System-level architecture diagram | ✅ |

---

## ⚙️ Technical Implementation

### 1️⃣ Service: `AnomalyDetectionService.cs`
- Extracts numeric values (`avgLatency`, `slaCompliance`, etc.) from JSON datasets.
- Calculates mean and standard deviation.
- Detects outliers using the formula:
  ```
  mean ± (thresholdMultiplier × stdDeviation)
  ```
- Dynamically adjusts threshold multiplier:
  - `< 5 data points` → `1.0σ` (more sensitive)
  - `≥ 5 data points` → `2.0σ` (standard sensitivity)

**Code Summary:**
```csharp
public List<string> DetectAnomalies(List<double> values, double thresholdMultiplier = 2.0)
{
    double mean = values.Average();
    double stdDev = Math.Sqrt(values.Sum(v => Math.Pow(v - mean, 2)) / values.Count);
    double upper = mean + thresholdMultiplier * stdDev;
    double lower = mean - thresholdMultiplier * stdDev;

    // Detect outliers
    var anomalies = new List<string>();
    for (int i = 0; i < values.Count; i++)
        if (values[i] > upper || values[i] < lower)
            anomalies.Add($"⚠️ Index {i}: {values[i]} (outside range {lower:F2}–{upper:F2})");

    anomalies.Add($"📊 Mean = {mean:F2}, StdDev = {stdDev:F2}, Range = [{lower:F2}, {upper:F2}], Threshold = {thresholdMultiplier:F1}σ");
    if (anomalies.Count == 1)
        anomalies.Insert(0, "✅ No anomalies detected.");

    return anomalies;
}
```

---

### 2️⃣ Integration with Insight Engine
`InsightEngineService` now receives `AnomalyDetectionService` through dependency injection:

```csharp
public InsightEngineService(HttpClient httpClient, IConfiguration config, AnomalyDetectionService anomalyDetection)
{
    _httpClient = httpClient;
    _config = config;
    _anomalyDetection = anomalyDetection;
}
```

Then, after GPT summary generation:
```csharp
if (!string.IsNullOrEmpty(request.DataJson))
{
    var anomalies = _anomalyDetection.AnalyzeJson(request.DataJson);
    responseDto.Anomalies = anomalies;
}
```

---

### 3️⃣ API Endpoint
New route added in `InsightsController`:
```csharp
[HttpPost("anomalies")]
public IActionResult DetectAnomalies([FromBody] object payload)
{
    string json = payload?.ToString() ?? string.Empty;
    var results = _anomalyDetection.AnalyzeJson(json);
    return Ok(new { Count = results.Count, Details = results });
}
```

---

### 4️⃣ React Frontend Enhancement
`InsightsPage.jsx` updated to display detected anomalies:
```jsx
{anomalies.length > 0 && (
  <div className="mt-4 p-3 bg-red-50 border border-red-200 rounded">
    <h4 className="font-semibold text-red-600 mb-1">Anomalies Detected:</h4>
    <ul className="list-disc pl-5 text-red-700">
      {anomalies.map((a, i) => (<li key={i}>{a}</li>))}
    </ul>
  </div>
)}
```

---

## 📊 Example Run
**Input JSON:**
```json
[
  {"provider":"OpenAI","avgLatency":2100},
  {"provider":"Claude","avgLatency":2400},
  {"provider":"Gemini","avgLatency":8900}
]
```

**Output Response:**
```json
{
  "summary": "OpenAI and Claude are consistent; Gemini is an outlier.",
  "anomalies": [
    "⚠️ Index 2: 8900 (outside range 1209.44–7723.89)",
    "📊 Mean = 4466.67, StdDev = 3257.23, Range = [1209.44, 7723.89], Threshold = 1.0σ"
  ],
  "generatedAt": "2025-10-11T20:05:22Z"
}
```

---

## 📂 Project Structure (Updated)
```
/AirNir
├── Library
│   └── ArNir.Services.AI
│       ├── InsightEngineService.cs
│       ├── AnomalyDetectionService.cs
│       ├── InsightRequestDto.cs
│       └── InsightResponseDto.cs
│
├── Presentation
│   ├── ArNir.Api
│   │   └── Controllers/InsightsController.cs
│   └── ArNir.Frontend.React
│       └── src/pages/InsightsPage.jsx
│
└── docs/Phase5.2_AnomalyDetection_Architecture.png
```

---

## ✅ Outcomes
- 🔹 Reliable anomaly detection integrated with AI summaries.
- 🔹 Auto-sensitivity prevents missed outliers in small datasets.
- 🔹 Frontend displays outlier diagnostics alongside GPT results.
- 🔹 Architecture ready for predictive extensions in Phase 5.3.

---

## 🚀 Next Phase: **5.3 – Predictive Model Service**
**Goals:**
- Forecast future metrics (e.g., latency, SLA) based on historical data.
- Use GPT reasoning + regression or moving average for predictions.
- Add `/api/insights/predict` endpoint and React visualization (trend chart).

**Deliverables:**
- `PredictiveModelService.cs`
- `/api/insights/predict` API
- `InsightsPage.jsx` prediction chart integration
- `Phase5.3_PredictiveModel_Architecture.png`

