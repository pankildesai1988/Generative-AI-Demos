# ArNir — .NET 9 Enterprise AI Platform (Perplexity Context)

## Summary
ArNir is a production-grade enterprise AI platform built with .NET 9 demonstrating RAG (Retrieval-Augmented Generation), multi-provider LLM orchestration, prompt engineering with version management, LLM-as-judge evaluation, and agent execution. It's a consulting portfolio project with 14 projects, 19 admin controllers, 12 API controllers, and 72 passing unit tests.

## Tech Stack
.NET 9 | ASP.NET Core MVC + Web API | EF Core 9 | SQL Server (12 tables) | PostgreSQL + pgvector (2 tables) | OpenAI GPT-4/gpt-4o-mini | Google Gemini | Anthropic Claude | Bootstrap 5 | Chart.js | Docker | xUnit + Moq

## Key Features
1. **RAG Pipeline**: Parse PDF/DOCX/TXT → Chunk (sliding window) → Embed (OpenAI) → Store (pgvector). Background Channel-based queue.
2. **Multi-LLM**: OpenAI, Gemini, Claude — runtime switchable via DB settings.
3. **Prompt Versioning**: 3-layer resolver (DB → Config → Code). Edit-creates-version, history timeline, rollback, side-by-side compare with diff.
4. **LLM-as-Judge**: Auto-scores RAG responses on Relevance + Faithfulness (0-1) using gpt-4o-mini. Dashboard with KPIs and Chart.js trends.
5. **Agent Execution**: IPlannerAgent multi-step orchestration with logging.
6. **Observability**: SLA metrics, latency tracking, notification center (bell icon, 30s polling).
7. **Admin Panel**: 19 controllers — health dashboard, document management (dual upload: SQL + pgvector), embedding management, memory panel, agent trigger, job monitor, prompt template CRUD with versioning, RAG history with feedback, platform settings, provider config, observability dashboard, analytics, reports (Excel/CSV/PDF), evaluation dashboard.
8. **REST API**: 12 controllers — document ingest (multipart + uploadedBy), RAG execution, chat, analytics, feedback (comment/Comments alias), agent, intelligence, insights, retrieval, evaluation. CORS allows demo ports 3001-3003.

## Architecture Rules (Critical)
- Services NEVER references RAG (interface name conflicts)
- IEmbeddingProvider in ArNir.Core (shared base)
- Admin is clean: controllers + views only
- Null stubs (Singleton) → real impls (Scoped, last wins)
- Optional DI: IEvaluationService? in RagService

## 8 Sprints Completed
S1: pgvector + auth (12 tests) | S2: health + ingestion (5) | S3: embeddings + memory + agents (19) | S4: feedback + templates + notifications (13) | S5: API parity (5) | S6: LLM evaluation (10) | S7: Docker + README + Postman | S8: prompt versioning (8)

## Demo Frontends (React)
3 React demos (npm workspaces) sharing @arnir/shared (23 files: API, hooks, components, UI, theme):
- Healthcare (port 3001, teal) — medical doc Q&A + source citations
- Ecommerce (port 3002, orange) — product advisor from catalog
- Finance (port 3003, navy/gold) — financial report analysis + insights
Stack: Vite 7.1.7 + React 19.1.1 + TailwindCSS + Framer Motion + Axios
**Improvement Phase 1**: ErrorBoundary, dark mode (Tailwind class + toggle), loading skeletons (animate-pulse), responsive mobile sidebar, 31 shared tests (vitest), Vite library pre-build.

**Improvement Phase 2**: Accessibility hooks (`useFocusTrap`, `useKeyboardNav`), ARIA roles/labels across shared components, and Storybook source files/stories for the shared library. Verification is green for tests/builds: shared 31/31, healthcare 12/12, ecommerce 8/8, finance 10/10, and all frontend builds succeed. Storybook runtime remains pending because CLI deps are declared but not installed.

**Improvement Phase 3**: Healthcare demo now supports document-scoped chat (`documentIds` end to end), highlighted medical terms, jsPDF chat export, and an inline source document panel/viewer backed by retrieved chunks. Verification is green for healthcare/shared + backend compile: healthcare 13/13, shared 31/31, healthcare build OK, shared build OK, `dotnet build ArNir.sln` OK with warnings only.

**Improvement Phase 4**: Ecommerce demo now supports side-by-side product comparison, budget-aware chat prompts, local cart + wishlist state, image metadata rendering, and facet-filtered recommendations. Verification is green: ecommerce 9/9 and ecommerce build OK.

**Improvement Phase 5**: Finance demo now adds chart extraction (`FinanceChart`), markdown table rendering (`DataTable`), weighted risk scoring (`riskScorer` + `RiskGauge`), compare mode (`/compare` + `ComparisonDashboard`), and PDF/XLSX export (`ExportMenu`). Verification is green for Phase 5: finance 13/13 and finance build OK.

**Improvement Phase 6**: Added runtime API config injection (window.__RUNTIME_CONFIG__ + env-config.js), frontend Docker health checks, nginx cache headers, a root .dockerignore, a parent-repo frontend workflow, and Playwright smoke tests for all 3 demos. Verification is green for shared/demo tests, frontend builds, and Playwright (6/6). Docker runtime validation is currently blocked by local Docker Desktop metadata I/O failures.

**Improvement Phase 7 (Streaming + Analytics)** — SSE streaming endpoint `GET /api/rag/stream` in RagController streams completed RAG answers as progressive token events. Frontend `ragStream.js` + `useChatStream` hook provide progressive rendering with fallback to useChat. Analytics layer (`tracker.js` + `AnalyticsProvider`) auto-tracks page views and instruments chat/upload/feedback events. All 3 demos now use streaming.

**Improvement Phase 8 (TypeScript Migration)** — All 56 .js/.jsx files in @arnir/shared renamed to .ts/.tsx. Strict tsconfig.json. types/index.ts defines Message, RetrievedChunk, ChatConfig, RagPayload, StreamHandlers, ThemeConfig, AnalyticsEvent, and all component prop interfaces. All source files fully typed. Verified: tsc --noEmit 0 errors, all tests pass, all builds succeed.

**Improvement Phase Tracker**: P1 complete+verified | P2 complete in source + verified for tests/builds | P3 healthcare complete+verified | P4 ecommerce complete+verified | P5 finance complete+verified | P6 infra complete in source + verified for tests/builds/E2E, Docker runtime blocked locally | P7 streaming+analytics complete | P8 TypeScript complete+verified


