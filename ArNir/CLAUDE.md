# ArNir — Claude Code Context

## Project
ArNir Enterprise AI Platform — .NET 9 solution
Branch: main

## Architecture Rules (NEVER BREAK THESE)
- ArNir.Services NEVER references ArNir.RAG — interface name conflicts exist
- Each new module must build with 0 errors before the next phase begins
- Existing projects (Core, Data, Services, API, Admin) must stay 100% functional
- LayeredPromptResolver is the live IPromptResolver in both ArNir.Admin and ArNir.API (DB → Config → Code chain)
- PlatformSettings table is the single source of truth for all module configuration (replaces hard-coded appsettings values)
- ArNir.Admin IS allowed to reference ArNir.RAG directly (bridge pattern — Admin MVC layer owns the dual-path upload)
- ArNir.RAG CANNOT reference ArNir.Services (IEmbeddingService / IRetrievalService name conflicts)
- IEmbeddingProvider lives in ArNir.Core.Interfaces — shared by both ArNir.Services and ArNir.RAG.Pgvector
- ArNir.RAG.Pgvector reference chain: ArNir.Core → ArNir.Data → ArNir.RAG → ArNir.RAG.Pgvector (consumed by Admin + API)
- ArNir.RAG/Hosting/ owns IngestionQueue + IngestionWorker — both Admin and API can use them without business logic in either

## Existing Interfaces — DO NOT RENAME
IRagService, IRetrievalService, IEmbeddingService, IDocumentService,
IContextMemoryService, ILlmService, IAnalyticsService, IAIInsightService

## New Module Interface Names (safe — no conflicts)
- IDocumentEmbedder     (NOT IEmbeddingService)
- IDocumentVectorStore  (NOT IRetrievalService)
- IEpisodicMemory       (NOT IContextMemoryService)
- ISemanticMemory       (new)
- IAIInsightGenerator   (NOT IAIInsightService)

## Phase Status
- Phase 0 ✅  csproj files restored
- Phase 2 ✅  ArNir.Platform complete
- Phase 3 ✅  ArNir.RAG complete
- Phase 4 ✅  ArNir.Memory complete
- Phase 5 ✅  ArNir.PromptEngine complete
- Phase 6 - Docker + Infrastructure: Complete in source, verified for tests/builds/E2E; Docker runtime validation blocked by local Docker Desktop I/O errors
- Phase 7 ✅  ArNir.Tools complete
- Phase 8 ✅  ArNir.Observability complete
- Phase 9 ✅  Wire new modules into ArNir.Services + 2 demo endpoints (Track 1 + Track 2)
- Phase 10 ✅  DB-driven settings + ArNir.Admin management panels
               Tables: PromptTemplates, PlatformSettings, AgentRunLogs, MetricEvents
               Services: DbPromptVersionStore, DbMetricCollector, PlatformSettingsService
               Resolver: LayeredPromptResolver (DB → Config → Code)
               Admin panels: PromptTemplate CRUD, Platform Settings, Agent Run History, Observability
               Document bridge: IIngestionPipeline wired into DocumentController Upload POST
               Seeded: 5 default PromptTemplates (zero-shot, few-shot, role, rag, hybrid)

- Sprint 1 ✅  Pipeline → PostgreSQL bridge + Authentication + Server-side file validation
               PgvectorDocumentEmbedder  (ArNir.RAG.Pgvector/) — calls OpenAI via IEmbeddingProvider,
                 returns float[] vectors; AddArNirRagPgvector() overrides NullDocumentEmbedder stub
               PgvectorDocumentVectorStore (ArNir.RAG.Pgvector/) — resolves SQL DocumentChunk.Id FK,
                 persists Embedding rows in PostgreSQL via pgvector <=> cosine operator
               IEmbeddingProvider moved to ArNir.Core.Interfaces — breaks Services↔RAG circular dep
               OpenAiEmbeddingProvider implements both ArNir.Services.Provider.IEmbeddingProvider
                 AND ArNir.Core.Interfaces.IEmbeddingProvider (single method, dual interface)
               IngestionRequest.LegacySqlDocumentId threads SQL doc ID through the RAG pipeline
               IngestionPipeline encodes chunkId as "sql:{docId}:{chunkIndex}" for FK resolution
               appsettings.json: Postgres conn string (AirNir_PG); OpenAI key at root; Auth section
               Cookie-based authentication (8h sliding session) + [Authorize] on all 12 controllers
               AccountController: GET/POST Login, POST Logout (antiforgery); Login.cshtml; _AuthLayout
               Logout form in _Layout.cshtml replaces broken onclick="logout()" (was undefined JS)
               Server-side file validation in DocumentController.Upload (size, MIME type, null check)
               Build: 0 errors | Tests: 12/12 Sprint 1 unit tests passed

