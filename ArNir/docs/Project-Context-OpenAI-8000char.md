# ArNir — .NET 9 Enterprise AI Platform

## Overview
Production-grade RAG platform: 14 .NET 9 projects, 19 Admin MVC controllers, 12 REST API controllers, 72 unit tests, Docker-ready.

## Capabilities
- **RAG**: Parse PDF/DOCX/TXT → Chunk → Embed (OpenAI) → Store (pgvector). Background queue.
- **Multi-LLM**: OpenAI GPT-4, Gemini, Claude — runtime switchable via DB settings.
- **Prompt Engine**: 3-layer resolution (DB → Config → Code). 5 styles. Version management: edit-creates-version, history, rollback, compare.
- **Evaluation (LLM-as-Judge)**: Auto-scores RAG answers on Relevance + Faithfulness (0-1) using gpt-4o-mini.
- **Agent**: IPlannerAgent multi-step orchestration.
- **Observability**: SLA metrics, latency tracking, notification center.

## Architecture
```
ArNir.Core (entities, DTOs, IEmbeddingProvider)
ArNir.Platform (enums, config POCOs)
ArNir.Data (EF: ArNirDbContext SQL Server + VectorDbContext pgvector)
ArNir.RAG (pipeline interfaces, null stubs, IngestionQueue/Worker)
ArNir.RAG.Pgvector (PgvectorDocumentEmbedder, PgvectorDocumentVectorStore)
ArNir.Memory (IEpisodicMemory, ISemanticMemory)
ArNir.PromptEngine (LayeredPromptResolver, IPromptVersionStore)
ArNir.Agents (IPlannerAgent)
ArNir.Tools (IAgentTool impls)
ArNir.Observability (IMetricCollector, IEvaluationService, DbMetricCollector)
ArNir.Services (RagService, LlmEvaluationService, EvaluationHistoryService, DocumentService, etc.)
ArNir.Admin (19 MVC controllers, Bootstrap 5, cookie auth)
ArNir.API (12 REST controllers, Swagger)
ArNir.Tests (72 xUnit tests)
```

## CRITICAL Rules
1. Services MUST NEVER reference RAG (name conflicts: IEmbeddingService, IRetrievalService)
2. RAG MUST NEVER reference Services
3. IEmbeddingProvider in ArNir.Core — shared by Services + RAG.Pgvector
4. Admin is clean — controllers+views only; logic in module projects

## DB Schema
SQL Server (12 tables): Documents, DocumentChunks, ChatSessions, ChatMessages, RagComparisonHistories, Feedbacks, ExportHistories, ChatMemories, PromptTemplates, PlatformSettings, AgentRunLogs, MetricEvents, EvaluationResults

PostgreSQL: Embeddings(vector(1536)), ChatEmbeddings(vector(1536))

## Key Patterns
- Optional DI: `IEvaluationService?` in RagService — never breaks pipeline
- Edit-creates-version: edits create Version=max+1, deactivate old
- Null stubs→real: AddArNirRAG() then AddArNirRagPgvector() (last wins)
- IDbContextFactory for short-lived contexts
- Channel-based IngestionQueue → BackgroundService

## Admin Controllers (19)
Account (auth), Home (health), Document (dual upload), Embedding, VectorStore, Memory, AgentRunHistory, JobMonitor (AJAX), PromptTemplate (CRUD+versioning+History+Rollback+Compare+Stats+Import/Export), RagHistory (filters+feedback), RagComparison, PlatformSettings, ProviderConfig (API keys), ObservabilityDashboard, Analytics, Retrieval, Reports (Excel/CSV/PDF), Evaluation (LLM-as-judge KPIs+Chart.js+DataTable), Notification (bell)

## API Controllers (12)
DocumentIngest (multipart + uploadedBy form field), Rag, Chat, Analytics, Feedback (comment/Comments alias), Agent, Intelligence, IntelligenceChat, Insights, Retrieval, Evaluation (GET /history, POST /evaluate, GET /stats). CORS: localhost:3001-3003 for demos.

## Docker
```bash
docker compose --profile full up -d   # Postgres + Admin:5001 + API:5000
docker compose --profile demos up -d  # Healthcare:3001 + Ecommerce:3002 + Finance:3003
```

## 8 Sprints
S1: pgvector, auth (12 tests) | S2: health, ingestion (5) | S3: embeddings, memory, agents (19) | S4: feedback, templates, notifications (13) | S5: API parity (5) | S6: LLM evaluation (10) | S7: Docker, README, Postman | S8: prompt versioning (8)

## Demo Frontends (React)
3 React demos via npm workspaces monorepo, sharing @arnir/shared (23 files: API clients, hooks, components, UI, theme).
- Healthcare (port 3001, teal) — medical doc Q&A with source citations
- Ecommerce (port 3002, orange) — product recommendations from catalog
- Finance (port 3003, navy/gold) — financial report analysis + insights

