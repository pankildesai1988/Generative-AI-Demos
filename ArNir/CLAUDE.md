# ArNir — Claude Code Context

## Project
ArNir Enterprise AI Platform — .NET 9 solution
Branch: feature/arnir-7module-architecture

## Architecture Rules (NEVER BREAK THESE)
- ArNir.Services NEVER references ArNir.RAG — interface name conflicts exist
- Each new module must build with 0 errors before the next phase begins
- Existing projects (Core, Data, Services, API, Admin) must stay 100% functional

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
- Phase 4 ⬜  ArNir.Memory — NEXT
- Phase 5 ⬜  ArNir.PromptEngine
- Phase 6 ⬜  ArNir.Agents
- Phase 7 ⬜  ArNir.Tools
- Phase 8 ⬜  ArNir.Observability

## Code Standards
- .NET 9 / net9.0
- Microsoft.Extensions.* version 9.0.9
- XML doc comments on every class and interface
- Always complete files — no partial code
- Verify interface names against existing before creating new ones

## Build Commands
dotnet build ArNir.Platform/ArNir.Platform.csproj
dotnet build ArNir.RAG/ArNir.RAG.csproj
dotnet build ArNir.Services/ArNir.Services.csproj