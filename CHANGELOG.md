# 📜 CHANGELOG – Generative AI Mentor Project

This changelog documents notable updates and improvements across all phases. Current status: **Phase 3.7 completed** ✅

---

## [Phase 3.7] – Advanced Analytics (✅ Completed)
### Added
- **Multi-provider support** → OpenAI, Gemini, Claude.
- **Model selection** in RAG Comparison + History.
- **SLA compliance tracking** per provider/model + per prompt style.
- **Token tracking** → `QueryTokens`, `ContextTokens`, `TotalTokens`.
- **Analytics Enhancements**:
  - KPI widgets (Total Runs, Avg Latency, SLA %).
  - SLA compliance by PromptStyle.
  - SLA compliance per Provider/Model.
  - Latency breakdowns per Provider/Model.
  - PromptStyle usage distribution.
  - Latency trends (daily averages).
- **Database Migration**: added token fields in `RagComparisonHistories`.

### Changed
- **RagService** → dispatch by provider, SLA check (5000 ms threshold), token counting via SharpToken.
- **RagHistoryService** → DTOs extended with token counts.
- **Frontend Updates** → `rag-comparison.js`, `rag-history.js`, `analytics.js` updated for provider/model selection + analytics visualizations.
- **Admin UI** → new Analytics Dashboard views + charts.

---

## [Phase 3.6] – Analytics Dashboard
### Added
- SLA & latency tracking.
- KPI metrics: SLA %, Avg Latency.
- Charts: SLA compliance, Latency trends, PromptStyle usage.
- Drill-down navigation from Analytics → History.

---

## [Phase 3.5] – RAG History + Docs Module
### Added
- History logging with SLA, latency, answers.
- Filters: SLA, date, provider, model, prompt style.
- Side-by-side run comparison.
- CSV/Excel export for history.
- Docs module for document lifecycle.

---

## [Phase 3.4] – Prompt Engineering
### Added
- Support for Zero-Shot, Few-Shot, Role, RAG, Hybrid prompts.
- Admin UI for prompt template CRUD + versioning.

---

## [Phase 3.3] – Retrieval Service
### Added
- Semantic, keyword, hybrid retrieval.
- Admin Debug UI for retrieval comparison.

---

## [Phase 3.2] – Embeddings + Vector DB
### Added
- OpenAI embeddings.
- Postgres + pgvector integration.
- Semantic retrieval queries.

---

## [Phase 3.1] – Ingestion Layer
### Added
- Document upload + chunking.
- SQL storage for documents & metadata.

---

## 📍 Roadmap
- ✅ Phase 1: Foundation (use cases, HuggingFace, Azure, prompt engineering)
- ✅ Phase 2: .NET Backend + Frontend Integration
- ✅ Phase 3.1 → 3.7: RAG Enhancements + Analytics
- ⏳ Phase 3.8: Advanced Exports + Token vs Latency Analytics

