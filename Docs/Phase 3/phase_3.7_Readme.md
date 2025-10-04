# Phase 3.7 – Advanced Analytics Documentation

## ✅ Overview
Phase 3.7 builds on the RAG pipeline (Phase 3.6) and introduces **Advanced Analytics** to measure and compare performance across providers, models, and prompt styles. This phase also adds **SLA compliance tracking**, **latency breakdowns**, and **export capabilities**.

---

## 🔹 Key Features Implemented

### 1. Multi-Provider Support
- **OpenAI**, **Gemini**, **Claude** providers integrated.
- Defaulted to **low-latency models**:
  - OpenAI → `gpt-4o-mini`
  - Gemini → `gemini-2.0-flash`
  - Claude → `claude-3-haiku`
- Model selection exposed in UI (RAG Comparison + History).

### 2. SLA Compliance Tracking
- Introduced `IsWithinSla` flag in `RagComparisonHistory`.
- SLA threshold set to **5000 ms**.
- SLA compliance calculated per:
  - **PromptStyle** → % queries answered within SLA.
  - **Provider/Model** → SLA compliance rates.

### 3. Token Tracking
- Added token counting with **SharpToken** (`cl100k_base` encoding).
- New fields in `RagComparisonHistory`:
  - `QueryTokens`
  - `ContextTokens`
  - `TotalTokens`
- Token counts saved for every run.
- Enabled future correlation between **token size** and **latency/SLA**.

### 4. Analytics Service Enhancements
All analytics methods return **`AnalyticsResponse<T>`** wrapper with:
- `Data` (list of DTOs)
- `TotalCount`
- `StartDate`, `EndDate`
- `PromptStyle`, `SlaStatus`

#### Implemented Methods
- `GetAverageLatenciesAsync` → Avg retrieval, LLM, total latency per provider/model.
- `GetSlaComplianceAsync` → SLA % per prompt style.
- `GetPromptStyleUsageAsync` → Usage distribution of prompt styles.
- `GetTrendsAsync` → Daily avg latency trends.
- `GetProviderAnalyticsAsync` → SLA %, latency, run counts by provider/model.

### 5. Frontend Enhancements
#### Analytics Dashboard (`Index.cshtml` + `analytics.js`)
- **Filters**: date range, prompt style, SLA status.
- **KPI Widgets**: total runs, avg latency, SLA %.
- **Charts**:
  - SLA compliance by prompt style (pie).
  - Avg latency by provider/model (bar).
  - Prompt style usage (pie).
  - Latency trends (line).
  - Avg latency per provider/model (bar).
  - SLA compliance per provider/model (bar).

#### RAG Comparison + History Updates
- Provider + model selection integrated.
- History export filters extended (include provider, model).
- SLA status displayed in history list.

### 6. Database Migration
Added new columns to `RagComparisonHistories`:
```sql
ALTER TABLE RagComparisonHistories
ADD QueryTokens INT NOT NULL DEFAULT 0,
    ContextTokens INT NOT NULL DEFAULT 0,
    TotalTokens INT NOT NULL DEFAULT 0;
```

---

## 📂 Project Structure (Phase 3.7)
```
/AirNir
├── Library
│   ├── ArNir.Core
│   │   ├── DTOs/Analytics (AvgLatencyDto, SlaComplianceDto, ProviderAnalyticsDto, TrendDto, PromptStyleUsageDto)
│   │   └── Utils/TokenizerUtil.cs
│   ├── ArNir.Data (DbContext + EF migrations)
│   └── ArNir.Services (RagService, RagHistoryService, Providers)
│
├── Presentation
│   ├── ArNir.Admin
│   │   ├── Controllers (AnalyticsController, RagHistoryController, RagComparisonController)
│   │   ├── Views
│   │   │   ├── Analytics/Index.cshtml
│   │   │   ├── RagHistory/Index.cshtml
│   │   │   └── RagComparison/Index.cshtml
│   │   └── wwwroot/js (analytics.js, rag-history.js, rag-comparison.js)
│   └── ArNir.Frontend (planned end-user chat/search UI)
│
├── sql
│   ├── update_rag_history.sql (migration for token fields)
│
└── docs
    ├── Phase3.7_Architecture.png
    ├── Phase3.7_Documentation.md ← (this file)
    └── GenerativeAI_KnowledgeBase.md
```

---

## ✅ Outcomes
- SLA compliance and latency analytics are now measurable.
- Token counts provide visibility into why responses are slow.
- Dashboard supports drill-down navigation from analytics → history.
- Foundation set for **Phase 3.8**: Token vs Latency Analytics, Export (CSV/Excel), and advanced filtering.

---

## ⏭ Next Steps (Phase 3.8)
- Add **Export to CSV/Excel** for Analytics datasets.
- Visualize **token count vs latency correlation**.
- Add **filter chaining (multi-select providers/models)**.
- Enhance KPI widgets with token-based insights.
- SLA thresholds configurable per provider/model.

