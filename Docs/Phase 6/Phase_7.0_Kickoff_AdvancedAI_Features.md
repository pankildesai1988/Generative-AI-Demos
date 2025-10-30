# Phase 7.0 – Advanced AI Intelligence & NLP Kickoff

### 🎯 Objective
Build on the existing Intelligence Dashboard with **Natural Language Querying (NLP)**, **Predictive Forecast Enhancements**, and **Contextual AI Insight Generation**.

---

## 🧱 Architecture Blueprint
```
/ArNir.Services.AI/
├── LlmService.cs
├── NaturalLanguageCommandService.cs
├── InsightEngineService.cs
├── PredictiveTrendService.cs
├── ChatInsightService.cs
├── InsightHistoryService.cs
└── DTOs/
    ├── InsightItemDto.cs
    ├── ChatRequestDto.cs
    ├── ChartSeriesItemDto.cs
```

---

## 🧠 Advanced Features Plan
| Feature | Description | Output |
|----------|--------------|--------|
| NLP Query Engine | Understands “Show OpenAI latency for last 7 days” | Parsed intent object |
| AI Comparison | Week-over-week provider comparison | Plain English analysis |
| Predictive Forecasting | Rolling average with confidence intervals | 7-day trend prediction |
| Insight Feed | Real-time recommendations | Feed of AI-generated insights |
| Contextual Chat | Memory-driven responses | Ongoing conversation context |
| Smart Export | Natural-language triggered data export | “Export Gemini data as Excel” |

---

## 🌐 API Endpoints
| Endpoint | Method | Purpose |
|-----------|---------|----------|
| `/api/intelligence/query` | POST | Handles NLP + GPT queries |
| `/api/intelligence/compare` | GET | Compare week vs week |
| `/api/intelligence/forecast` | GET | Forecast provider performance |
| `/api/intelligence/insights` | GET | Fetch AI insight summaries |
| `/api/intelligence/export/nlp` | POST | Export via natural query |

---

## 🧩 Development Roadmap
| Subphase | Task | Deliverable |
|-----------|-------|-------------|
| 7.0.1 | NLP Command Engine | Detects compare/filter/forecast |
| 7.0.2 | Predictive Forecast Enhancements | Confidence + delta analysis |
| 7.0.3 | AI Chat Integration | GPT + Analytics hybrid chat |
| 7.0.4 | Context Memory | Persistent chat history |
| 7.0.5 | Insight Feed | AI recommendations |
| 7.0.6 | Smart Export | NLP-based export actions |

---

## 📘 Summary
Phase 7 begins with **Advanced AI & NLP**.  
The project now transitions from analytics visualization to **intelligent understanding and contextual predictions**, using LLM-based trend analysis and user-driven insights.
