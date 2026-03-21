# ArNir — Architecture Reference

## Overview

ArNir is an enterprise RAG (Retrieval-Augmented Generation) platform built on .NET 9. It ingests documents, creates vector embeddings, retrieves relevant context via pgvector, generates answers through multiple LLM providers, and automatically evaluates response quality using an LLM-as-judge pattern.

---

## Solution Structure (14 Projects)

```
ArNir.sln
  ArNir.Core/              Entities, DTOs, shared interfaces, utilities
  ArNir.Data/              EF Core (ArNirDbContext + VectorDbContext), migrations, repositories
  ArNir.Platform/          PromptStyleEnum, configuration models
  ArNir.Services/          Business logic: RagService, LLM providers, evaluation, analytics
  ArNir.RAG/               Ingestion pipeline, parsers (PDF/DOCX/TXT), chunkers, background worker
  ArNir.RAG.Pgvector/      Real pgvector embeddings (PgvectorDocumentEmbedder + PgvectorDocumentVectorStore)
  ArNir.Memory/            IEpisodicMemory, ISemanticMemory (chat context persistence)
  ArNir.PromptEngine/      IPromptResolver chain (CodePromptResolver), IPromptVersionStore
  ArNir.Agents/            IPlannerAgent (autonomous tool-calling agent)
  ArNir.Tools/             DocumentLookupTool, WebFetchTool (IToolRegistry)
  ArNir.Observability/     IMetricCollector, SlaAlertRule, IEvaluationService interface
  ArNir.Admin/             ASP.NET MVC admin panel (AdminLTE 4), 19 controllers, cookie auth
  ArNir.API/               ASP.NET Web API (Swagger), 12 controllers, 30+ REST endpoints
  ArNir.Tests/             xUnit + Moq (72 tests across 8 sprints)
```

---

## Data Flow

### RAG Pipeline

```
User Query
    |
    v
[RagService.RunRagAsync]
    |
    +---> [IRetrievalService.SearchAsync] ---> pgvector (cosine similarity)
    |         Returns top-K document chunks
    |
    +---> [IPromptResolver.BuildPromptAsync] ---> LayeredPromptResolver
    |         DB templates > Config > Code fallback
    |
    +---> [ILlmService.GetCompletionAsync] ---> OpenAI / Gemini / Claude
    |         Baseline answer + RAG answer
    |
    +---> [Save RagComparisonHistory to SQL Server]
    |
    +---> [IEvaluationService.EvaluateAsync] ---> LLM-as-judge (gpt-4o-mini)
              Scores: Relevance [0-1], Faithfulness [0-1]
              Persists EvaluationResultEntity with FK to history
```

### Document Ingestion

```
File Upload (PDF/DOCX/TXT)
    |
    +---> Path 1: IDocumentService ---> SQL Server (Document + DocumentChunk rows)
    |
    +---> Path 2: IngestionQueue ---> IngestionWorker (background)
              |
              +---> IDocumentParser (PDF/DOCX/TXT parsers)
              +---> IDocumentChunker (sliding window, 500 tokens)
              +---> IDocumentEmbedder (OpenAI text-embedding-3-small)
              +---> IDocumentVectorStore (pgvector INSERT)
```

---

## Database Schema

### SQL Server (ArNirDbContext)

| Table | Purpose |
|-------|---------|
| Documents | Uploaded documents (name, type, size, raw content) |
| DocumentChunks | Text chunks per document |
| RagComparisonHistories | RAG query history (query, answers, latency, tokens, SLA) |
| Feedbacks | User quality ratings (1-5 stars per history) |
| EvaluationResults | LLM-as-judge scores (relevance, faithfulness, reasoning) |
| PromptTemplates | Versioned prompt templates (style, version, active, text) |
| PlatformSettings | Key-value config overrides (replaces appsettings) |
| AgentRunLogs | Autonomous agent execution logs |
| MetricEvents | Observability events (provider, latency, tokens, SLA) |
| ChatMemories | Episodic chat session messages |
| AIInsightExportHistories | Export tracking |

### PostgreSQL (VectorDbContext)

| Table | Purpose |
|-------|---------|
| Embeddings | Vector embeddings (float[] via pgvector, FK to DocumentChunk) |

---

## Dependency Rules

```
  ArNir.Core          (no dependencies — leaf)
       |
  ArNir.Platform      (no dependencies — leaf)
       |
  ArNir.Data          --> Core
       |
  ArNir.Observability --> Core
       |
  ArNir.PromptEngine  --> Core, Platform
       |
  ArNir.Memory        --> Core, Data
       |
  ArNir.RAG           --> Core, Data, Platform
       |
  ArNir.RAG.Pgvector  --> Core, Data, RAG
       |
  ArNir.Tools         --> Core, Data
       |
  ArNir.Agents        --> Core, Tools
       |
  ArNir.Services      --> Core, Data, Platform, PromptEngine, Observability, Memory
       |               NEVER references ArNir.RAG (interface name conflicts!)
       |
  ArNir.Admin         --> All modules (bridge layer)
  ArNir.API           --> All modules except Admin
```

