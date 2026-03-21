# ArNir Enterprise AI Platform — Knowledge Base
**v2.0 | March 2026 | Branch: AI_Consultant_Update**
*For use as context with: Claude Projects · ChatGPT · Google Gemini · Perplexity*

### Frontend Improvement Phases

| Improvement Phase | Status | Details |
|---|---|---|
| Phase 1 | Complete and verified | ErrorBoundary, dark mode, loading skeletons, responsive layout, shared test suite (31), Vite pre-build |
| Phase 2 | Complete in source, verified for tests/builds | Accessibility hooks, ARIA roles/labels, Storybook source/stories, demo test alignment follow-up |
| Phase 3 | Complete and verified | Multi-document healthcare chat, medical term highlighting, jsPDF export, inline source document panel/viewer |
| Phase 4 | Complete and verified | Product comparison, price filters, cart + wishlist, image metadata rendering, facet-filtered recommendations |
| Phase 5 | Pending | Finance domain features |
| Phase 6 | Pending | Docker + infrastructure improvements |
| Phase 7 | Pending | Streaming + analytics |
| Phase 8 | Pending | TypeScript migration |

> Note: Storybook scripts are declared for Phase 2, but Storybook CLI dependencies are not currently installed in `node_modules` / lockfile, so Storybook runtime verification remains pending until install.

---

## 1. Project Overview

ArNir is a full-stack **.NET 9** Enterprise AI Platform that exposes:
- **RAG** (Retrieval-Augmented Generation) — ingest PDF/DOCX/TXT, embed with OpenAI, query with top-K chunk retrieval
- **Multi-provider LLM orchestration** — OpenAI GPT-4, Google Gemini, Anthropic Claude, switchable at runtime
- **Semantic memory** — chat session storage + pgvector similarity search
- **Prompt engineering** — 3-layer resolution (DB → Config → Code), 5 styles (zero-shot, few-shot, role, rag, hybrid)
- **Agent execution** — IPlannerAgent multi-step orchestration
- **Observability** — SLA metrics, latency tracking, AI insight generation
- **Evaluation layer** — LLM-as-judge RAG quality scoring (relevance + faithfulness)
- **Prompt versioning** — edit-creates-version, history timeline, rollback, side-by-side compare

The platform has two consumer apps:
- **ArNir.API** — REST API (ASP.NET Core Web API) — 12 controllers
- **ArNir.Admin** — Management UI (ASP.NET Core MVC + Bootstrap 5) — 19 controllers

**Build status:** 0 errors | **Backend tests:** 72/72 passing | **Frontend verification:** shared 31/31, healthcare 12/12, ecommerce 8/8, finance 10/10 | **Docker:** `docker compose --profile full up`

---

## 2. Solution Architecture

### 2.1 All Projects

| Project | Type | Role |
|---|---|---|
| ArNir.Core | Class Library | Entities, DTOs, `IEmbeddingProvider`. No project refs — base layer |
| ArNir.Platform | Class Library | Enums, constants, config POCOs (OpenAiSettings, RagSettings). No project refs |
| ArNir.Data | Class Library | EF Core DbContexts: `ArNirDbContext` (SQL Server) + `VectorDbContext` (PostgreSQL/pgvector). Migrations |
| ArNir.RAG | Class Library | Ingestion pipeline interfaces + null stubs + real pipeline. Background hosting: IngestionQueue, IngestionWorker |
| ArNir.RAG.Pgvector | Class Library | Real implementations: `PgvectorDocumentEmbedder` + `PgvectorDocumentVectorStore` |
| ArNir.Memory | Class Library | IEpisodicMemory, ISemanticMemory — chat session management |
| ArNir.PromptEngine | Class Library | IPromptResolver, IPromptVersionStore, LayeredPromptResolver |
| ArNir.Agents | Class Library | IPlannerAgent — multi-step agent orchestration |
| ArNir.Tools | Class Library | IAgentTool implementations |
| ArNir.Observability | Class Library | IMetricCollector, IAIInsightGenerator, IEvaluationService, DbMetricCollector |
| ArNir.Services | Class Library | All domain services (DocumentService, EmbeddingService, RagService, LlmEvaluationService, EvaluationHistoryService, etc.) |
| ArNir.Admin | ASP.NET MVC Web App | 19 controllers, Bootstrap 5 admin panel. Clean: controllers + views only |
| ArNir.API | ASP.NET Web API | 12 REST controllers |
| ArNir.Tests | xUnit Test Project | 72 unit tests across 8 sprints |

### 2.2 Dependency Graph (→ means "references")