- Sprint 2 ✅  Ops visibility + Modular refactoring + Unit test project
               [S2-T1] Platform Health Dashboard (Home/Index): live status tiles — doc count, chunk
                 count, embedding count, Postgres connectivity, embedder type (Real vs Stub), 24h SLA%,
                 24h avg latency, recent agent runs table
               [S2-T2] Vector Store Health Panel (/VectorStore): embeddings by model, orphaned chunk
                 detection (chunks with no Embedding row in pgvector), per-document rebuild trigger
               [S2-T3] Background Ingestion Queue: Channel<T>-based IngestionQueue (Singleton) +
                 IngestionWorker BackgroundService; upload returns immediately with "queued" message;
                 RecentResults ConcurrentQueue buffers last 100 job results
               [S2-T4] Provider Config UI (/ProviderConfig): OpenAI/Gemini/Claude API keys stored in
                 PlatformSettings table; masked password input; DB-first + appsettings.json fallback
               Refactoring — ArNir.Admin kept clean (controllers + views only, no business logic):
                 ArNir.RAG.Pgvector (NEW PROJECT): PgvectorDocumentEmbedder + PgvectorDocumentVectorStore
                   moved from ArNir.Admin/Infrastructure/; AddArNirRagPgvector() DI extension
                 ArNir.RAG/Hosting/ (NEW): IngestionQueue + IngestionWorker moved from ArNir.Admin/Services/
                   AddArNirRAGBackgroundIngestion() DI extension added to ArNir.RAG ServiceCollectionExtensions
                 ArNir.Core/Interfaces/IEmbeddingProvider.cs (NEW): shared interface for cross-module use
                 ArNir.Tests (NEW PROJECT): 17 xUnit unit tests (Moq) — Sprint1 (12) + Sprint2 (5)
                   PgvectorDocumentEmbedderTests, IngestionQueueTests, AccountControllerTests,
                   DocumentControllerTests, HomeControllerTests, VectorStoreControllerTests, ProviderConfigTests
               Build: 0 errors | Tests: 17/17 passed

- Sprint 3 ✅  Full feature parity: Embeddings Mgmt, Memory Panel, Agent Trigger, Job Monitor, A/B Stats
               [S3-T1] Enhanced Embeddings Management (/Embedding): stats cards (total, by model, age
                 range), Rebuild All button (enqueues all docs via IngestionQueue), Delete by Model
                 (removes Embedding rows for specific model with confirmation modal)
               [S3-T2] Memory Management Panel (/Memory): session list (SessionId, message count,
                 last activity), session transcript view, DeleteSession POST, PurgeOld(daysOld) POST
               [S3-T3] Agent Manual Trigger (/AgentRunHistory/TriggerRun): query form → calls
                 IPlannerAgent, logs AgentRunLog to DB, redirects to Index with success message
               [S3-T4] Background Job Monitor (/JobMonitor): live queue depth card + recent jobs
                 table auto-refreshing every 3s via AJAX /JobMonitor/Status JSON endpoint
               [S3-T5] Prompt A/B Statistics (/PromptTemplate/Stats): Chart.js bar chart (style vs
                 avg latency), stats table grouped by PromptStyle with SLA%, avg rating, last used
               Nav: "Job Monitor" and "Memory Sessions" nav links added to _Layout.cshtml
               ArNir.Tests/Sprint3/: 19 new unit tests (EmbeddingController, MemoryController,
                 JobMonitorController, AgentRunHistoryController)
               Build: 0 errors | Tests: 36/36 passed (Sprint1: 12, Sprint2: 5, Sprint3: 19)

- Sprint 4 ✅  Polish: Feedback Loop, Template Import/Export, Notification Center
               [S4-T1] Retrieval Quality Feedback (/RagHistory): 1–5 star rating modal on each history
                 row; AJAX POST to /RagHistory/SubmitFeedback; upsert logic (update if exists, insert
                 if new); returns JSON { success, message }; wired to existing showToast helper
               [S4-T2] Prompt Template Import/Export (/PromptTemplate): ExportJson() GET returns
                 all templates as application/json file download; ImportJson(IFormFile) POST deserialises
                 and inserts new rows (skips duplicate Style+Version); TempData success/error banners
               [S4-T3] Notification Center: NotificationController.GetUnread() AJAX endpoint queries
                 MetricEvents (last 1h, IsWithinSla=false), returns { count, alerts (last 5) };
                 navbar bell icon with red badge; Bootstrap dropdown showing breach details;
                 setInterval polling every 30s + DOMContentLoaded initial load; graceful DB failure
               ArNir.Tests/Sprint4/: 15 new unit tests (RagHistoryController: 4, PromptTemplateController: 5,
                 NotificationController: 4, + 2 edge cases)
               Build: 0 errors | Tests: 51/51 passed (Sprint1: 12, Sprint2: 5, Sprint3: 19, Sprint4: 15)

