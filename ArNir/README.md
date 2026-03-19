# ArNir Enterprise AI Platform

A production-grade **Retrieval-Augmented Generation (RAG)** platform built with **.NET 9**, featuring multi-provider LLM support, vector search, prompt engineering, autonomous agents, real-time observability, and LLM-as-judge quality evaluation.

---

## Architecture

```
                          +-------------------+
                          |   ArNir.Admin     |  (MVC + AdminLTE)
                          |   Port 5001       |
                          +--------+----------+
                                   |
        +-------------------+------+------+-------------------+
        |                   |             |                   |
+-------v-------+  +-------v-------+  +--v-----------+  +---v-----------+
| ArNir.Services|  | ArNir.RAG     |  | ArNir.Memory |  | ArNir.Agents  |
| (Business)    |  | (Ingestion)   |  | (Episodic)   |  | (Planner)     |
+-------+-------+  +-------+-------+  +--------------+  +---+-----------+
        |                   |                                |
+-------v-------+  +-------v-----------+            +-------v-------+
| ArNir.Core    |  | ArNir.RAG.Pgvector|            | ArNir.Tools   |
| (Entities/DTOs)|  | (Real embeddings) |            | (Doc/Web)     |
+-------+-------+  +-------+-----------+            +---------------+
        |                   |
+-------v-------+  +-------v-------+
| ArNir.Data    |  | ArNir.Platform|     +-------------------+
| (EF Core)     |  | (Enums/Config)|     | ArNir.Observability|
+---+-------+---+  +---------------+     | (Metrics/SLA)     |
    |       |                             +-------------------+
    v       v                                      |
 SQL Server  PostgreSQL                   +--------v----------+
 (Entities)  (pgvector)                   | ArNir.PromptEngine|
                                          | (3-Layer Resolver) |
                          +-------------------+----------------+
                          |   ArNir.API       |
                          |   Port 5000       |  (REST + Swagger)
                          +-------------------+
```

### 14 Projects

| Project | Purpose |
|---------|---------|
| **ArNir.Core** | Entities, DTOs, shared interfaces |
| **ArNir.Data** | EF Core DbContexts, migrations, repositories |
| **ArNir.Platform** | Enums, configuration models |
| **ArNir.Services** | Business logic, LLM providers (OpenAI/Gemini/Claude) |
| **ArNir.RAG** | Ingestion pipeline, parsers, chunkers, background worker |
| **ArNir.RAG.Pgvector** | Real pgvector embeddings + vector store |
| **ArNir.Memory** | Episodic + semantic memory |
| **ArNir.PromptEngine** | 3-layer prompt resolver (DB > Config > Code) |
| **ArNir.Agents** | Autonomous planner agent with tool execution |
| **ArNir.Tools** | Document lookup + web fetch tools |
| **ArNir.Observability** | Metric collection, SLA rules, evaluation interface |
| **ArNir.Admin** | MVC admin panel (AdminLTE, 19 controllers) |
| **ArNir.API** | REST API (Swagger, 12 controllers, 30+ endpoints) |
| **ArNir.Tests** | xUnit + Moq (66 unit tests) |

---

## Prerequisites

| Requirement | Version |
|------------|---------|
| .NET SDK | 9.0+ |
| SQL Server | 2019+ (or LocalDB) |
| PostgreSQL | 14+ with [pgvector](https://github.com/pgvector/pgvector) extension |
| Docker (optional) | 24+ (for PostgreSQL + pgAdmin) |
| OpenAI API Key | Required for RAG, evaluation, and embeddings |

---

## Quick Start

### 1. Clone & Restore

```bash
git clone <repo-url>
cd ArNir
dotnet restore ArNir.sln
```

### 2. Start PostgreSQL (Docker)

```bash
docker compose up -d postgres pgadmin
```

This starts pgvector on port `5432` and pgAdmin on port `8080`.

### 3. Configure Connection Strings

**ArNir.Admin/appsettings.json:**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=ArNir;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True;",
    "Postgres": "Host=localhost;Database=airnir;Username=postgres;Password=YOUR_PASSWORD_HERE"
  }
}
```

**ArNir.API/appsettings.json:**
```json
{
  "ConnectionStrings": {
    "SqlServer": "Server=localhost;Database=AirNir_SQL;Trusted_Connection=True;MultipleActiveResultSets=true;",
    "Postgres": "Host=localhost;Database=AirNir_PG;Username=postgres;Password=YOUR_PASSWORD_HERE"
  }
}
```

### 4. Set OpenAI API Key

Add to both `appsettings.json` files:
```json
{
  "OpenAI": {
    "ApiKey": "sk-..."
  }
}
```

Or via environment variable:
```bash
export OpenAI__ApiKey="sk-..."
```

### 5. Apply Migrations

```bash
# SQL Server migrations
dotnet ef database update --context ArNirDbContext --project ArNir.Data --startup-project ArNir.Admin

# PostgreSQL migrations (vector store)
dotnet ef database update --context VectorDbContext --project ArNir.Data --startup-project ArNir.Admin
```

### 6. Run

```bash
# Terminal 1 — Admin Panel
cd ArNir.Admin
dotnet run    # https://localhost:5001