```
ArNir.Core         (no project refs)
ArNir.Platform     (no project refs)
ArNir.Data         → Core
ArNir.RAG          → Platform
ArNir.RAG.Pgvector → Core, Data, RAG
ArNir.Memory       → Platform
ArNir.PromptEngine → Platform
ArNir.Agents       → Platform, Memory, PromptEngine
ArNir.Tools        → Platform, Agents
ArNir.Observability→ Platform
ArNir.Services     → Core, Data, Platform, PromptEngine, Observability, Memory
ArNir.Admin        → Core, Data, Services, Platform, PromptEngine, RAG, RAG.Pgvector, Memory, Agents, Tools, Observability
ArNir.API          → Core, Data, Services, Observability, Memory, PromptEngine, Agents, Tools, RAG
```

### 2.3 CRITICAL Architecture Rules — NEVER BREAK

1. **ArNir.Services MUST NEVER reference ArNir.RAG** — `IEmbeddingService` / `IRetrievalService` name conflicts
2. **ArNir.RAG MUST NEVER reference ArNir.Services** — same reason
3. `IEmbeddingProvider` lives in **ArNir.Core.Interfaces** — shared by both Services and RAG.Pgvector
4. `OpenAiEmbeddingProvider` implements BOTH `ArNir.Services.Provider.IEmbeddingProvider` AND `ArNir.Core.Interfaces.IEmbeddingProvider`
5. `LayeredPromptResolver` is the live `IPromptResolver` in both Admin and API
6. `PlatformSettings` table is single source of truth for all runtime configuration
7. **ArNir.Admin is clean** — controllers + views only; all business logic in respective module project
8. **DO NOT RENAME:** `IRagService`, `IRetrievalService`, `IEmbeddingService`, `IDocumentService`, `IContextMemoryService`, `ILlmService`, `IAnalyticsService`, `IAIInsightService`

---

## 3. Database Schema

### 3.1 SQL Server — ArNirDbContext
Connection string: `Server=localhost;Database=ArNir;Trusted_Connection=True;TrustServerCertificate=True`

| DbSet | Entity | Key Fields |
|---|---|---|
| Documents | Document | Id (int PK), Name, Type, UploadedBy, UploadedAt |
| DocumentChunks | DocumentChunk | Id (int PK), DocumentId (FK→Documents), ChunkOrder (int), Text |
| ChatSessions | ChatSession | Id (int PK), Model, CreatedAt |
| ChatMessages | ChatMessage | Id (int PK), SessionId (FK→ChatSessions), Role, Content |
| RagComparisonHistories | RagComparisonHistory | Id (int PK), UserQuery, BaselineAnswer, RagAnswer, RetrievedChunksJson, RetrievalLatencyMs, LlmLatencyMs, TotalLatencyMs, IsWithinSla, PromptStyle, Provider, Model, CreatedAt, QueryTokens, ContextTokens, TotalTokens |
| Feedbacks | Feedback | Id (int PK), HistoryId (FK→RagComparisonHistories), Rating (int 1–5), Comments?, CreatedAt |
| ExportHistories | ExportHistory | Id (int PK), UserName?, Provider?, Format?, DateRange?, CreatedAt |
| ChatMemories | ChatMemory | Id (int PK), SessionId (string 64), UserMessage, AssistantMessage?, CreatedAt, EmbeddingRefId (Guid?) |
| PromptTemplates | PromptTemplateEntity | Id (Guid PK), Style (50), Name (100), TemplateText, Version (int), IsActive (bool), Source (20), CreatedAt |
| PlatformSettings | PlatformSetting | Id (int PK), Module (50), Key (100), Value, Description (300)?, UpdatedAt |
| AgentRunLogs | AgentRunLog | Id (Guid PK), SessionId (64), OriginalQuery, PlanJson, Status (20), CreatedAt, CompletedAt? |
| MetricEvents | MetricEventEntity | Id (int PK, identity), EventType (50), Provider (50), Model (100), LatencyMs (long), IsWithinSla (bool), TokensUsed (int), OccurredAt, TagsJson? |
| EvaluationResults | EvaluationResultEntity | Id (int PK, identity), Question, Answer, Context, RelevanceScore (double), FaithfulnessScore (double), Reasoning, EvaluatedAt, RelatedHistoryId (int? FK→RagComparisonHistories) |

### 3.2 PostgreSQL — VectorDbContext
Connection string: `Host=localhost;Database=AirNir_PG;Username=postgres;Password=yourpassword`
Requires: `CREATE EXTENSION vector;` in the database

| Table | Entity | Key Fields |
|---|---|---|
| Embeddings | Embedding | EmbeddingId (Guid PK), ChunkId (int FK→DocumentChunks.Id), Model (string), Vector (vector(1536)), CreatedAt |
| ChatEmbeddings | ChatEmbedding | EmbeddingId (Guid PK), ChatMemoryId (int), Model (default: text-embedding-3-small), Vector (vector(1536)), CreatedAt |

> **IMPORTANT:** `Embedding.ChunkId` is an `int` FK pointing to `DocumentChunks.Id` in SQL Server.
> The pipeline encodes chunkId as `"sql:{docId}:{chunkIndex}"` so `PgvectorDocumentVectorStore` can resolve this FK during vector storage.

