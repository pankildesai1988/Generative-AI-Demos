# ArNir Enterprise AI Platform — Claude Project Context

## Platform Overview
ArNir is a production-grade **.NET 9** Enterprise AI Platform with 14 projects, 19 Admin controllers (MVC + Bootstrap 5), 12 API controllers (REST + Swagger), and 72 passing unit tests across 8 sprints.

## Core Capabilities
- **RAG Pipeline**: 4-step ingestion (Parse → Chunk → Embed → Store) for PDF/DOCX/TXT. Dual storage: SQL Server (relational) + PostgreSQL/pgvector (vectors). Background IngestionQueue + IngestionWorker.
- **Multi-Provider LLM**: OpenAI GPT-4, Google Gemini, Anthropic Claude — switchable at runtime via PlatformSettings DB table.
- **Prompt Engineering**: 3-layer resolution (DB → Config → Code). 5 styles: zero-shot, few-shot, role, rag, hybrid. Full version management: edit-creates-version, history timeline, rollback, side-by-side compare with JS diff.
- **Evaluation Layer (LLM-as-Judge)**: Auto-scores every RAG response on Relevance (0-1) and Faithfulness (0-1) using gpt-4o-mini. Persisted in EvaluationResults table. Dashboard with KPI cards, Chart.js trends, color-coded DataTable.
- **Agent Execution**: IPlannerAgent multi-step orchestration with AgentRunLog persistence.
- **Semantic Memory**: Chat session storage + pgvector similarity search (IEpisodicMemory, ISemanticMemory).
- **Observability**: SLA metrics (DbMetricCollector), latency tracking, notification center (30s polling for SLA breaches), evaluation trends.

## Architecture (14 Projects)
| Project | Role |
|---|---|
| ArNir.Core | Entities, DTOs, IEmbeddingProvider. No refs — base layer |
| ArNir.Platform | Enums, constants, config POCOs. No refs |
| ArNir.Data | EF Core: ArNirDbContext (SQL Server, 12 tables) + VectorDbContext (PostgreSQL/pgvector, 2 tables) |
| ArNir.RAG | Ingestion pipeline interfaces + null stubs + parsers + chunker. IngestionQueue + IngestionWorker |
| ArNir.RAG.Pgvector | Production: PgvectorDocumentEmbedder + PgvectorDocumentVectorStore |
| ArNir.Memory | IEpisodicMemory, ISemanticMemory |
| ArNir.PromptEngine | IPromptResolver, IPromptVersionStore, LayeredPromptResolver, CodePromptResolver |
| ArNir.Agents | IPlannerAgent |
| ArNir.Tools | IAgentTool implementations |
| ArNir.Observability | IMetricCollector, IAIInsightGenerator, IEvaluationService, DbMetricCollector |
| ArNir.Services | All domain services: RagService, LlmEvaluationService, EvaluationHistoryService, DocumentService, etc. |
| ArNir.Admin | 19 MVC controllers, Bootstrap 5, cookie auth (8h sliding), AdminLTE layout |
| ArNir.API | 12 REST controllers, Swagger UI |
| ArNir.Tests | 72 xUnit tests (Moq + EF InMemory) across Sprint1-Sprint8 folders |

## CRITICAL Architecture Rules
1. **ArNir.Services MUST NEVER reference ArNir.RAG** (interface name conflicts: IEmbeddingService, IRetrievalService)
2. **ArNir.RAG MUST NEVER reference ArNir.Services** (same reason)
3. `IEmbeddingProvider` lives in ArNir.Core.Interfaces — shared by Services and RAG.Pgvector
4. `LayeredPromptResolver` is the live IPromptResolver in both Admin and API
5. ArNir.Admin is clean — controllers + views only; business logic in module projects
6. DO NOT RENAME: IRagService, IRetrievalService, IEmbeddingService, IDocumentService, IContextMemoryService, ILlmService

## Database Schema (SQL Server — 12 Tables)
Documents, DocumentChunks, ChatSessions, ChatMessages, RagComparisonHistories, Feedbacks, ExportHistories, ChatMemories, PromptTemplates, PlatformSettings, AgentRunLogs, MetricEvents, EvaluationResults

PostgreSQL (pgvector): Embeddings (vector(1536)), ChatEmbeddings (vector(1536))