- Sprint 5 ✅  API Production Parity, Security & Polish
               [S5-T1] Wire ArNir.API for production RAG pipeline: added ArNir.RAG.Pgvector project
                 reference; Program.cs calls AddArNirRagPgvector() + AddArNirRAGBackgroundIngestion()
                 + IEmbeddingProvider forwarding (overrides null stubs with real pgvector)
               [S5-T1] DocumentIngestController refactored: dual-path pattern (Path 1 — SQL save via
                 IDocumentService, Path 2 — background IngestionQueue); returns 202 Accepted with
                 documentId; removed NullDocumentEmbedder demo remarks
               [S5-T2] Security: .gitignore added (.claude/, appsettings.Development.json, bin/obj);
                 appsettings.json Postgres password sanitized to YOUR_PASSWORD_HERE
               [S5-T3] Renamed RetrievalControllerr.cs → RetrievalController.cs (file typo fix)
               [S5-T4] Admin UI polish: footer → "ArNir Enterprise AI Platform v1.0"; brand text →
                 "ArNir Admin"; brand link → "/"; breadcrumb Home → "/"; removed Contact nav link
               ArNir.Tests/Sprint5/: 5 new unit tests (DocumentIngestControllerApiTests: no file,
                 empty file, SQL save, queue enqueue + 202, SQL doc ID verification)
               Build: 0 errors | Tests: 56/56 passed (Sprint1: 12, Sprint2: 5, Sprint3: 19, Sprint4: 15, Sprint5: 5)

- Sprint 6 ✅  Evaluation Layer — LLM-as-Judge RAG Quality Scoring
               [S6-T1] EvaluationResultEntity (ArNir.Core/Entities/): new DB table EvaluationResults with
                 Question, Answer, Context, RelevanceScore, FaithfulnessScore, Reasoning, EvaluatedAt,
                 RelatedHistoryId (optional FK to RagComparisonHistories); DbSet added to ArNirDbContext
               [S6-T2] DTOs: EvaluationRequestDto, EvaluationResultDto, EvaluationStatsDto + EvaluationTrendPoint
                 (ArNir.Core/DTOs/Evaluation/); IEvaluationHistoryService interface (ArNir.Services/Interfaces/)
               [S6-T2] LlmEvaluationService (ArNir.Services/): implements IEvaluationService from
                 ArNir.Observability; calls gpt-4o-mini with structured judge prompt; parses JSON response
                 with markdown fence stripping; clamps scores to [0.0, 1.0]; graceful fallback on errors
               [S6-T2] EvaluationHistoryService (ArNir.Services/): implements IEvaluationHistoryService;
                 paginated queries, date/score filtering, daily trend aggregation, GetByIdAsync, GetTotalCountAsync
               [S6-T3] Auto-evaluation hook in RagService.RunRagAsync: optional IEvaluationService injected;
                 after RAG history save, calls EvaluateAsync and persists EvaluationResultEntity with FK;
                 wrapped in try-catch — never breaks RAG pipeline
               [S6-T4] Admin Evaluation Panel (/Evaluation): EvaluationController with Index + Details actions;
                 KPI cards (total, avg relevance, avg faithfulness, combined score) with color-coded thresholds;
                 Chart.js dual-axis line chart (daily relevance/faithfulness trends + evaluation count);
                 DataTable with paginated results, color-coded score badges, reasoning preview, history links
               [S6-T5] API EvaluationController: GET /api/evaluation/history (paginated + filters),
                 POST /api/evaluation/evaluate (on-demand LLM evaluation + persist),
                 GET /api/evaluation/stats (aggregate stats + daily trends)
               [S6-T6] DI: both ArNir.Admin + ArNir.API register IEvaluationService → LlmEvaluationService
                 and IEvaluationHistoryService → EvaluationHistoryService; Admin adds ILlmService → OpenAiService
               EF Migration: AddEvaluationResults (EvaluationResults table + FK index)
               Nav: "Evaluation" link added to _Layout.cshtml sidebar (AI Platform section)
               ArNir.Tests/Sprint6/: 10 new unit tests (LlmEvaluationServiceTests: 6 — valid JSON, markdown
                 wrapped, out-of-range clamping, invalid JSON, LLM exception, missing fields;
                 EvaluationControllerAdminTests: 4 — empty state, populated VM, not found, found details)
               Build: 0 errors | Tests: 66/66 passed (Sprint1-5: 56, Sprint6: 10)