### 3.3 EF Migrations
```bash
cd ArNir.Data
dotnet ef migrations add <MigrationName> --context ArNirDbContext
dotnet ef database update --context ArNirDbContext
```
`IDesignTimeDbContextFactory` reads `../ArNir.Admin/appsettings.json` → `ConnectionStrings.DefaultConnection` at design time.

---

## 4. RAG Ingestion Pipeline

### 4.1 Four-Step Pipeline (IngestionPipeline in ArNir.RAG)

1. **Parse** — `IDocumentParser` selects correct parser (PDF → PdfDocumentParser, DOCX → DocxDocumentParser, TXT → PlainTextDocumentParser) → produces `RagDocument`
2. **Chunk** — `IDocumentChunker` (SlidingWindowChunker) splits text into overlapping `RagChunk` items
3. **Embed** — `IDocumentEmbedder.GenerateBatchAsync()` → float[] vectors. **Production:** PgvectorDocumentEmbedder calls OpenAI API. **Dev stub:** NullDocumentEmbedder returns empty arrays
4. **Store** — `IDocumentVectorStore.StoreBatchAsync()` persists (chunkId, vector) pairs. **Production:** PgvectorDocumentVectorStore resolves SQL FK then writes Embedding rows to PostgreSQL. **Dev stub:** NullDocumentVectorStore is no-op

### 4.2 RAG Interfaces (ArNir.RAG/Interfaces)

| Interface | Key Methods | Implementation |
|---|---|---|
| IIngestionPipeline | `Task<IngestionResult> IngestAsync(IngestionRequest)` | IngestionPipeline (Scoped) |
| IDocumentParser | `Task<RagDocument> ParseAsync(Stream, string contentType)` | Pdf/Docx/PlainText parsers (Singleton) |
| IDocumentChunker | `IReadOnlyList<RagChunk> Chunk(RagDocument, int size, int overlap)` | SlidingWindowChunker (Singleton) |
| IDocumentEmbedder | `GenerateAsync(text, model)`, `GenerateBatchAsync(texts, model)` | PgvectorDocumentEmbedder (prod) / NullDocumentEmbedder (stub) |
| IDocumentVectorStore | `StoreAsync(chunkId, vector, metadata?)`, `StoreBatchAsync(items)`, `SearchAsync(queryVector, topK)`, `DeleteByDocumentAsync(documentId)` | PgvectorDocumentVectorStore (prod) / NullDocumentVectorStore (stub) |

### 4.3 Key Models (ArNir.RAG/Models)

| Model | Properties |
|---|---|
| IngestionRequest | FileStream, FileName, ContentType, UploadedBy, EmbeddingModel, `LegacySqlDocumentId (int?)` — threads SQL doc ID for FK resolution |
| IngestionResult | Success (bool), ChunksCreated (int), EmbeddingsCreated (int), ErrorMessage? |
| RagDocument | Id (Guid), FileName, ContentType, RawText, Chunks |
| RagChunk | Id (Guid), DocumentId (Guid), ChunkIndex (int), Text, TokenCount |
| RetrievalResult | ChunkId (string), Score (float), Text?, Metadata |

### 4.4 Background Ingestion Queue (ArNir.RAG/Hosting)

- **IngestionQueue** — `Channel<IngestionJobRequest>` (capacity 100). `QueueDepth` property. `RecentResults: ConcurrentQueue<IngestionJobResult>` (last 100). **DI: Singleton**
- **IngestionWorker** — `BackgroundService`. Dequeues, creates DI scope, resolves `IIngestionPipeline`, calls `IngestAsync()`. **DI: AddHostedService**
- **Registration:** `builder.Services.AddArNirRAGBackgroundIngestion()`

### 4.5 DI Registration Order (Program.cs)

```csharp
builder.Services.AddArNirRAG();                      // Registers Null stubs (Singleton)
builder.Services.AddArNirRagPgvector();              // Overrides with real Scoped impls (last wins)
builder.Services.AddArNirRAGBackgroundIngestion();   // Queue + Worker
```

### 4.6 Dual Upload Path (DocumentController)

- **Path 1 (Legacy SQL):** `IDocumentService.UploadDocumentAsync(dto)` → saves Documents + DocumentChunks in SQL Server → returns `docResult.Id (int)`
- **Path 2 (RAG Pipeline):** Creates `IngestionRequest { LegacySqlDocumentId = docResult.Id }` → enqueues to `IngestionQueue` → `IngestionWorker` processes async: Parse → Chunk → Embed → Store to pgvector

---

## 5. Service Interfaces (ArNir.Services)

All services registered **Scoped** unless noted.