# Terminal 2 — REST API
cd ArNir.API
dotnet run    # https://localhost:5000/swagger
```

### 7. Run with Docker (Full Stack)

```bash
docker compose --profile full up -d
```

This starts PostgreSQL, pgAdmin, ArNir.Admin (port 5001), and ArNir.API (port 5000).

---

## Admin Panel Features

| Panel | Route | Description |
|-------|-------|-------------|
| Dashboard | `/` | Platform health: doc count, chunks, embeddings, SLA%, latency |
| Documents | `/Document` | Upload, manage, view chunks |
| Embedding Test | `/Embedding` | Stats, rebuild all, delete by model |
| Retrieval Debug | `/Retrieval` | Vector search testing |
| RAG Comparison | `/RagComparison` | Side-by-side baseline vs RAG answers |
| RAG History | `/RagHistory` | Query history with star ratings |
| Analytics | `/Analytics` | Latency, SLA, prompt usage charts |
| Reports | `/Reports` | Generated reports |
| Prompt Templates | `/PromptTemplate` | CRUD, import/export, A/B stats |
| Platform Settings | `/PlatformSettings` | DB-driven configuration |
| Agent Run History | `/AgentRunHistory` | Autonomous agent logs + trigger |
| Evaluation | `/Evaluation` | LLM-as-judge quality scoring |
| Observability | `/ObservabilityDashboard` | Provider metrics, SLA dashboard |
| Job Monitor | `/JobMonitor` | Background ingestion queue status |
| Memory Sessions | `/Memory` | Episodic chat memory management |
| Vector Store | `/VectorStore` | Embedding health + orphan detection |
| Provider Config | `/ProviderConfig` | API key management (OpenAI/Gemini/Claude) |

---

## API Endpoints

Base URL: `https://localhost:5000`

| Method | Endpoint | Description |
|--------|----------|-------------|
| **RAG** | | |
| POST | `/api/rag/run` | Execute RAG query (provider, model, promptStyle) |
| **Documents** | | |
| POST | `/api/documents/ingest` | Upload & ingest document (dual-path: SQL + pgvector) |
| **Retrieval** | | |
| POST | `/api/retrieval/test` | Vector similarity search |
| **Evaluation** | | |
| GET | `/api/evaluation/history` | Paginated evaluation results |
| POST | `/api/evaluation/evaluate` | On-demand LLM-as-judge scoring |
| GET | `/api/evaluation/stats` | Aggregate quality statistics + trends |
| **Analytics** | | |
| GET | `/api/analytics/average-latencies` | Latency by provider/model |
| GET | `/api/analytics/provider` | Provider performance analytics |
| GET | `/api/analytics/sla-compliance` | SLA compliance by prompt style |
| GET | `/api/analytics/prompt-style-usage` | Prompt style distribution |
| GET | `/api/analytics/trends` | Daily latency trends |
| **Feedback** | | |
| POST | `/api/feedback` | Submit quality rating |
| GET | `/api/feedback` | List all feedback |
| GET | `/api/feedback/average` | Average rating |
| **Chat** | | |
| POST | `/api/chat/query` | Contextual chat with memory |
| GET | `/api/chat/context/{sessionId}` | Session context |
| **Intelligence** | | |
| GET | `/api/intelligence/dashboard` | Unified intelligence dashboard |
| GET | `/api/intelligence/export` | Export analytics data |
| GET | `/api/intelligence/insights` | AI-generated insights |
| POST | `/api/intelligence/chat` | Intelligence chat |
| POST | `/api/intelligence/related` | Related insights |
| POST | `/api/intelligence/action` | Execute AI action |
| **Insights** | | |
| POST | `/api/insights/analyze` | AI analysis |
| POST | `/api/insights/anomalies` | Anomaly detection |
| POST | `/api/insights/predict` | Predictive modeling |
| POST | `/api/insights/report` | Narrative report generation |
| **Agent** | | |
| POST | `/api/agent/run` | Execute autonomous agent plan |

---

## Key Technologies

- **.NET 9** — Web API + MVC
- **Entity Framework Core 9** — SQL Server + PostgreSQL (dual-context)
- **pgvector** — Vector similarity search for RAG
- **OpenAI GPT-4o-mini** — LLM provider + embeddings + evaluation judge
- **Google Gemini / Anthropic Claude** — Multi-provider LLM support
- **Chart.js** — Admin dashboard visualizations
- **AdminLTE 4** — Admin panel UI framework
- **xUnit + Moq** — Unit testing (66 tests)

---

## Testing

```bash
dotnet test ArNir.Tests/ArNir.Tests.csproj --verbosity normal
```

**66 tests** across 6 sprints covering controllers, services, and edge cases.

---

## Sprint History

| Sprint | Focus | Tests |
|--------|-------|-------|
| 1 | PostgreSQL bridge, authentication, file validation | 12 |
| 2 | Ops visibility, background ingestion, vector store health | 5 |
| 3 | Embeddings mgmt, memory panel, agent trigger, job monitor | 19 |
| 4 | Feedback loop, template import/export, notification center | 15 |
| 5 | API production parity, security, UI polish | 5 |
| 6 | LLM-as-judge evaluation layer (quality scoring) | 10 |
| **Total** | | **66** |

---

## License

Proprietary — Empirical Edge Consulting