- Sprint 7 ✅  Demo Mode — Consulting Portfolio & Docker Setup
               [S7-T1] Root README.md: project overview, ASCII architecture diagram, prerequisites table,
                 quick start guide (6 steps), admin panel features table (17 routes), API endpoints table
                 (30+ endpoints), key technologies, sprint history, Docker full-stack instructions
               [S7-T2] Updated Postman collection (Docs/ArNir-Postman-Collection.json): all 12 API controllers
                 with example request bodies — RAG, Documents, Retrieval, Evaluation (3 endpoints),
                 Analytics (5 endpoints), Feedback (3), Chat (2), Intelligence (6), Insights (4), Agent (1);
                 + ArNir-Postman-Environment.json (baseUrl + adminUrl variables)
               [S7-T3] Demo seed data migration (SeedDemoData): 3 RagComparisonHistories (IDs 9001-9003,
                 varied providers/styles), 3 EvaluationResults (linked via FK, scores 0.78-0.95),
                 5 MetricEvents (4 within SLA + 1 breach), 2 Feedbacks (4-5 star ratings);
                 high IDs avoid conflicts with production data
               [S7-T4] Dockerfiles: Dockerfile.admin + Dockerfile.api (multi-stage .NET 9 builds);
                 docker-compose.yml updated with --profile full services (arnir-admin:5001, arnir-api:5000),
                 Postgres healthcheck, env var overrides for connection strings + OpenAI key
               [S7-T5] Architecture reference (Docs/ArNir-Architecture.md): solution structure, RAG pipeline
                 data flow diagram, document ingestion flow, database schema (11 SQL Server tables + 1
                 pgvector table), dependency rules, key design patterns (LayeredPromptResolver, optional DI,
                 dual-path ingestion), authentication model, configuration reference, sprint timeline
               Build: 0 errors | Tests: 66/66 passed (no new tests — demo/docs sprint)

- Sprint 8 ✅  Prompt Versioning — Version History, Rollback, Compare
               [S8-T1] Edit creates new version: POST /PromptTemplate/Edit now deactivates old version
                 and inserts a new row with Version = max + 1; auto-increment versioning preserves full
                 history; TempData success message shows old → new version transition
               [S8-T2] Version History Timeline (/PromptTemplate/History?style=rag): lists all versions
                 for a style ordered by version descending; active version highlighted green; template text
                 preview; rollback button on inactive versions; compare selector with two dropdowns
               [S8-T3] Rollback (/PromptTemplate/Rollback/{id}): deactivates all versions for the style,
                 creates new version with rolled-back template text + "(rollback from vN)" name suffix;
                 redirects to History view with success message
               [S8-T4] Compare (/PromptTemplate/Compare?id1=...&id2=...): side-by-side metadata cards
                 (version, name, status, created date); pre-formatted template text panels; JavaScript
                 line-by-line diff table with color-coded rows (green=added, red=removed, yellow=changed)
               [S8-T5] Index view updated: "History" button (clock icon) added per template row alongside
                 Edit and Deactivate; links to /PromptTemplate/History?style={style}
               [S8-T6] CreateEdit view updated: version-awareness banner shown when editing — "Saving will
                 create a new version (vN+1) and deactivate the current vN"
               ArNir.Tests/Sprint8/: 8 new unit tests (PromptVersioningTests: edit-creates-version with
                 deactivation verification, history returns sorted versions, empty style redirects, rollback
                 creates new active version + deactivates all others, rollback 404, compare returns both
                 versions, compare missing version 404, edit not found 404)
               Build: 0 errors | Tests: 74/74 passed (Sprint1-7: 66, Sprint8: 8)

- Phase A ✅  Demo Frontends — Shared Library (@arnir/shared)
               npm workspaces monorepo: root package.json extended with 4 workspace entries
               frontend/shared/ — @arnir/shared package (23 files)
               API modules (7): client.js (env-configurable baseURL), rag.js, chat.js, feedback.js,
                 documents.js (multipart FormData ingest), evaluation.js
               Hooks (2): useChat (configurable provider/model/promptStyle, returns messages/chunks/loading),
                 useFileUpload (drag-drop with PDF/TXT/DOCX validation, 202 handling)
               Components (8): ChatWindow (header + message list + input + feedback), FileUpload (drag-drop
                 zone + progress + success/error states), SourceViewer (collapsible chunk panels),
                 FeedbackModal (5-star rating + comment + actual API call), MessageBubble (react-markdown),
                 TypingIndicator (framer-motion animated dots), Loader, ErrorBanner
               UI primitives (3): Button (4 variants: primary/secondary/accent/ghost), Card/CardHeader/CardContent, Input
               Theme (2): themes.js (healthcare/ecommerce/finance chart colors + metadata),
                 themeContext.jsx (React context + ThemeProvider + useTheme hook)
               Barrel export: index.js exports all 30+ symbols
               Shared components use semantic colors (primary-*, accent-*) — resolved per-demo via Tailwind