| Interface | Key Methods |
|---|---|
| `IDocumentService` | `GetAllDocumentsAsync()`, `GetDocumentByIdAsync(id)`, `UploadDocumentAsync(dto)`, `UpdateDocumentAsync(id,dto)`, `DeleteDocumentAsync(id)`, `RebuildDocumentEmbeddingsAsync(docId, model)` |
| `IEmbeddingService` | `GenerateForDocumentAsync(request)`, `GenerateForQueryAsync(text, model)`, `DeleteEmbeddingsForDocumentAsync(docId)`, `RebuildEmbeddingsForDocumentAsync(docId, model)` |
| `IRetrievalService` | `SearchAsync(query, topK, useHybrid) → List<ChunkResultDto>` |
| `IRagService` | `RunRagAsync(query, topK, useHybrid, promptStyle, saveAsNew, provider, model)`, analytics methods (avg latency, SLA compliance, prompt style usage, trends, provider analytics), `GetRelatedInsightsAsync()` |
| `IRagHistoryService` | `GetHistoryAsync(slaStatus, startDate, endDate, queryText, promptStyle, provider, model)`, `GetHistoryDetailsAsync(id)` |
| `IFeedbackService` | `AddFeedbackAsync(dto)`, `GetAllAsync()`, `GetAverageRatingAsync()` |
| `ILlmService` | `GetCompletionAsync(prompt, model)` |
| `IAnalyticsService` | `GetKpisAsync(provider, start, end)`, `GetChartsAsync(provider, start, end)` |
| `IPlatformSettingsService` | `GetAsync(module, key)`, `GetAsync<T>(module, key)`, `SetAsync(module, key, value, description)`, `GetModuleSettingsAsync(module)` — DB-backed, memory-cached |
| `IIntelligenceService` | `GetUnifiedDashboardAsync()`, `GetDashboardExportAsync()`, `GetRelatedInsightsAsync(prompt, topK)` |
| `IChatEmbeddingService` | `GenerateEmbeddingForMessageAsync(chatMemoryId, text, model)`, `FindSimilarAsync(queryVector, limit)` |
| `INotificationService` | `GetActiveAlertsAsync(provider?) → List<AlertDto>` |
| `IExportService` | `ExportToExcel(dto)`, `ExportToCsv(dto)`, `ExportToPdf(dto)` → `(byte[], contentType, fileName)` |
| `IEmbeddingProvider` *(ArNir.Core)* | `GenerateEmbeddingAsync(text, model) → float[]` — implemented by `OpenAiEmbeddingProvider` |
| `IEvaluationService` *(ArNir.Observability)* | `EvaluateAsync(question, answer, context) → EvaluationResult` — LLM-as-judge scoring (relevance + faithfulness) |
| `IEvaluationHistoryService` | `GetRecentAsync(page, pageSize, minRelevance?, minFaithfulness?) → List<EvaluationResultDto>`, `GetByIdAsync(id)`, `GetStatsAsync() → EvaluationStatsDto`, `GetTotalCountAsync()`, `PersistAsync(entity)` |

---

## 6. Admin Panel (ArNir.Admin)

### 6.1 Authentication
- Cookie authentication, **8-hour sliding session**, HttpOnly
- Login: POST `/Account/Login` — validates against `Auth:AdminUsername` / `Auth:AdminPassword` in appsettings.json
- Default credentials: `admin` / `Admin@123`
- All 19 controllers decorated with `[Authorize]`
- Logout: POST `/Account/Logout` with antiforgery token

### 6.2 Controllers & Features

