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
DocumentIngest, Rag, Chat, Analytics, Feedback, Agent, Intelligence, IntelligenceChat, Insights, Retrieval, Evaluation (GET /history, POST /evaluate, GET /stats)

## Docker
```bash
docker compose --profile full up -d  # Postgres + Admin:5001 + API:5000
```

## 8 Sprints
S1: pgvector, auth (12 tests) | S2: health, ingestion (5) | S3: embeddings, memory, agents (19) | S4: feedback, templates, notifications (13) | S5: API parity (5) | S6: LLM evaluation (10) | S7: Docker, README, Postman | S8: prompt versioning (8)

## Build
```bash
dotnet build ArNir.Admin/ArNir.Admin.csproj
dotnet test ArNir.Tests/ArNir.Tests.csproj  # 72 tests
```
