# 📜 Changelog – Generative AI Mentor Project

All notable changes to this project will be documented in this file.

---

## [Phase 3.3] – Retrieval Service (Completed)
### Added
- Implemented `IRetrievalService` with **Semantic (pgvector)**, **Keyword (SQL Server FTS)**, and **Hybrid Search**.
- Hybrid fallback: defaults to semantic-only when keyword hits = 0.
- Admin Debug UI with filters, counters, SLA monitoring (<300ms), and source tagging.
- Query sanitization (tokenized, escaped) for FTS to avoid SQL errors.
- Updated **AirNir Project Structure** with Core (Validations), Services (Helpers, Mapping), and Admin UI (Controllers, ViewModels, Views).
- Updated Knowledge Base (Markdown + Word) and GitHub README with Phase 3.3 details and architecture diagram.

---

## [Phase 3.2] – Embeddings & Vector Storage (Completed)
### Added
- Configured **Postgres with pgvector** for vector storage.
- Implemented `EmbeddingService` to generate and store embeddings from chunks.
- Added Admin test UI for embeddings & semantic search (direct DI calls, no API endpoints).
- Verified vectors persisted in Postgres and similarity search working.
- Architecture diagram updated.

---

## [Phase 3.1] – Document Ingestion & Chunking (Completed)
### Added
- Document upload (PDF, DOCX, TXT, Markdown) with validation.
- Chunking by semantic boundaries, whitespace cleaning, and text normalization.
- Storage in SQL Server (Documents, DocumentChunks).
- AdminLTE UI: Upload, Delete, Details (preview with PDF inline, TXT chunks, DOCX fallback download).

---

## [Phase 2.4] – Session Cloning & Cross-Model Comparisons (Completed)
### Added
- `ComparisonService` with pluggable LLM providers (OpenAI, Gemini, Claude-ready).
- Admin panel: comparison page with model/provider dropdowns, input field, run button, and results grid.
- Comparison history page with DataTables view and modal details.
- Deduplication filter in JS.
- Persisted comparison results in SQL Server.

---

## [Phase 2.3] – Admin Panel for Prompt Templates (Completed)
### Added
- AdminLTE integration with authentication (JWT).
- CRUD for prompt templates with parameterized prompts.
- Live preview with validation + error tooltips.
- Versioning: history, rollback, and compare.

---

## [Phase 2.2] – Prompt Templates + Clean UI (Completed)
### Added
- Templates stored in DB with parameters (tone, length).
- `buildPrompt()` and `buildPromptPreview()` logic.
- Modular frontend JS with streaming + typing dots.

---

## [Phase 2.1] – Backend & Frontend Integration (Completed)
### Added
- .NET Core Web API (`ChatController`) with session management.
- Persistence in SQL Server (ChatSessions + ChatMessages).
- Frontend with modular JS and session sidebar.
- Azure App Service + SQL Azure deployment.

---

## [Phase 1] – Foundation (Completed)
### Added
- Explored Generative AI use cases in .NET apps (chatbots, summarization, code assist).
- Tested HuggingFace models for API demo + Azure deployment.
- Learned prompt engineering: zero-shot, few-shot, role prompting.