| Controller | Route | Features |
|---|---|---|
| AccountController | /Account/* | GET/POST Login, POST Logout, GET AccessDenied |
| HomeController | / | Platform health dashboard: doc/chunk/embedding counts, Postgres status, embedder type, 24h SLA%, avg latency, recent agent runs |
| DocumentController | /Document/* | CRUD + dual-path upload (SQL + RAG queue). Server-side validation: MIME type + 5MB max |
| EmbeddingController | /Embedding/* | Stats cards (total, by-model, age-range). Rebuild All (enqueues all docs). Delete by Model |
| VectorStoreController | /VectorStore/* | Embeddings by model, orphaned chunks detection, per-doc rebuild |
| MemoryController | /Memory/* | Session list, transcript view, DeleteSession, PurgeOld(daysOld) |
| AgentRunHistoryController | /AgentRunHistory/* | Run history list. TriggerRun: manual agent query → calls IPlannerAgent, logs AgentRunLog |
| JobMonitorController | /JobMonitor/* | Live queue depth + recent jobs. Status JSON endpoint (3s AJAX polling) |
| PromptTemplateController | /PromptTemplate/* | Full CRUD with **version-aware editing** (edit creates new version). Stats: Chart.js bar chart. ExportJson/ImportJson. **History(style)**: version timeline. **Rollback(id)**: restore old version as new active. **Compare(id1,id2)**: side-by-side diff |
| RagHistoryController | /RagHistory/* | History table with filters. Details. SubmitFeedback POST: AJAX 1-5 star upsert |
| RagComparisonController | /RagComparison/* | Side-by-side RAG vs baseline comparison |
| PlatformSettingsController | /PlatformSettings/* | CRUD for PlatformSettings table (runtime config) |
| ProviderConfigController | /ProviderConfig/* | API key management (OpenAI/Gemini/Claude), masked input, stored in PlatformSettings |
| ObservabilityDashboardController | /ObservabilityDashboard/* | SLA metrics, latency trends, token usage |
| AnalyticsController | /Analytics/* | KPI cards, chart data |
| RetrievalController | /Retrieval/* | Semantic search test UI |
| ReportsController | /Reports/* | Export to Excel/CSV/PDF |
| EvaluationController | /Evaluation/* | LLM-as-judge dashboard: 4 KPI cards (total, avg relevance, avg faithfulness, combined), Chart.js trend chart, DataTable with color-coded scores. Details view per evaluation |
| NotificationController | /Notification/* | `GetUnread` JSON: MetricEvents where IsWithinSla=false in last 1h → `{count, alerts[5]}`. Navbar bell icon (30s polling) |

---

## 7. REST API (ArNir.API)

Base URL: `https://localhost:{port}/api/`

| Controller | Route | Endpoints |
|---|---|---|
| DocumentIngestController | /api/documents | POST /ingest — upload + trigger RAG pipeline |
| RagController | /api/rag | POST /run — `{query, topK, useHybrid, promptStyle, provider, model}` |
| ChatController | /api/chat | POST /query, GET /context/{sessionId} |
| AnalyticsController | /api/analytics | GET /average-latencies, /sla-compliance, /prompt-style-usage, /trends, /provider |
| FeedbackController | /api/feedback | POST (submit), GET (list all), GET /average |
| AgentController | /api/agent | POST /run — `{query, sessionId}` |
| IntelligenceController | /api/intelligence | GET /dashboard, GET /export, GET /insights, POST /chat, POST /related, POST /action |
| InsightsController | /api/insights | POST /analyze, /anomalies, /predict, /report |
| RetrievalController | /api/retrieval | POST /test — similarity search test |
| EvaluationController | /api/evaluation | GET /history — paginated with filters. POST /evaluate — on-demand LLM scoring + persist. GET /stats — aggregate statistics |
| IntelligenceChatController | /api/intelligence/chat | POST — unified chat endpoint |

---

## 8. Configuration

### 8.1 appsettings.json Structure (ArNir.Admin)

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=ArNir;Trusted_Connection=True;TrustServerCertificate=True",
    "Postgres": "Host=localhost;Database=AirNir_PG;Username=postgres;Password=yourpassword"
  },
  "OpenAI": {
    "ApiKey": "<your-openai-api-key>",
    "EmbeddingModel": "text-embedding-ada-002"
  },
  "Auth": {
    "AdminUsername": "admin",
    "AdminPassword": "Admin@123"
  },
  "FileUploadSettings": {
    "AllowedTypes": ["application/pdf", "text/plain", "application/vnd.openxmlformats-officedocument.wordprocessingml.document"],
    "MaxFileSize": 5242880
  }
}
```

### 8.2 Runtime Config — PlatformSettings Table

| Module | Key | Purpose |
|---|---|---|
| AI | DefaultProvider | Active LLM: `OpenAI` / `Gemini` / `Claude` |
| AI | DefaultModel | Model ID: `gpt-4o`, `gemini-pro`, `claude-3-haiku` |
| AI | OpenAiApiKey | OpenAI API key (stored in DB) |
| AI | GeminiApiKey | Google Gemini API key |
| AI | ClaudeApiKey | Anthropic Claude API key |
| RAG | ChunkSize | Sliding window size in tokens (default 400) |
| RAG | ChunkOverlap | Overlap tokens (default 50) |
| RAG | TopK | Chunks retrieved per query (default 5) |
| RAG | EmbeddingModel | Embedding model (default text-embedding-ada-002) |
| Observability | SlaThresholdMs | Max acceptable latency ms for SLA (default 3000) |
| Prompts | DefaultStyle | Default prompt style |

---

## 9. Prompt Engine

### 9.1 Three-Layer Resolution (LayeredPromptResolver)
1. **Layer 1 — Database:** Query `PromptTemplates` for Style + highest Version + `IsActive=true`
2. **Layer 2 — Config:** Check `appsettings.json` `Prompts:{style}`
3. **Layer 3 — Code:** Hardcoded fallback in `CodePromptResolver`

### 9.2 PromptStyle Enum (ArNir.Platform.Enums)

| Value | Name | Description |
|---|---|---|
| 0 | ZeroShot | No examples; model responds from instruction alone |
| 1 | FewShot | Small input-output examples guide the model |
| 2 | Role | Model assigned a persona/role |
| 3 | Rag | Document chunks injected as context |
| 4 | Hybrid | Combines two or more styles |

### 9.3 Provider Enum (ArNir.Platform.Enums)
`OpenAI`, `Gemini`, `Claude`

---

## 10. Sprint & Phase History

### Phases Completed

| Phase | What Was Built |
|---|---|
| Phase 0 | Solution structure, all .csproj files, project references |
| Phase 2 | ArNir.Platform — enums, constants, config POCOs, helpers |
| Phase 3 | ArNir.RAG — interfaces, null stubs, parsers, chunker, pipeline |
| Phase 4 | ArNir.Memory — IEpisodicMemory, ISemanticMemory |
| Phase 5 | ArNir.PromptEngine — IPromptResolver, IPromptVersionStore, CodePromptResolver |
| Phase 6 | ArNir.Agents — IPlannerAgent |
| Phase 7 | ArNir.Tools — IAgentTool implementations |
| Phase 8 | ArNir.Observability — IMetricCollector, IAIInsightGenerator, DbMetricCollector |
| Phase 9 | Wire modules into Services; 2 demo API endpoints (RAG + Agent) |
| Phase 10 | DB-driven settings: PromptTemplates, PlatformSettings, AgentRunLogs, MetricEvents. LayeredPromptResolver. Admin panels. 5 default templates seeded. |

### Sprints Completed

| Sprint | Commit | Key Deliverables |
|---|---|---|
| Sprint 1 | ebd56c4 | pgvector bridge (PgvectorDocumentEmbedder + PgvectorDocumentVectorStore). Cookie auth. [Authorize] on all controllers. AccountController. Server-side file validation. 12 tests. |
| Sprint 2 + Refactor | d51f900 | Health Dashboard. Vector Store Health. Background IngestionQueue + IngestionWorker. Provider Config UI. **Refactor:** ArNir.RAG.Pgvector (new project), ArNir.RAG/Hosting (new folder), ArNir.Admin cleaned. 17 tests. |
| Sprint 3 | 402c084 | Enhanced Embeddings Mgmt. Memory Panel. Agent Manual Trigger. Job Monitor (3s AJAX). Prompt A/B Stats (Chart.js). 19 tests. |
| Sprint 4 | a488f17 | 1-5 star Feedback on RAG history (AJAX upsert). Template Import/Export (JSON). Notification Center (bell icon, SLA breach dropdown, 30s polling). 15 tests. |
| Sprint 5 | — | **API Production Parity.** DocumentIngestController (upload + RAG pipeline trigger). Production DI wiring for ArNir.API (pgvector, RAG, background ingestion, observability). Swagger UI. 5 tests. |
| Sprint 6 | — | **Evaluation Layer (LLM-as-Judge).** LlmEvaluationService (calls OpenAI gpt-4o-mini, scores relevance + faithfulness). EvaluationHistoryService (paginated history, stats, persistence). Auto-evaluation hook in RagService (optional, try-catch, never breaks pipeline). Admin EvaluationController (KPI dashboard, Chart.js trends, DataTable). API EvaluationController (3 endpoints). EvaluationResultEntity + EF migration. 10 tests. |
| Sprint 7 | — | **Demo Mode & DevOps.** Root README.md (architecture diagram, quick start, 30+ API endpoints). Dockerfile.admin + Dockerfile.api (multi-stage .NET 9 builds). docker-compose.yml updated (full profile with healthchecks). Postman collection (all 12 API controllers). ArNir-Architecture.md. Demo seed data migration (sample RAG histories, evaluations, metrics, feedbacks). 0 new tests. |
| Sprint 8 | — | **Prompt Versioning.** Edit-creates-new-version (deactivates old, auto-increments Version). History(style) action + timeline view with compare selector. Rollback(id) (creates new version from old, deactivates all). Compare(id1,id2) side-by-side diff with JS line-by-line highlighting. Updated Index + CreateEdit views for version awareness. 8 tests. |

---

## 11. Test Coverage

**72 tests total — all passing.** xUnit 2.9.2 + Moq 4.20.72 + EF InMemory 9.0.9

| Sprint | Tests | Classes Covered |
|---|---|---|
| Sprint 1 (12) | PgvectorDocumentEmbedderTests (3), IngestionQueueTests (3), AccountControllerTests (3), DocumentControllerTests (3) |
| Sprint 2 (5) | HomeControllerTests (1), VectorStoreControllerTests (2), ProviderConfigControllerTests (2) |
| Sprint 3 (19) | EmbeddingControllerTests (5), MemoryControllerTests (5), JobMonitorControllerTests (4), AgentRunHistoryControllerTests (5) |
| Sprint 4 (13) | RagHistoryControllerTests (4), PromptTemplateControllerTests (5), NotificationControllerTests (4) |
| Sprint 5 (5) | DocumentIngestControllerApiTests (5) |
| Sprint 6 (10) | LlmEvaluationServiceTests (6), EvaluationControllerAdminTests (4) |
| Sprint 8 (8) | PromptVersioningTests (8): edit-creates-version, history, rollback, compare, edge cases |

---

## 12. Build Commands

```bash
# Build individual projects
dotnet build ArNir.Platform/ArNir.Platform.csproj
dotnet build ArNir.RAG/ArNir.RAG.csproj
dotnet build ArNir.RAG.Pgvector/ArNir.RAG.Pgvector.csproj
dotnet build ArNir.Services/ArNir.Services.csproj
dotnet build ArNir.Admin/ArNir.Admin.csproj   # builds entire dep tree
dotnet build ArNir.API/ArNir.API.csproj

# Run tests
dotnet test ArNir.Tests/ArNir.Tests.csproj
dotnet test ArNir.Tests/ArNir.Tests.csproj --verbosity normal

# EF Core (run from ArNir.Data/ directory)
cd ArNir.Data
dotnet ef migrations add <MigrationName> --context ArNirDbContext
dotnet ef database update --context ArNirDbContext
```

---

## 13. Quick Reference — Where Does X Live?

| What You Need | Project | Class / File |
|---|---|---|
| IEmbeddingProvider interface | ArNir.Core | `ArNir.Core.Interfaces.IEmbeddingProvider` |
| OpenAI embedding caller | ArNir.Services | `OpenAiEmbeddingProvider` |
| Real pgvector embedder | ArNir.RAG.Pgvector | `PgvectorDocumentEmbedder` |
| Real pgvector vector store | ArNir.RAG.Pgvector | `PgvectorDocumentVectorStore` |
| Register pgvector DI | ArNir.RAG.Pgvector | `AddArNirRagPgvector()` |
| Register null stubs DI | ArNir.RAG | `AddArNirRAG()` |
| Background ingestion queue | ArNir.RAG | `ArNir.RAG.Hosting.IngestionQueue` |
| Background ingestion worker | ArNir.RAG | `ArNir.RAG.Hosting.IngestionWorker` |
| Register queue+worker | ArNir.RAG | `AddArNirRAGBackgroundIngestion()` |
| SQL Server DbContext | ArNir.Data | `ArNir.Data.ArNirDbContext` |
| PostgreSQL DbContext | ArNir.Data | `ArNir.Data.VectorDbContext` |
| Layered prompt resolver | ArNir.PromptEngine | `LayeredPromptResolver` |
| Runtime config service | ArNir.Services | `PlatformSettingsService` |
| SLA metrics collector | ArNir.Observability | `DbMetricCollector` |
| Admin login | ArNir.Admin | `AccountController` + `Views/Account/Login.cshtml` |
| Navbar notification bell | ArNir.Admin | `NotificationController` + `Views/Shared/_Layout.cshtml` |
| LLM-as-judge evaluation | ArNir.Services | `LlmEvaluationService` (implements `IEvaluationService` from ArNir.Observability) |
| Evaluation history/stats | ArNir.Services | `EvaluationHistoryService` |
| Auto-eval hook | ArNir.Services | `RagService` (optional `IEvaluationService?` constructor param) |
| Evaluation dashboard | ArNir.Admin | `EvaluationController` + `Views/Evaluation/Index.cshtml` |
| Prompt version history | ArNir.Admin | `PromptTemplateController.History()` + `Views/PromptTemplate/History.cshtml` |
| Prompt version compare | ArNir.Admin | `PromptTemplateController.Compare()` + `Views/PromptTemplate/Compare.cshtml` |
| Prompt rollback | ArNir.Admin | `PromptTemplateController.Rollback()` — POST creates new version from old |
| Docker deployment | Root | `Dockerfile.admin`, `Dockerfile.api`, `docker-compose.yml` (profile: full) |
| Postman collection | Docs | `ArNir-Postman-Collection.json` + `ArNir-Postman-Environment.json` |
| Architecture docs | Docs | `ArNir-Architecture.md` |
| Unit tests | ArNir.Tests | `Sprint1/` `Sprint2/` `Sprint3/` `Sprint4/` `Sprint5/` `Sprint6/` `Sprint8/` |

---

## 14. Common Gotchas

1. **Never cross-reference Services ↔ RAG** — interface name conflicts (`IEmbeddingService`, `IRetrievalService`)
2. **Null stubs are Singleton; real impls are Scoped** — always register real impls AFTER `AddArNirRAG()` so last registration wins
3. **Embedding.ChunkId is `int`** — FK to `DocumentChunks.Id` in SQL Server, NOT a UUID
4. **ChunkId encoding** — pipeline stores `"sql:{docId}:{chunkIndex}"` string; `PgvectorDocumentVectorStore` parses this to resolve the FK
5. **PostgreSQL setup** — requires `CREATE EXTENSION vector;` before EF can create vector columns
6. **[ValidateAntiForgeryToken]** on ALL POST actions; **[Authorize]** on all controllers except AccountController
7. **EF migrations** — always run from `ArNir.Data/` directory; `IDesignTimeDbContextFactory` reads `../ArNir.Admin/appsettings.json`
8. **OpenAI API key path** — config reads `OpenAI:ApiKey` at root level, NOT inside `FileUploadSettings`
9. **Postgres connection string** — Host=localhost;Database=**AirNir_PG** (note: AirNir not ArNir)
10. **Cookie auth order** — `app.UseAuthentication()` MUST come BEFORE `app.UseAuthorization()` in the pipeline

---

## 15. Docker Deployment

### 15.1 Quick Start
```bash
docker compose --profile full up -d
```

### 15.2 Services
| Service | Port | Profile |
|---|---|---|
| PostgreSQL + pgvector | 5432 | default |
| PgAdmin | 5050 | default |
| ArNir.API | 5000 | full |
| ArNir.Admin | 5001 | full |

### 15.3 Dockerfiles
- `Dockerfile.admin` — Multi-stage build (.NET 9 SDK → aspnet runtime), port 5001
- `Dockerfile.api` — Multi-stage build (.NET 9 SDK → aspnet runtime), port 5000
- Environment overrides: `ConnectionStrings__DefaultConnection`, `ConnectionStrings__Postgres`, `OpenAI__ApiKey`

---

## 16. Evaluation Layer (LLM-as-Judge)

### 16.1 How It Works
1. **RagService** completes a RAG query → auto-fires `IEvaluationService.EvaluateAsync()` (optional, try-catch)
2. **LlmEvaluationService** sends structured prompt to OpenAI gpt-4o-mini: "Score relevance 0-1, faithfulness 0-1, provide reasoning"
3. Parses JSON response `{relevance, faithfulness, reasoning}`, clamps scores to [0,1]
4. Persists `EvaluationResultEntity` with FK to `RagComparisonHistories`

### 16.2 Key Pattern
```csharp
// Optional dependency — never breaks RAG pipeline
public RagService(..., IEvaluationService? evaluationService = null)
```

---

## 17. Prompt Versioning

### 17.1 Edit-Creates-Version Pattern
- Editing a template does NOT modify in-place
- Creates a new row with `Version = max(version) + 1`, `IsActive = true`
- Deactivates the previous active version

### 17.2 Rollback Pattern
- POST `Rollback(id)` → finds the old version → creates NEW version with old text
- Name includes "(rollback from vN)" suffix
- Deactivates all other versions of that style

### 17.3 Compare
- GET `Compare(id1, id2)` → side-by-side metadata cards + pre-formatted text + JS line-by-line diff

---

## 18. Demo Frontends (React)

### 18.1 Architecture
ArNir includes 3 industry-specific React demo frontends showcasing the platform as a multi-industry AI solution. All demos share a common component library via **npm workspaces**.

```
/frontend
  /shared                 @arnir/shared — reusable components, hooks, API clients
  /healthcare-demo        Port 3001 — Healthcare Knowledge Assistant (teal/green)
  /ecommerce-demo         Port 3002 — Ecommerce Product Advisor (orange/amber)
  /finance-demo           Port 3003 — Financial Document Analyzer (navy/gold)
```

### 18.1.1 Improvement Status
- **Phase 1 — Foundation**: Complete and verified
- **Phase 2 — Accessibility + Storybook**: Complete in source and verified for shared/demo tests and builds
- **Verification snapshot**: shared 31/31, healthcare 13/13, `@arnir/shared` build OK, `@arnir/healthcare-demo` build OK, `dotnet build ArNir.sln` OK with warnings only
- **Storybook runtime**: pending dependency install
- **Phase 3**: complete and verified
- **Phase 4**: complete and verified
- **Phase 5+**: still pending implementation

### 18.2 Shared Library (@arnir/shared)
| Category | Files | Purpose |
|----------|-------|---------|
| API (7) | client.js, rag.js, chat.js, feedback.js, documents.js, evaluation.js | Env-configurable axios (`VITE_API_BASE_URL`) |
| Hooks (2) | useChat.js, useFileUpload.js | Configurable provider/model/promptStyle; drag-drop upload with validation |
| Components (8) | ChatWindow, FileUpload, SourceViewer, FeedbackModal, MessageBubble, TypingIndicator, Loader, ErrorBanner | Full chat UI + document upload + source display |
| UI (3) | Button (4 variants), Card, Input | Semantic `primary-*` / `accent-*` colors |
| Theme (2) | themes.js, themeContext.jsx | Runtime chart colors + React context |

### 18.3 Tech Stack
- Vite 7.1.7 + React 19.1.1 + TailwindCSS 3.4.13
- Framer Motion (animations), Lucide React (icons), React Markdown, Axios
- Vitest + React Testing Library (testing)
- npm workspaces monorepo (shared code, single `npm install`)

### 18.4 Theming
Each demo has its own `tailwind.config.js` mapping `primary-*` and `accent-*` to domain-specific palettes. Shared components use only semantic colors — resolved at build time.

---

*ArNir Knowledge Base v2.1 | Generated March 2026 | Build: 0 errors | Tests: 72/72 | 8 Sprints + Phase A completed*