- Phase B ✅  Healthcare Knowledge Assistant Demo (port 3001)
               Vite scaffold: package.json, index.html, vite.config.js (port 3001, @shared alias),
                 tailwind.config.js (teal/green palette), postcss.config.js, .env
               Components: HealthcareLayout (sidebar + nav + branding), MedicalChatPage (two-panel:
                 ChatWindow + SourceCitationPanel), SourceCitationPanel (citation count + SourceViewer),
                 MedicalUploadPage (FileUpload + "Try Sample Data" with 3 one-click sample uploads)
               Sample data (3 files): hypertension-guidelines.txt (WHO BP management, medications,
                 targets, monitoring), diabetes-treatment.txt (HbA1c targets, metformin, GLP-1, insulin,
                 hypoglycemia), drug-interactions.txt (warfarin, statins, SSRIs, ACE/ARBs, metformin)
               Tests: 11 Vitest tests (App routing, MedicalChatPage rendering, MedicalUploadPage sample data)
               Theme: teal-500→900 primary, emerald-500→900 accent

- Phase C ✅  Ecommerce Product Advisor Demo (port 3002)
               Vite scaffold: package.json, index.html, vite.config.js (port 3002),
                 tailwind.config.js (orange/amber palette), postcss.config.js
               Components: EcommerceLayout (sidebar + shopping cart branding), ProductAdvisorPage
                 (two-panel: ChatWindow + RecommendationList), ProductCard (price extraction from chunk
                 text, star ranking, document title), RecommendationList (product count badge + card grid),
                 CatalogUploadPage (FileUpload + 3 sample catalog uploads)
               Sample data (3 files): laptop-catalog.txt (10 laptops, $349-$2499, specs + use cases),
                 mobile-phones.txt (8 smartphones, $349-$1399, camera + 5G specs),
                 electronics-accessories.txt (15 items: headphones, chargers, cases, monitors, smart home)
               Tests: 8 Vitest tests (App routing, ProductAdvisorPage product cards, branding)
               Theme: orange-500→900 primary, amber-500→900 accent

- Phase D ✅  Financial Document Analyzer Demo (port 3003)
               Vite scaffold: package.json, index.html, vite.config.js (port 3003),
                 tailwind.config.js (navy/gold palette, dark sidebar), postcss.config.js
               Components: FinanceLayout (dark sidebar + gold accent branding), FinanceChatPage
                 (three-panel: ChatWindow + InsightsPanel + SourceViewer), InsightsPanel (extracts
                 $ amounts, %, risk keywords, growth indicators from RAG answer via regex),
                 MetricsCard (gold-accented cards for financial figures), FinancialUploadPage
                 (FileUpload + 3 sample report uploads)
               Sample data (3 files): annual-report-2024.txt (TechCorp $4.2B revenue, segments,
                 workforce, risk factors, capital allocation), quarterly-earnings-q3.txt ($1.15B
                 Q3 revenue, EPS beat, guidance raised), market-analysis.txt (AI industry TAM
                 $500B by 2028, key players, investment trends, 5-year forecast)
               Tests: 10 Vitest tests (App routing + InsightsPanel: dollar extraction, risk detection,
                 growth indicators, percentage extraction, empty/no-metrics states)
               Theme: slate-800→900 primary (navy), amber-400→500 accent (gold)

