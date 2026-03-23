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

**API ↔ Demo Frontend Compatibility**: CORS allows localhost:3001-3003. DocumentIngestController binds `uploadedBy` from multipart form data. FeedbackDto accepts both `comment` (singular, from demos) and `Comments` (plural, from Admin).

## Docker Deployment
```bash
docker compose --profile full up -d   # PostgreSQL + PgAdmin + ArNir.Admin:5001 + ArNir.API:5000
docker compose --profile demos up -d  # Healthcare:3001 + Ecommerce:3002 + Finance:3003
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

### Shared Library (@arnir/shared) — 30+ files
- **API modules** (7): Env-configurable axios client (`VITE_API_BASE_URL`), rag.js, chat.js, feedback.js, documents.js (multipart FormData), evaluation.js
- **Hooks** (2): `useChat` (configurable provider/model/promptStyle, returns messages/chunks/loading), `useFileUpload` (drag-drop, PDF/TXT/DOCX validation, 202 handling)
- **Components** (12): ChatWindow, FileUpload, SourceViewer, FeedbackModal, MessageBubble, TypingIndicator, Loader, ErrorBanner, **ErrorBoundary** (class component, fallback UI, retry), **Skeleton** (text/circle/card/chat-bubble variants), **ChatSkeleton**, **CardSkeleton**
- **UI** (3): Button (4 variants: primary/secondary/accent/ghost), Card, Input — semantic `primary-*`/`accent-*` colors, all with `dark:` variants
- **Theme** (2): themes.js (runtime chart colors + dark sub-objects), themeContext.jsx (React context + ThemeProvider + **dark mode**: mode/toggleMode, localStorage persistence, `dark` class on documentElement)
- **Tests** (8 files, 31 tests): vitest + @testing-library/react + jsdom. Covers: ChatWindow, MessageBubble, FileUpload, ErrorBanner, ErrorBoundary, SourceViewer, Skeleton, themeContext
- **Build**: Vite library mode (ESM output), `npm run build --workspace=@arnir/shared`

### Demos
| Demo | Port | Theme | Use Case |
|------|------|-------|----------|
| Healthcare | 3001 | Teal/green | Upload medical docs → Ask questions → Get answers with source citations |
| Ecommerce | 3002 | Orange/amber | Ask product questions → AI recommends items from catalog |
| Finance | 3003 | Navy/gold | Upload financial reports → AI summarizes insights + key metrics |

**Stack**: Vite 7.1.7 + React 19.1.1 + TailwindCSS 3.4.13 + Framer Motion + Lucide React + Axios

### Improvement Phase 1: Foundation (Completed)
- **Error Boundary**: Class component wrapping all 3 App.jsx Routes — catches render errors with fallback UI + retry
- **Dark Mode**: Tailwind `darkMode: "class"`, all shared components + UI primitives have `dark:` variants, Sun/Moon toggle in all 3 layouts
- **Loading Skeletons**: animate-pulse skeletons for chat bubbles, cards, generic shapes
- **Responsive Mobile**: Collapsible sidebar with hamburger menu, mobile overlay, `hidden lg:block` side panels
- **Shared Test Suite**: 31 tests across 8 files — components, hooks, theme context
- **Pre-build**: Vite library mode for @arnir/shared, removed `optimizeDeps.include` workaround

### Improvement Phase 2: Accessibility + Storybook (Completed, Build/Test Verified)
- **Accessibility**: `useFocusTrap` + `useKeyboardNav` hooks added. Shared UI updated with ARIA roles/labels for ChatWindow, MessageBubble, FileUpload, FeedbackModal, SourceViewer, ErrorBanner, Button, and Input.
- **Storybook Source**: `.storybook` config plus 11 stories added for shared UI/components.
- **Verification**: Shared tests 31/31, healthcare 12/12, ecommerce 8/8, finance 10/10. Shared library and all 3 demo frontends build successfully.
- **Current Gap**: Storybook scripts are declared, but Storybook CLI packages are not currently installed in `node_modules` / lockfile, so Storybook runtime verification is pending dependency install.

### Improvement Phase 3: Healthcare Domain Features (Completed, Build/Test Verified)
- **Multi-document scope**: `GET /api/documents`, `GET /api/documents/{id}`, `documentIds` support in shared `useChat`, RAG request DTOs, and retrieval flow.
- **Healthcare UX**: Medical chat now includes `DocumentSelector`, `HighlightedMessage`, `ExportButton`, `SourceDocPanel`, and an inline chunk-based source viewer (`PdfViewer`) for retrieved context inspection.
- **Verification**: Healthcare tests 13/13, shared tests 31/31, healthcare build successful, shared package build successful, `dotnet build ArNir.sln` successful with warnings only.

### Improvement Phase 4: Ecommerce Domain Features (Completed, Build/Test Verified)
- **Shopping experience**: Added local ecommerce state for cart, wishlist, and comparison, surfaced through the layout cart drawer and product recommendation cards.
- **Advisor controls**: Added budget range filtering, query budget enrichment, and recommendation facets for category and price band filtering.
- **Verification**: Ecommerce tests 9/9 and ecommerce build successful.

### Improvement Phase Tracker
- **Phase 1 — Foundation**: Complete and verified
- **Phase 2 — Accessibility + Storybook**: Complete in source, verified for tests/builds, Storybook runtime blocked by missing installed CLI deps
- **Phase 3 — Healthcare Domain Features**: Complete and verified
- **Phase 4 — Ecommerce Domain Features**: Complete and verified
- **Phase 5 — Finance Domain Features**: Complete and verified on this branch
- **Phase 6 ??? Docker + Infrastructure**: Complete in source, verified for tests/builds/E2E; Docker runtime validation blocked by local Docker Desktop I/O errors
- **Phase 7 — Streaming + Analytics**: Pending
- **Phase 8 — TypeScript Migration**: Pending

### Improvement Phase 5
- **Finance analytics UX**: `InsightsPanel` now combines extracted financial figures, responsive Recharts visualizations, weighted risk scoring, and export actions for the latest assistant answer.
- **Structured outputs**: markdown tables are promoted into `DataTable`, while `useComparisonHistory`, `FinanceProvider`, `/compare`, and `ComparisonDashboard` allow side-by-side review of recent finance analyses.
- **Verification**: finance tests 13/13 and `@arnir/finance-demo` build successful.

### Improvement Phase 6
- **Runtime deployability**: the shared API client now reads window.__RUNTIME_CONFIG__.API_URL, while each demo ships env-config.js plus an nginx entrypoint that injects ${API_BASE_URL} at container startup.
- **Infra coverage**: all demos now have frontend health checks, explicit nginx cache policy, root .dockerignore, a parent-repo rnir-frontend.yml workflow, and Playwright smoke tests for healthcare/ecommerce/finance.
- **Verification**: shared 31/31, healthcare 13/13, ecommerce 9/9, finance 13/13, Playwright 6/6, all four frontend builds successful. Docker runtime validation is currently blocked by local Docker Desktop metadata database I/O failures.

### Improvement Phase 7
- **SSE streaming**: new `GET /api/rag/stream` endpoint in RagController streams completed RAG answers as incremental SSE token events with final metadata (historyId + chunks). Frontend `ragStream.js` client reads the stream via fetch + ReadableStream.
- **useChatStream hook**: progressive assistant message updates during streaming, automatic fallback to `useChat` on SSE failure. All 3 demo chat pages now use `useChatStream`.
- **Analytics layer**: pluggable `tracker.js` with console backend, `AnalyticsProvider` context with auto page-view tracking. Instrumented in `useChat`, `useChatStream`, `useFileUpload`, and `FeedbackModal` for submit/success/error events.
- **Tests**: AnalyticsProvider.test.jsx, ragStream.test.jsx, useChatStream.test.jsx (frontend); Sprint7 tests (backend).

### Improvement Phase 8
- **TypeScript migration**: all 56 `.js`/`.jsx` files in `frontend/shared/src/` renamed to `.ts`/`.tsx`. Strict `tsconfig.json` with `noEmit`, `react-jsx`, bundler module resolution.
- **Shared type definitions**: `src/types/index.ts` defines 20+ interfaces/types (Message, RetrievedChunk, ChatConfig, RagPayload, StreamHandlers, ThemeConfig, AnalyticsEvent, all component props, Window augmentation).
- **Typed source**: all API modules, hooks, components, UI primitives, theme, and analytics modules annotated with parameter types, return types, useState generics, and event handler types.
- **Config updates**: package.json entry to `.ts`, added typescript + @types/react + @types/react-dom, `typecheck` script. Tailwind content globs and Storybook stories globs extended to `.ts`/`.tsx`.
- **Verification**: `tsc --noEmit` 0 errors, shared 37/37, healthcare 13/13, ecommerce 9/9, finance 13/13, all builds pass.

## Build
```bash
dotnet build ArNir.Admin/ArNir.Admin.csproj   # builds entire dependency tree
dotnet test ArNir.Tests/ArNir.Tests.csproj     # runs all 72 tests
npm install                                     # install all frontend workspaces
npm run dev --workspace=@arnir/healthcare-demo  # start healthcare demo on :3001
npm run dev --workspace=@arnir/ecommerce-demo   # start ecommerce demo on :3002
npm run dev --workspace=@arnir/finance-demo     # start finance demo on :3003
docker compose --profile demos up -d            # all 3 demos via Docker
```


