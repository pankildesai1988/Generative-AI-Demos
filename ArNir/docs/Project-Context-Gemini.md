# ArNir Enterprise AI Platform — Gemini Project Context

## What is ArNir?
ArNir is a production-grade **.NET 9** Enterprise AI Platform demonstrating enterprise RAG, multi-provider LLM orchestration, prompt engineering, LLM-as-judge evaluation, and agent execution. Built as a consulting portfolio project showcasing full-stack AI engineering.

**Stats:** 14 projects | 19 Admin controllers | 12 API controllers | 72 unit tests | 8 sprints | Docker-ready

## Technical Stack
- **Backend:** .NET 9, ASP.NET Core MVC + Web API, Entity Framework Core 9
- **Databases:** SQL Server (12 relational tables) + PostgreSQL with pgvector (vector similarity search)
- **AI/ML:** OpenAI GPT-4/gpt-4o-mini (completions + embeddings), Google Gemini, Anthropic Claude
- **Frontend:** Bootstrap 5, AdminLTE, Chart.js, DataTables, jQuery AJAX
- **Testing:** xUnit 2.9.2, Moq 4.20, EF Core InMemory Provider
- **DevOps:** Docker multi-stage builds, docker-compose profiles

## Core Features

### RAG Pipeline
4-step: Parse (PDF/DOCX/TXT) → Chunk (sliding window) → Embed (OpenAI) → Store (pgvector). Background processing via Channel-based IngestionQueue + BackgroundService worker. Dual storage: SQL Server for relational data, PostgreSQL for vector embeddings.

### Multi-Provider LLM Orchestration
Runtime-switchable between OpenAI, Gemini, and Claude via PlatformSettings DB table. ProviderConfig UI for API key management.

### Prompt Engineering
LayeredPromptResolver: 3-layer resolution (Database → Config → Code fallback). 5 prompt styles. Full version management:
- **Edit-creates-version**: Edits create new version row (Version = max+1), deactivate old
- **History timeline**: Per-style version listing with compare selector
- **Rollback**: Restore any old version as new active version
- **Compare**: Side-by-side diff with JS line-by-line highlighting

### LLM-as-Judge Evaluation
Auto-scores every RAG response using gpt-4o-mini on two dimensions:
- **Relevance** (0-1): Does the answer address the question?
- **Faithfulness** (0-1): Is the answer grounded in the provided context?
Persisted in EvaluationResults table. Admin dashboard with KPI cards, Chart.js trend lines, color-coded DataTable.

### Agent Execution
IPlannerAgent multi-step orchestration with AgentRunLog persistence. Manual trigger from admin UI.

### Observability
DbMetricCollector for SLA metrics. Latency tracking. Notification center (bell icon with 30s polling for SLA breaches). Evaluation trends.

## Architecture (14 Projects)
```
ArNir.Core         → Entities, DTOs, IEmbeddingProvider (base layer, no refs)
ArNir.Platform     → Enums, constants, config POCOs (no refs)
ArNir.Data         → EF Core: SQL Server + pgvector contexts, migrations
ArNir.RAG          → Ingestion pipeline, null stubs, parsers, IngestionQueue
ArNir.RAG.Pgvector → Production embedder + vector store
ArNir.Memory       → IEpisodicMemory, ISemanticMemory
ArNir.PromptEngine → LayeredPromptResolver, IPromptVersionStore
ArNir.Agents       → IPlannerAgent
ArNir.Tools        → IAgentTool implementations
ArNir.Observability→ IMetricCollector, IEvaluationService, DbMetricCollector
ArNir.Services     → All domain services (RagService, LlmEvaluationService, etc.)
ArNir.Admin        → 19 MVC controllers, Bootstrap 5, cookie auth
ArNir.API          → 12 REST controllers, Swagger
ArNir.Tests        → 72 xUnit tests across 8 sprint folders
```

## Critical Architecture Rules
1. **Services NEVER references RAG** (and vice versa) — interface name conflicts
2. IEmbeddingProvider lives in ArNir.Core — shared by both layers
3. Admin is clean — controllers + views only; all logic in module projects
4. Null stubs registered first (Singleton), real impls override (Scoped, last wins)
5. Optional DI pattern: `IEvaluationService?` in RagService — never breaks pipeline

## API ↔ Demo Frontend Compatibility
- **CORS**: FrontendPolicy allows localhost:3001-3003 for demo frontends
- **DocumentIngest**: Accepts `uploadedBy` from multipart form data (defaults to "demo-user")
- **Feedback**: FeedbackDto accepts both `comment` (singular, React demos) and `Comments` (plural, Admin)

## Sprint History
| Sprint | What Was Built |
|---|---|
| 1 | pgvector bridge, cookie auth, server-side validation (12 tests) |
| 2 | Health dashboard, background ingestion, provider config (5 tests) |
| 3 | Embeddings management, memory panel, agent trigger, job monitor, A/B stats (19 tests) |
| 4 | Feedback (1-5 star), template import/export, notification center (13 tests) |
| 5 | API production parity, DocumentIngest endpoint, Swagger (5 tests) |
| 6 | Evaluation layer — LLM-as-judge, auto-eval hook, admin+API controllers (10 tests) |
| 7 | README, Dockerfiles, docker-compose, Postman collection, architecture docs, demo seed data |
| 8 | Prompt versioning — history, rollback, compare, version-aware editing (8 tests) |

## Docker Deployment
```bash
docker compose --profile full up -d   # PostgreSQL:5432 + PgAdmin:5050 + ArNir.API:5000 + ArNir.Admin:5001
docker compose --profile demos up -d  # Healthcare:3001 + Ecommerce:3002 + Finance:3003
```