Stack: Vite 7.1.7 + React 19.1.1 + TailwindCSS + Framer Motion + Axios. Semantic `primary-*`/`accent-*` colors per demo.

**Improvement Phase 1 (Foundation)**: ErrorBoundary (class component, fallback+retry), dark mode (Tailwind `darkMode:"class"`, ThemeProvider toggleMode, localStorage), loading skeletons (animate-pulse), responsive mobile layout (collapsible sidebar, hamburger menu), shared test suite (31 tests, vitest), Vite library pre-build.

**Improvement Phase 2 (Accessibility + Storybook)**: `useFocusTrap` + `useKeyboardNav`, ARIA roles/labels across shared components, shared Storybook config and 11 stories. Verified: shared 31/31, healthcare 12/12, ecommerce 8/8, finance 10/10, plus successful builds for the shared library and all 3 demo frontends. Remaining gap: Storybook CLI packages are declared but not installed, so Storybook runtime validation is still pending.

**Improvement Phase 3 (Healthcare Domain Features)**: Added `documentIds`-aware healthcare chat flow across API/shared/frontend, plus `DocumentSelector`, `HighlightedMessage`, `ExportButton`, `SourceDocPanel`, and inline chunk-page `PdfViewer`. Verified: healthcare 13/13, shared 31/31, successful builds for `@arnir/healthcare-demo` and `@arnir/shared`, and `dotnet build ArNir.sln` with 0 errors.

**Improvement Phase 4 (Ecommerce Domain Features)**: Added local ecommerce commerce-state handling (`CommerceProvider`, cart, wishlist, comparison), `PriceFilter`, `FacetPanel`, `ComparisonTable`, `CartDrawer`, richer `ProductCard` presentation, and budget-aware advisor prompts. Verified: ecommerce 9/9 and successful build for `@arnir/ecommerce-demo`.

**Ecommerce Demo Bug Fix (Product Parsing, Count-Limiting & Image Support)**: Fixed 5 rendering bugs caused by RAG backend serializing chunk text without newlines — `^Label:` regex extractions failed silently, producing corrupted titles, Category="General", all specs N/A, and no images. Added `normalizeChunkText()` (inserts `\n` before 19 field labels), `splitOnProductBoundaries()` (multi-product chunk splitting), and `buildProductsFromChunks()` deduplication. Added Pattern 3 to `parseRequestedCount()` for bare-digit queries; `displayedProducts` useMemo enforces exact count. Restructured all 3 catalog files so `Category:` and `Image URL:` appear at lines 2–3 of every product. Verified: ecommerce 9/9.

**Improvement Phase 5 (Finance Domain Features)**: Added `FinanceChart` for extracted revenue/percentage trends, `DataTable` for markdown financial tables, `riskScorer` + `RiskGauge` for weighted risk analysis, persisted comparison history with `/compare`, and `ExportMenu` for PDF/XLSX exports. Verified: finance 13/13 and successful `@arnir/finance-demo` build.

**Improvement Phase 6 (Docker + Infrastructure)**: Added runtime API config resolution in the shared client, per-demo env-config.js and entrypoint.sh, frontend container health checks, nginx cache policy, root .dockerignore, a parent-level arnir-frontend.yml workflow, and Playwright smoke tests for all three demos. Verified: shared 31/31, healthcare 13/13, ecommerce 9/9, finance 13/13, Playwright 6/6, and successful frontend builds. Docker runtime validation is blocked locally by Docker Desktop metadata database I/O failures.
**Improvement Phase 7 (Streaming + Analytics)**: Added SSE streaming endpoint `GET /api/rag/stream` in RagController that streams completed RAG answers as progressive token events. Frontend `ragStream.js` SSE client and `useChatStream` hook provide progressive message rendering with fallback to `useChat`. Pluggable analytics layer (`tracker.js` + `AnalyticsProvider`) auto-tracks page views and instruments chat/upload/feedback events. All 3 demo chat pages now use streaming.

**Improvement Phase 8 (TypeScript Migration)**: All 56 .js/.jsx files in @arnir/shared renamed to .ts/.tsx. Strict tsconfig.json. Comprehensive types/index.ts defines Message, RetrievedChunk, ChatConfig, RagPayload, StreamHandlers, ThemeConfig, AnalyticsEvent, and all component prop interfaces. All source files annotated with parameter types, return types, and useState generics. Verified: tsc --noEmit 0 errors, all tests pass, all builds succeed.

**Improvement Phase Tracker**: P1 complete+verified; P2 complete in source and verified for tests/builds; P3 healthcare complete+verified; P4 ecommerce complete+verified; P5 finance complete+verified; P6 infrastructure complete in source + verified for tests/builds/E2E with Docker runtime blocked locally; P7 streaming+analytics complete; P8 TypeScript migration complete+verified.