## Key Design Patterns
- **Optional DI**: `IEvaluationService?` in RagService — never breaks pipeline if missing
- **Edit-creates-version**: Prompt edits create new version row (Version = max+1), deactivate old
- **Null stubs → real impls**: `AddArNirRAG()` registers Singleton stubs, `AddArNirRagPgvector()` overrides with Scoped (last wins)
- **IDbContextFactory**: Controllers use `IDbContextFactory<ArNirDbContext>` for short-lived DB contexts
- **Background processing**: Channel-based IngestionQueue → BackgroundService IngestionWorker

## Admin Panel Highlights (19 Controllers)
AccountController (cookie auth), HomeController (health dashboard), DocumentController (dual-path upload), EmbeddingController, VectorStoreController, MemoryController, AgentRunHistoryController, JobMonitorController (3s AJAX), PromptTemplateController (CRUD + versioning + History + Rollback + Compare + Stats + Import/Export), RagHistoryController (filters + feedback), RagComparisonController, PlatformSettingsController, ProviderConfigController (API keys), ObservabilityDashboardController, AnalyticsController, RetrievalController, ReportsController (Excel/CSV/PDF), EvaluationController (LLM-as-judge dashboard), NotificationController (bell icon)

## API Endpoints (12 Controllers)
DocumentIngestController, RagController, ChatController, AnalyticsController, FeedbackController, AgentController, IntelligenceController, IntelligenceChatController, InsightsController, RetrievalController, EvaluationController (GET /history, POST /evaluate, GET /stats)

## Docker Deployment
```bash
docker compose --profile full up -d  # PostgreSQL + PgAdmin + ArNir.Admin:5001 + ArNir.API:5000
```

## Sprint History (8 Sprints)
| Sprint | Key Deliverables |
|---|---|
| 1 | pgvector bridge, cookie auth, file validation. 12 tests |
| 2 | Health dashboard, background ingestion, provider config. 5 tests |
| 3 | Embeddings mgmt, memory panel, agent trigger, job monitor, A/B stats. 19 tests |
| 4 | Feedback (1-5 star), template import/export, notification center. 13 tests |
| 5 | API production parity, DocumentIngest, Swagger. 5 tests |
| 6 | Evaluation layer (LLM-as-judge), auto-eval hook, admin+API evaluation. 10 tests |
| 7 | README, Dockerfiles, docker-compose, Postman collection, architecture docs, demo seed data |
| 8 | Prompt versioning: edit-creates-version, history, rollback, compare. 8 tests |

## Test Coverage: 72 Tests
xUnit 2.9.2 + Moq 4.20.72 + EF InMemory 9.0.9. All passing. Pattern: IDbContextFactory mock + named InMemory DB + TempData setup.

## Demo Frontends (React — npm Workspaces Monorepo)
3 industry-specific React demo frontends sharing a common component library (`@arnir/shared`). All consume the same backend API — no backend changes needed.

### Shared Library (@arnir/shared) — 23 files
- **API modules** (7): Env-configurable axios client (`VITE_API_BASE_URL`), rag.js, chat.js, feedback.js, documents.js (multipart FormData), evaluation.js
- **Hooks** (2): `useChat` (configurable provider/model/promptStyle, returns messages/chunks/loading), `useFileUpload` (drag-drop, PDF/TXT/DOCX validation, 202 handling)
- **Components** (8): ChatWindow, FileUpload, SourceViewer (collapsible chunk panels), FeedbackModal (5-star + API call), MessageBubble (react-markdown), TypingIndicator (framer-motion), Loader, ErrorBanner
- **UI** (3): Button (4 variants: primary/secondary/accent/ghost), Card, Input — semantic `primary-*`/`accent-*` colors
- **Theme** (2): themes.js (runtime chart colors), themeContext.jsx (React context + ThemeProvider)

### Demos
| Demo | Port | Theme | Use Case |
|------|------|-------|----------|
| Healthcare | 3001 | Teal/green | Upload medical docs → Ask questions → Get answers with source citations |
| Ecommerce | 3002 | Orange/amber | Ask product questions → AI recommends items from catalog |
| Finance | 3003 | Navy/gold | Upload financial reports → AI summarizes insights + key metrics |

**Stack**: Vite 7.1.7 + React 19.1.1 + TailwindCSS 3.4.13 + Framer Motion + Lucide React + Axios

## Build
```bash
dotnet build ArNir.Admin/ArNir.Admin.csproj   # builds entire dependency tree
dotnet test ArNir.Tests/ArNir.Tests.csproj     # runs all 72 tests
npm install                                     # install all frontend workspaces
npm run dev --workspace=@arnir/healthcare-demo  # start healthcare demo on :3001
```