## Demo Frontends (React — npm Workspaces Monorepo)
3 industry-specific React demo frontends sharing `@arnir/shared` component library (23 files). All consume the same backend API.

| Demo | Port | Theme | Use Case |
|------|------|-------|----------|
| Healthcare | 3001 | Teal/green | Medical doc Q&A with source citations |
| Ecommerce | 3002 | Orange/amber | Product recommendations from uploaded catalog |
| Finance | 3003 | Navy/gold | Financial report analysis + insights extraction |

**Shared library**: API modules (env-configurable axios), hooks (useChat, useFileUpload), components (ChatWindow, FileUpload, SourceViewer, FeedbackModal with 5-star + API call, MessageBubble, TypingIndicator), UI primitives (Button with 4 variants, Card, Input), theme (React context + chart colors).

**Stack**: Vite 7.1.7 + React 19.1.1 + TailwindCSS 3.4.13 + Framer Motion + Lucide React + Axios

### Improvement Phase 1: Foundation (Completed)
- **ErrorBoundary**: Class component with componentDidCatch, fallback UI, retry button — wraps all 3 App.jsx
- **Dark Mode**: Tailwind `darkMode: "class"`, ThemeProvider mode/toggleMode, localStorage persistence, all components have `dark:` variants, Sun/Moon toggle in all layouts
- **Loading Skeletons**: Skeleton (text/circle/card/chat-bubble), ChatSkeleton, CardSkeleton — animate-pulse
- **Responsive Mobile**: Collapsible sidebar with hamburger menu, mobile overlay backdrop, `hidden lg:block` side panels
- **Shared Test Suite**: 8 test files, 31 tests (vitest + @testing-library/react + jsdom)
- **Pre-build**: Vite library mode for @arnir/shared, removed `optimizeDeps.include` workaround from demos

### Improvement Phase 2: Accessibility + Storybook (Completed, Build/Test Verified)
- **Accessibility**: `useFocusTrap` + `useKeyboardNav` added in shared hooks. Shared components updated with ARIA roles/labels and better keyboard navigation support.
- **Storybook Source**: shared `.storybook` config and 11 stories added for reusable UI/components.
- **Verification**: shared 31/31 tests, healthcare 12/12, ecommerce 8/8, finance 10/10. Shared library and all 3 demo builds succeed.
- **Current Gap**: Storybook CLI dependencies are declared but not currently installed, so Storybook runtime validation is pending install.

### Improvement Phase 3: Healthcare Domain Features (Completed, Build/Test Verified)
- **Multi-document healthcare chat**: API and shared hooks now support optional `documentIds`, and healthcare users can choose document scope from the new selector.
- **Healthcare-specific UI**: Added highlighted medical term badges, chat export via jsPDF, and a source document panel with inline chunk-page navigation/highlighting.
- **Verification**: healthcare 13/13, shared 31/31, `@arnir/healthcare-demo` build OK, `@arnir/shared` build OK, `dotnet build ArNir.sln` OK with warnings only.

### Improvement Phase 4: Ecommerce Domain Features (Completed, Build/Test Verified)
- **Ecommerce UX**: Added comparison, wishlist, cart drawer, price range controls, and facet filtering without changing the shared package.
- **Product presentation**: Recommendation cards now support product image metadata, compare selection, and richer product attributes.
- **Verification**: ecommerce 9/9, `@arnir/ecommerce-demo` build OK.

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
- **Finance demo enhancements**: FinanceChatPage now feeds reusable finance utilities for chart extraction, markdown table extraction, and weighted risk scoring, all surfaced in the updated insights panel.
- **Compare and export**: the demo includes `/compare` routing, persisted comparison history, side-by-side dashboards, and PDF/XLSX export for the latest analysis.
- **Verification**: finance 13/13, finance build OK.

### Improvement Phase 6
- **Container/runtime updates**: all 3 demos now inject API_BASE_URL at startup via env-config.js and entrypoint.sh, with health checks and explicit nginx cache headers for assets versus entrypoint files.
- **Quality gates**: added parent-level GitHub Actions workflow and Playwright smoke coverage for each demo, plus root .dockerignore to shrink Docker build context.
- **Verification**: shared 31/31, healthcare 13/13, ecommerce 9/9, finance 13/13, Playwright 6/6, all frontend builds OK. Docker runtime validation is blocked by local Docker Desktop metadata I/O failures.

### Improvement Phase 7
- **SSE streaming**: new `GET /api/rag/stream` endpoint in RagController streams completed RAG answers as incremental SSE token events with final metadata (historyId + retrieved chunks). Frontend `ragStream.js` client reads the stream via fetch + ReadableStream.
- **useChatStream hook**: progressive assistant message updates during streaming, automatic fallback to `useChat` on SSE failure. All 3 demo chat pages (MedicalChatPage, ProductAdvisorPage, FinanceChatPage) now use `useChatStream`.
- **Analytics**: pluggable `tracker.js` with console backend, `AnalyticsProvider` context with auto page-view tracking. Instrumented in useChat, useChatStream, useFileUpload, and FeedbackModal for submit/success/error events.
- **Tests**: AnalyticsProvider.test.jsx, ragStream.test.jsx, useChatStream.test.jsx (frontend); Sprint7 tests (backend).

## Build & Test
```bash
dotnet build ArNir.Admin/ArNir.Admin.csproj   # builds entire dependency tree
dotnet test ArNir.Tests/ArNir.Tests.csproj     # 72 tests, all passing
npm install                                     # install all frontend workspaces
npm run dev --workspace=@arnir/healthcare-demo  # start healthcare demo on :3001
npm run dev --workspace=@arnir/ecommerce-demo   # start ecommerce demo on :3002
npm run dev --workspace=@arnir/finance-demo     # start finance demo on :3003
docker compose --profile demos up -d            # all 3 demos via Docker
```