---

## Key Design Patterns

### LayeredPromptResolver (3-Layer Chain)
1. **DB** — `PromptTemplates` table (versioned, active flag)
2. **Config** — `PlatformSettings` table
3. **Code** — Hard-coded `CodePromptResolver` fallback

### IDbContextFactory Pattern
All services use `IDbContextFactory<ArNirDbContext>` for short-lived contexts (prevents connection pool exhaustion in async/scoped scenarios).

### Optional Dependency Injection
`IEvaluationService?` is injected as optional into `RagService` — if not registered, auto-evaluation is silently skipped. This prevents circular dependencies and allows the evaluation feature to be toggled.

### Dual-Path Ingestion
Documents are saved to SQL Server (immediate, synchronous) AND enqueued for background vector embedding (IngestionQueue → IngestionWorker). The API returns 202 Accepted immediately.

---

## Authentication

- **Admin Panel**: Cookie-based authentication (8h sliding session)
  - Login: `/Account/Login`
  - All controllers decorated with `[Authorize]`
- **API**: No authentication (Swagger-accessible for development)

---

## Configuration

### Connection Strings
| Key | Database |
|-----|----------|
| `DefaultConnection` (Admin) | SQL Server — entities, history, settings |
| `SqlServer` (API) | SQL Server — same schema, different key name |
| `Postgres` (both) | PostgreSQL — vector embeddings via pgvector |

### OpenAI
| Key | Usage |
|-----|-------|
| `OpenAI:ApiKey` | Embeddings (text-embedding-3-small), LLM completions, evaluation judge |

### Provider Keys (PlatformSettings table)
| Setting Key | Provider |
|-------------|----------|
| `openai_api_key` | OpenAI |
| `gemini_api_key` | Google Gemini |
| `claude_api_key` | Anthropic Claude |

---

## Sprint Timeline

| Sprint | Milestone | Key Deliverable |
|--------|-----------|-----------------|
| Phase 0-10 | Foundation | 14-project architecture, 11 DB tables, admin panels |
| Sprint 1 | Production Bridge | pgvector pipeline, cookie auth, file validation |
| Sprint 2 | Ops Visibility | Health dashboard, background ingestion, vector store health |
| Sprint 3 | Feature Parity | Embeddings mgmt, memory panel, agent trigger, job monitor |
| Sprint 4 | Polish | Feedback loop, template import/export, notification center |
| Sprint 5 | API Parity | API production RAG pipeline, security hardening |
| Sprint 6 | Quality | LLM-as-judge evaluation layer (auto-scoring every RAG query) |
| Sprint 7 | Demo Mode | README, Postman collection, Dockerfiles, architecture docs |
| Sprint 8 | Versioning | Prompt versioning — edit-creates-version, history, rollback, compare |
| Phase A | Frontend Shared | @arnir/shared component library — npm workspaces monorepo |
| Improvement Phase 1 | Frontend Foundation | Error boundary, dark mode, skeletons, responsive layout, shared tests, pre-build |
| Improvement Phase 2 | Frontend Accessibility | Accessibility hooks, ARIA roles/labels, Storybook source/stories, frontend verification follow-up |

---

## Frontend Architecture (Demo Frontends)

### Monorepo Structure (npm workspaces)
```
/frontend
  /shared                 @arnir/shared — 23 files (API, hooks, components, UI, theme)
  /healthcare-demo        Port 3001 — teal/green — Healthcare Knowledge Assistant
  /ecommerce-demo         Port 3002 — orange/amber — Ecommerce Product Advisor
  /finance-demo           Port 3003 — navy/gold — Financial Document Analyzer
```

### Shared Library Components
- **API layer**: Env-configurable axios client (`VITE_API_BASE_URL`), modules for RAG, chat, feedback, documents (multipart), evaluation
- **Hooks**: `useChat` (configurable provider/model/promptStyle), `useFileUpload` (drag-drop, validation)
- **Components**: ChatWindow, FileUpload, SourceViewer, FeedbackModal (5-star + API call), MessageBubble (markdown), TypingIndicator
- **UI**: Button (4 variants), Card, Input — all using semantic `primary-*`/`accent-*` Tailwind colors
- **Theme**: React context provider + runtime chart color definitions per demo

### Tech Stack
Vite 7.1.7 | React 19.1.1 | TailwindCSS 3.4.13 | Framer Motion | Lucide React | Axios | Vitest

### Frontend Improvement Status
- **Phase 1 — Foundation**: Complete and verified
- **Phase 2 — Accessibility + Storybook**: Complete in source; shared + demo tests/builds verified
- **Storybook Runtime**: Blocked until declared Storybook CLI dependencies are installed
- **Phase 3 — Healthcare Domain Features**: Pending
- **Phase 4 — Ecommerce Domain Features**: Pending
- **Phase 5 — Finance Domain Features**: Pending
- **Phase 6 — Docker + Infrastructure**: Pending
- **Phase 7 — Streaming + Analytics**: Pending
- **Phase 8 — TypeScript Migration**: Pending
