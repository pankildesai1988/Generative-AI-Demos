# ArNir — Claude Code Context

## Project
ArNir Enterprise AI Platform — .NET 9 solution
Branch: AI_Consultant_Update

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
- Phase 6 ✅  ArNir.Agents complete
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