- Phase E ✅  Docker + Final Integration
               Dockerfiles (3): Multi-stage builds — node:22-alpine (npm install + build) → nginx:1.27-alpine
                 (serve dist + SPA routing). Each demo copies shared/ + demo/ + root package.json, runs
                 workspace-scoped install and build.
               nginx.conf (3): SPA routing (try_files → /index.html) + API reverse proxy
                 (location /api → proxy_pass http://arnir-api:5000)
               docker-compose.yml updated: 3 new services under profiles: ["demos"] —
                 healthcare-demo (3001:80), ecommerce-demo (3002:80), finance-demo (3003:80);
                 all depend_on arnir-api; build context is repo root with demo-specific Dockerfile
               README.md updated: "Demo Frontends (React)" section with npm workspace commands,
                 Docker commands, and demo table (port/theme/use case)
               All 4 project context docs updated with final Docker demo commands
               Total frontend tests: 29 (shared: 0, healthcare: 11, ecommerce: 8, finance: 10)

- Phase F ✅  API Compatibility Fixes for Demo Frontends
               [F-1] CORS: Added http://localhost:3001, :3002, :3003 to FrontendPolicy in ArNir.API/Program.cs
                 (without these, all 3 React demos were blocked by browser CORS policy)
               [F-2] DocumentIngestController: Added [FromForm] string uploadedBy = "demo-user" parameter;
                 replaced User.Identity?.Name (always null — API has no auth) with form-bound uploadedBy;
                 frontend sends FormData { file, uploadedBy } which now correctly persists on Document entity
               [F-3] FeedbackDto: Added Comment alias property (singular) → maps to Comments (plural);
                 frontend sends { comment } but DTO had Comments — System.Text.Json couldn't bind;
                 alias uses [JsonPropertyName("comment")] + getter/setter forwarding to Comments
               Build: 0 errors | Tests: all passing | Backward compatible (Admin still uses Comments)

- Improvement Phase 1 ✅  Foundation (Error Boundary, Dark Mode, Skeletons, Responsive, Tests, Pre-build)
               [1a] ErrorBoundary — class component with componentDidCatch, fallback UI, retry button
               [1b] Dark Mode — Tailwind darkMode:"class", ThemeProvider mode/toggleMode, localStorage persist
                 All shared components + UI primitives updated with dark: variants
                 All 3 Layout files rewritten with Sun/Moon toggle in sidebar
               [1c] Loading Skeletons — Skeleton (text/circle/card/chat-bubble), ChatSkeleton, CardSkeleton
               [1d] Responsive Mobile — Collapsible sidebar, hamburger menu, mobile overlay, hidden lg:block panels
               [1e] Shared Test Suite — 8 test files, 31 tests, vitest + @testing-library/react + jsdom
               [1f] Pre-build — vite lib mode for @arnir/shared, removed optimizeDeps.include from demos
               [fix] workspace:* → * for npm compatibility (pnpm not available)
               Build: all 3 demos build | Tests: 31/31 shared tests pass

 - Improvement Phase 2 ✅  Accessibility + Storybook (Accessibility hooks, ARIA, keyboard support, stories)
               [2a] Accessibility — useFocusTrap + useKeyboardNav added in shared hooks
                 Shared components updated with ARIA roles/labels: ChatWindow, MessageBubble, FileUpload,
                 FeedbackModal, SourceViewer, ErrorBanner, button, input
               [2b] Storybook — shared/.storybook config + 11 stories added for shared UI/components
               Verification follow-up commit updated demo App tests for ErrorBoundary export + BrowserRouter usage
               Tests verified: shared 31/31, healthcare 12/12, ecommerce 8/8, finance 10/10
               Builds verified: @arnir/shared + all 3 demo frontends build successfully
               Known gap: Storybook scripts exist in package.json, but Storybook CLI deps are not present in
                 node_modules/package-lock.json, so Storybook cannot run until those deps are installed

- Improvement Phase 3 ✅  Healthcare Domain Features (Document scope, highlighting, export, source viewer)
               [3a] Multi-document scope — DocumentIngestController now exposes GET /api/documents and
                 GET /api/documents/{id}; RagRequestDto, IRagService, IRetrievalService, RagController,
                 RagService, RetrievalService, shared documents API, and useChat now support optional
                 documentIds filtering
               [3b] Healthcare UI — MedicalChatPage now includes DocumentSelector, selected document scope,
                 HighlightedMessage term badges, ExportButton, and SourceDocPanel
               [3c] Export chat — healthcare demo uses jsPDF to export clinician/assistant chat transcripts
               [3d] Inline source viewer — SourceDocPanel + PdfViewer page through stored document chunks
                 and highlight retrieved medical terms/context
               Tests verified: healthcare 13/13, shared 31/31
               Builds verified: @arnir/healthcare-demo + @arnir/shared
               Cross-layer compile verified: dotnet build ArNir.sln (0 errors, warnings only)

  - Improvement Phase 4 ✅  Ecommerce Domain Features (Comparison, filters, cart, wishlist, facets)
               [4a] Product comparison — ComparisonTable + useComparison; ProductCard compare selection;
                 ProductAdvisorPage now shows side-by-side product comparison when two items are selected
               [4b] Price filters — PriceFilter adds min/max budget controls and prepends budget constraints
                 into outgoing advisor queries
               [4c] Cart and wishlist — CommerceProvider + useCart/useWishlist persist local state; cart
                 count/drawer integrated into EcommerceLayout; ProductCard supports add-to-cart and wishlist toggle
               [4d] Product image support — ProductCard reads image metadata when present and falls back
                 gracefully when no image URL is available
               [4e] Faceted search — FacetPanel + useFacets filter recommendation output by category and
                 price band; layout expanded to facets | chat | recommendations
               Tests verified: ecommerce 9/9
               Build verified: @arnir/ecommerce-demo

- Improvement Phase 5 ✅  Finance Domain Features (Charts, tables, risk scoring, compare mode, export)
               [5a] Recharts integration — FinanceChart extracts yearly/quarterly values from assistant
                 responses and renders responsive bar charts inside the insights workflow
               [5b] Tabular data extraction — DataTable parses markdown tables from assistant responses
                 and shows sortable financial rows inline beneath the chat
               [5c] Risk scoring — riskScorer computes weighted 0-100 scores from financial language and
                 RiskGauge visualizes the score with factor summaries and severity messaging
               [5d] Comparative analysis — FinanceProvider + useComparisonHistory persist recent analyses,
                 App adds /compare, FinanceLayout adds Compare Analyses nav, and ComparisonDashboard shows
                 side-by-side risk/chart/insight comparisons
               [5e] Export insights — ExportMenu exports the current analysis as PDF or XLSX using jsPDF
                 and xlsx, including chart/table/risk-derived summaries
               Tests verified: finance 13/13
               Builds verified: @arnir/finance-demo

- Improvement Phase 6 -  Docker + Infrastructure (Runtime config, health checks, CI, E2E)
               [6a] Runtime API config - shared runtime helper now resolves window.__RUNTIME_CONFIG__.API_URL
                 first, ignores the placeholder token during local dev, and falls back to Vite env/default URL
               [6b] Demo container hardening - all 3 demos now ship public/env-config.js, inject API URL via
                 entrypoint.sh, include frontend container HEALTHCHECK, and serve explicit cache headers in nginx
               [6c] Docker compose improvements - demos profile now includes arnir-api; demo services define
                 API_BASE_URL and healthcheck blocks; root .dockerignore trims node_modules, dist, bin/obj, and
                 worktree metadata from Docker build context
               [6d] Frontend CI - added parent-repo workflow .github/workflows/arnir-frontend.yml for npm
                 install, shared/demo tests + builds, Playwright browser install, and E2E smoke validation
               [6e] Playwright smoke coverage - added frontend/playwright.config.js and demo smoke tests for
                 healthcare, ecommerce, and finance core routes/upload routes
               Tests verified: shared 31/31, healthcare 13/13, ecommerce 9/9, finance 13/13, Playwright 6/6
               Builds verified: @arnir/shared, @arnir/healthcare-demo, @arnir/ecommerce-demo, @arnir/finance-demo
               Docker note: container validation reached Docker Desktop daemon/storage I/O failures after code-level
                 fixes (meta.db / metadata_v2.db), so compose runtime/header verification remains blocked locally

- Improvement Phase 7 -  Streaming + Analytics (SSE streaming, analytics layer)
               [7a] SSE streaming endpoint — added GET /api/rag/stream in RagController.cs that reuses
                 IRagService.RunRagAsync, then streams the completed RagAnswer as incremental SSE token events
                 with a final metadata event containing historyId and retrieved chunks
               [7b] SSE client — frontend/shared/src/api/ragStream.js reads SSE response via fetch +
                 ReadableStream, parsing token/metadata/done/error events
               [7c] useChatStream hook — frontend/shared/src/hooks/useChatStream.js provides streaming chat
                 with progressive assistant message updates; falls back to useChat on SSE failure
               [7d] Demo integration — all 3 chat pages (MedicalChatPage, ProductAdvisorPage, FinanceChatPage)
                 now use useChatStream instead of useChat, preserving document-scoping and demo-specific behavior
               [7e] Analytics layer — frontend/shared/src/analytics/tracker.js provides pluggable trackEvent
                 with console backend; AnalyticsProvider.jsx exposes context and auto-tracks page views
               [7f] Analytics instrumentation — useChat, useChatStream, useFileUpload, and FeedbackModal emit
                 analytics events for submit/success/error; all 3 App.jsx wrap routes in AnalyticsProvider
               Tests added: AnalyticsProvider.test.jsx, ragStream.test.jsx, useChatStream.test.jsx
               Backend tests: ArNir.Tests/Sprint7/

- Improvement Phase 8 -  TypeScript Migration (Strict TypeScript for @arnir/shared)
               [8a] tsconfig.json — strict mode, ESNext module, bundler resolution, react-jsx, noEmit
               [8b] types/index.ts — comprehensive type definitions: Message, RetrievedChunk, ChatConfig,
                 ChatHookReturn, RagPayload, StreamHandlers, ThemeConfig, ThemeContextValue, AnalyticsEvent,
                 AnalyticsBackend, all component prop interfaces, Window augmentation for runtime config
               [8c] File renames — all 56 .js/.jsx files in frontend/shared/src/ renamed to .ts/.tsx
                 via git mv (API, hooks, components, UI, theme, analytics, config, tests, stories)
               [8d] Type annotations — all source files annotated with import type, function parameter
                 types, return types, useState generics, event handler types, class component Props/State
               [8e] Config updates — package.json exports/main to .ts, added typescript + @types/react +
                 @types/react-dom devDeps, typecheck script; all 3 tailwind.config.js content globs
                 include .ts/.tsx; storybook stories glob includes .ts/.tsx; vite lib entry to .ts
               [8f] Type re-exports — index.ts re-exports 13 key types from types/index.ts for consumers
               Verified: tsc --noEmit 0 errors, shared 37/37, healthcare 13/13, ecommerce 9/9,
                 finance 13/13, all 4 frontend builds successful
## Improvement Phase Status Tracking
- Phase 1 — Foundation: Complete and verified
- Phase 2 — Accessibility + Storybook: Complete in source, verified for tests/builds; Storybook runtime currently blocked by missing installed CLI deps
- Phase 3 — Healthcare Domain Features: Complete and verified
- Phase 4 — Ecommerce Domain Features: Complete and verified
- Phase 5 — Finance Domain Features: Complete and verified on this branch
- Phase 6 - Docker + Infrastructure: Complete in source, verified for tests/builds/E2E; Docker runtime validation blocked by local Docker Desktop I/O errors
- Phase 7 — Streaming + Analytics: Complete (SSE endpoint, useChatStream, ragStream client, AnalyticsProvider, tracker)
- Phase 8 — TypeScript Migration: Complete (strict TS, 56 files renamed, types/index.ts, tsc --noEmit 0 errors)

## Latest Frontend Verification Snapshot
- Commits: 83976cb (Phase 1), 7066893 (Phase 2 features), b264ad3 (Phase 2 verification/test alignment)
- Verified commands:
  npm test --workspace=@arnir/shared
  npm test --workspace=@arnir/healthcare-demo
  npm test --workspace=@arnir/ecommerce-demo
  npm test --workspace=@arnir/finance-demo
  npm run build --workspace=@arnir/shared
  npm run build --workspace=@arnir/healthcare-demo  
  npm run build --workspace=@arnir/ecommerce-demo
  npm run build --workspace=@arnir/finance-demo
  npm run e2e
  dotnet build ArNir.sln
  docker compose up -d --build --no-deps healthcare-demo ecommerce-demo finance-demo  # blocked by local Docker Desktop I/O error

## Code Standards
- .NET 9 / net9.0
- Microsoft.Extensions.* version 9.0.9
- XML doc comments on every class and interface
- Always complete files — no partial code
- Verify interface names against existing before creating new ones

## Build Commands
dotnet build ArNir.Platform/ArNir.Platform.csproj
dotnet build ArNir.RAG/ArNir.RAG.csproj
dotnet build ArNir.Memory/ArNir.Memory.csproj
dotnet build ArNir.PromptEngine/ArNir.PromptEngine.csproj
dotnet build ArNir.Agents/ArNir.Agents.csproj
dotnet build ArNir.Tools/ArNir.Tools.csproj
dotnet build ArNir.Observability/ArNir.Observability.csproj
dotnet build ArNir.Services/ArNir.Services.csproj
dotnet build ArNir.Admin/ArNir.Admin.csproj
dotnet build ArNir.API/ArNir.API.csproj
dotnet build ArNir.RAG.Pgvector/ArNir.RAG.Pgvector.csproj
dotnet test ArNir.Tests/ArNir.Tests.csproj

## EF Core Commands (run from ArNir.Data/ directory)
dotnet ef migrations add <MigrationName> --context ArNirDbContext
dotnet ef database update --context ArNirDbContext
# Factory reads ../ArNir.Admin/appsettings.json → DefaultConnection (Server=localhost;Database=ArNir)

## Key Design-Time Note — IDesignTimeDbContextFactory
ArNirDbContextFactory implements IDesignTimeDbContextFactory<ArNirDbContext> so EF CLI tools can
create DbContext instances at design time (no running host needed). The factory reads
../ArNir.Admin/appsettings.json → ConnectionStrings.DefaultConnection with localdb fallback.
Without this factory, dotnet ef commands fail with "Unable to resolve service for DbContextOptions".





