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
               PgvectorDocumentEmbedder  (ArNir.Admin/Infrastructure/) — calls OpenAI via IEmbeddingProvider,
                 returns float[] vectors; overrides NullDocumentEmbedder stub registered by AddArNirRAG()
               PgvectorDocumentVectorStore (ArNir.Admin/Infrastructure/) — resolves SQL DocumentChunk.Id
                 from "sql:{docId}:{chunkIndex}" encoded chunkId, persists Embedding rows in PostgreSQL;
                 SearchAsync uses raw SQL with pgvector <=> operator (matches RetrievalService pattern);
                 DeleteByDocumentAsync cleans up all embeddings for a given SQL document ID
               IEmbeddingProvider resolved via ArNir.Services.Provider (no circular dep — both
                 Admin and Services already share a reference to ArNir.Core)
               IngestionRequest.LegacySqlDocumentId added to thread SQL doc ID through pipeline
               IngestionPipeline updated to encode chunkId as "sql:{docId}:{chunkIndex}"
               appsettings.json: ConnectionStrings.Postgres added; OpenAI key moved to root level;
                 Auth section added (AdminUsername / AdminPassword)
               Cookie-based authentication (8h sliding session) + [Authorize] on all 12 controllers
               AccountController: GET/POST Login, POST Logout (antiforgery protected)
               Login.cshtml + _AuthLayout.cshtml (minimal sidebar-free layout for auth pages)
               Logout form in _Layout.cshtml replaced broken onclick="logout()" JS (was undefined)
               Server-side file validation in DocumentController.Upload (size, MIME type, null check)
               Build: 0 errors (warnings only — AutoMapper CVE in third-party package)

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

## EF Core Commands (run from ArNir.Data/ directory)
dotnet ef migrations add <MigrationName> --context ArNirDbContext
dotnet ef database update --context ArNirDbContext
# Factory reads ../ArNir.Admin/appsettings.json → DefaultConnection (Server=localhost;Database=ArNir)

## Key Design-Time Note — IDesignTimeDbContextFactory
ArNirDbContextFactory implements IDesignTimeDbContextFactory<ArNirDbContext> so EF CLI tools can
create DbContext instances at design time (no running host needed). The factory reads
../ArNir.Admin/appsettings.json → ConnectionStrings.DefaultConnection with localdb fallback.
Without this factory, dotnet ef commands fail with "Unable to resolve service for DbContextOptions".