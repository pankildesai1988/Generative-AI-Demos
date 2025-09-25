# 📘 CHANGELOG

## [Phase 1] – Foundation
- Explored AI use cases in .NET apps (chatbots, Q&A, summarization, content generation, code assist).
- Tested HuggingFace models with Azure deployment.
- Learned Prompt Engineering basics (Zero-Shot, Few-Shot, Role Prompting).

## [Phase 2.1] – Backend Integration
- Implemented .NET Core Web API with ChatController endpoints.
- Added services layer (`IOpenAiService`, `IChatHistoryService`).
- SQL Server persistence for sessions & messages.

## [Phase 2.2] – Frontend Integration
- Modularized JS: chat.js, sessions.js, templates.js, utils.js, main.js.
- Features: streaming, typing dots, session sidebar, model selector, prompt preview.

## [Phase 2.3] – Deployment
- Deployed via Azure App Service + SQL Azure.
- Fixed CORS + connection string issues.

## [Phase 2.4] – Prompt Templates & Clean UI
- Templates stored in DB with parameters.
- Live preview of templates with parameter insertion.
- Admin panel for CRUD templates.

---

## [Phase 3.1] – Architecture Foundations
- Designed modular RAG architecture.
- DTOs for retrieval results and responses.
- Integrated retrieval pipelines with OpenAI abstraction.

## [Phase 3.2] – RAG Service Integration
- Implemented RetrievalService with hybrid semantic search.
- Added OpenAI completions with prompt building.
- DTOs for consistent data flow.

## [Phase 3.3] – Admin UI for Comparisons
- AdminLTE-based comparison page.
- Side-by-side provider/model comparisons.
- Comparison history with details modal.

## [Phase 3.4] – Prompt Engineering
- Added **advanced prompt engineering**:
  - Zero-Shot, Few-Shot, Role, RAG-Augmented, Hybrid Role+RAG.
- Integrated dynamic prompt generation into RAG service.
- Extended DB schema with `PromptStyle` column.
- Prompt experimentation enabled in Admin UI.

## [Phase 3.5] – RAG History Enhancements & Docs Module
- **RAG Comparison Page**:
  - Query + PromptStyle selector.
  - Results: Baseline vs RAG answers.
  - Expandable retrieved context.
  - SLA badge for latency.
- **History Page**:
  - Filters: SLA, date, query, PromptStyle.
  - Details modal with chunks.
  - Compare Mode for multiple runs.
  - Export CSV/Excel (single/multiple runs).
- **Docs Module**:
  - Upload new docs
  - Edit/Delete existing docs
  - Rebuild embeddings on demand
- Migrated Admin UI to Bootstrap 5.

## [Phase 3.6] – Analytics & Insights
- Added **Analytics Dashboard** with Chart.js visualizations:
  - SLA Compliance (Pie)
  - Average Latencies (Bar)
  - PromptStyle Distribution (Pie)
  - SLA & Latency Trends (Line)
- Implemented **filters**: date range, SLA status, PromptStyle.
- Added **drill-down navigation** from Analytics ➝ RAG History.
- Updated KnowledgeBase, README, and Architecture diagrams.

---

## ✅ Current Status
- Phase 1 → Phase 3.6 completed.
- Ready to begin **Phase 3.7 – Advanced Analytics**:
  - Provider/Model analytics
  - Export datasets (CSV/Excel)
  - KPI Widgets
  - Drill-down enhancements
