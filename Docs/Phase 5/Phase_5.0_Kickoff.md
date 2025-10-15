# Phase 5.0 – AI‑Driven Insights & Automation (Kickoff Plan)

## 📅 Project Context
- **Phase Start:** 2025‑10‑17  
- **Previous:** Phase 4.0 (Analytics & ClosedXML Reports)  
- **Next:** Phase 5.1 (Insight Engine Implementation)

---

## 🎯 Vision
AirNir evolves from a data‑reporting tool into an **AI‑driven observability platform** capable of analyzing, interpreting, and predicting system performance.

OpenAI GPT models will serve as the primary reasoning layer, transforming raw metrics into meaningful insights, summaries, and alerts.

---

## 🧱 High‑Level Objectives

| # | Objective | Description |
|---|------------|-------------|
| 1 | Insight Generation | Use GPT to summarize analytics trends and explain provider/model performance. |
| 2 | Anomaly Detection | Detect unusual latency spikes or SLA breaches automatically. |
| 3 | Predictive Analytics | Forecast future SLA and latency trends using historical data. |
| 4 | Narrative Reports | Generate natural‑language summaries for Admin exports. |
| 5 | Smart Alerts | Trigger email/Teams notifications for critical anomalies. |

---

## ⚙️ Technical Approach

### 🧠 Primary AI Engine
- **Provider:** OpenAI GPT (through official API)  
- **Model Target:** `gpt‑4o‑mini` (for fast, cost‑efficient insight generation)  
- **Integration:** via `ArNir.Services.AI.InsightEngineService` with configurable provider support.

### 🧩 Service Layer Additions

| Service Class | Responsibility |
|----------------|----------------|
| `InsightEngineService` | Generate summaries & recommendations via OpenAI API |
| `AnomalyDetectionService` | Identify outliers in analytics datasets |
| `PredictiveModelService` | Perform simple forecasting (e.g., SLA trends) |
| `ReportSummarizerService` | Attach GPT‑generated narrative to exported reports |
| `AlertNotificationService` | Dispatch email alerts for critical events |

---

## 🧩 Module Breakdown (5.1 → 5.5)

| Sub‑Phase | Module | Description |
|-------------|---------|--------------|
| **5.1** | Insight Engine Service | Generate text insights using GPT from analytics datasets. |
| **5.2** | Anomaly Detection Layer | Compare metrics against moving averages + thresholds. |
| **5.3** | Predictive Performance | Forecast future latency/SLA with regression or GPT‑based predictions. |
| **5.4** | Narrative Report Summaries | Embed AI narratives in ClosedXML exports. |
| **5.5** | Smart Alerting System | Send alerts via email or Teams based on insight thresholds. |

---

## 📂 Proposed Project Structure

```
/AirNir
├── Library
│   └── ArNir.Services.AI
│      ├── InsightEngineService.cs
│      ├── AnomalyDetectionService.cs
│      ├── PredictiveModelService.cs
│      ├── ReportSummarizerService.cs
│      └── AlertNotificationService.cs
│
├── Presentation
│   ├── ArNir.Admin
│   │   └── Controllers
│   │      └── AIReportsController.cs
│   └── ArNir.Frontend.React
│      └── src/pages/InsightsPage.jsx
│
└── docs
    └── Phase5.0_Architecture.png
```

---

## 🔗 Integration Flow

```
[React Insights Dashboard]
   ↓ (API Request)
[/api/insights/analyze]
   ↓
[InsightEngineService]
   ↓ (OpenAI GPT API → Response)
   ↓
[Summarized Insights + Anomaly Flags → UI or Reports]
```

---

## 🧠 Sample GPT Prompt Template

```json
{
 "role": "system",
 "content": "You are an AI assistant analyzing provider performance data to summarize key patterns and issues."
}
```
```json
{
 "role": "user",
 "content": "Given this dataset of average latency and SLA compliance per provider, summarize anomalies, top performers, and suggest optimizations."
}
```

---

## ⚙️ Configuration (Backend appsettings.json)

```json
"OpenAI": {
  "ApiKey": "YOUR_API_KEY_HERE",
  "Model": "gpt‑4o‑mini",
  "BaseUrl": "https://api.openai.com/v1/"
}
```

---

## 📊 Expected Outputs
- **Insight Summary:** Plain text generated by GPT describing patterns and issues.  
- **Anomaly List:** Flagged entries with reasons.  
- **Predicted Metrics:** Forecast values for next period.  
- **Narrative Reports:** Appended AI‑written summary to Excel exports.  
- **Alerts:** Emails when SLA drops below threshold.  

---

## 🚀 Roadmap and Timeline

| Phase | Deliverable | ETA |
|--------|---------------|------|
| 5.1 | InsightEngineService + API | Week 1 |
| 5.2 | AnomalyDetectionService | Week 2 |
| 5.3 | PredictiveModelService | Week 3 |
| 5.4 | Narrative Reports Integration | Week 4 |
| 5.5 | Smart Alerting System + Email Module | Week 5 |

---

## 🧱 Dependencies

- **OpenAI .NET SDK** or direct HTTP client calls.  
- **ClosedXML** (for report attachment integration).  
- **System.Net.Mail** or SendGrid (Azure) for notifications.  
- **Hangfire/Quartz** (optional for scheduled AI reports).  

---

## 🧩 Future Considerations

- Add optional **local LLM fallback** (Ollama or Hugging Face).  
- Enable prompt templates and parameter tuning via Admin panel.  
- Store AI insight logs for auditing and training.  
- Integrate graph visual summaries on the Insights Dashboard.  

---

**Author:** AirNir AI Engineering Team  
**Version:** Phase 5.0 Kickoff Plan  
**Date:** 2025‑10‑17
