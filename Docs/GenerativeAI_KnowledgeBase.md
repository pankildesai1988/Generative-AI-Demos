# Generative AI .NET Project – Knowledge Base

---

## Phase 1 – Foundation
- Explored use cases in .NET apps (chatbots, Q&A bots, summarization, content generation, code assist).
- Tested HuggingFace models for API demo + Azure deployment.
- Learned Prompt Engineering: zero-shot, few-shot, role prompting.
- Findings: Few-shot & role prompting gave better results; zero-shot was verbose/unreliable.

---

## Phase 2.1 – .NET Backend & Frontend Integration

✅ **Backend**
- .NET Core Web API with `ChatController` endpoints (send, stream, history, sessions, duplicate-session, delete).
- Services layer with `IOpenAiService` + `IChatHistoryService`.
- Persistence in SQL Server with `ChatSessions` + `ChatMessages`.

✅ **Frontend**
- Modularized JS files: `chat.js`, `sessions.js`, `templates.js`, `utils.js`, `main.js`.
- Features: streaming, typing dots, session sidebar, model selector, prompt preview.

✅ **Deployment**
- Azure App Service + SQL Azure.
- Fixed CORS + connection strings in App Config.

---

## Phase 2.2 – Prompt Templates + Clean UI
- Templates stored in DB with parameters (tone, length).
- `buildPrompt()` inserts parameters into templates.
- `buildPromptPreview()` updates preview instantly.
- Admin panel for template CRUD (planned in Phase 2.3).

---

## Phase 2.2 – Bug Fixes & Enhancements
- Fixed `sessionId undefined` bug.
- Fixed circular imports between `chat.js` and `sessions.js`.
- Streaming shows typing dots before assistant reply.
- Cloning sessions now copies both user + assistant messages.
- User messages persisted before OpenAI call.

---

## Phase 2.3 – Admin Panel for Prompt Templates

✅ **AdminLTE Integration**
- Added `/Admin` area in ASP.NET Core MVC.
- Integrated AdminLTE v4 theme for consistent UI.
- Sidebar navigation (Templates, Users [future], Analytics [future]).

✅ **Authentication**
- Implemented custom authentication with backend-issued JWT.
- Frontend (Admin) stores JWT in cookies and sends with requests.
- Added Login/Logout flow.

✅ **CRUD for Templates**
- Templates managed via `/Admin/Templates`.
- Fields: `Name`, `KeyName`, `TemplateText`.
- Data served entirely from backend API (`/api/PromptTemplate`).

✅ **Parameterized Prompts**
- Dynamic parameter editor in Create/Edit pages.
- Supports `text`, `number`, `boolean`, `select`, `multiselect`.
- Parameters include: `Name`, `KeyName`, `Type`, `Options`, `DefaultValue`, `IsRequired`, `RegexPattern`.

✅ **Live Preview**
- Preview updates instantly when editing TemplateText or parameters.
- Inline highlighting of invalid values (red, tooltip with error).
- Error list below preview for full context.
- Admin can disable validation (persistent toggle, with reset button).

✅ **Versioning**
- Template versions saved on update.
- Version History modal lists all versions with rollback + compare.
- Rollback restores older version to active.

---

## Phase 2.4 – Session Cloning & Cross-Model Comparisons

### Backend
- Introduced **ComparisonService** (`IComparisonService`) for cross-model testing.
- Added **LLMProviders folder** with pluggable providers (OpenAI, Gemini, Claude).
- Built **RunComparisonAsync** to handle multiple models per input.
- Implemented **error handling + persistence** (`ComparisonResults`, `SessionComparisons`).
- Added `GetHistoryAsync` and `GetHistoryByIdAsync` for fetching comparisons.
- Persisted **error codes/messages** in DB.
- Fixed **duplicate results bug** (Distinct + one add per provider:model).

### Frontend (AdminLTE)
- Added `/Admin/Comparison` page:
  - Model & provider dropdown.
  - Input field for test prompt.
  - Run button → calls backend.
  - **Side-by-side comparison grid (columns)** for providers/models.
  - **Loading spinner** while waiting.
  - Auto-scroll to results.
  - Deduplication filter in JS.

- Added `/Admin/Comparison/History` page:
  - DataTables summary view of past comparisons.
  - Row click → details modal (provider/model responses).
  - Deduplication applied to modal + table.

### Improvements
- Fixed **login DI bug** (`IAdminAuthService`).
- Clean separation of business logic into `ComparisonService`.
- Reusable DTOs: `ComparisonResultDto`, `ComparisonHistoryDto`.
- Project stable for **OpenAI + Gemini** testing.
- Ready to extend with Claude once API key available.

---

## Updated Project Structure (Phase 2.4)

```
/2_OpenAIChatDemo
 ├── 2_OpenAIChatDemo (Backend)
 │   ├── Controllers (AuthController.cs, ChatController.cs, PromptController.cs, ComparisonController.cs)
 │   ├── Data (ChatDbContext.cs)
 │   ├── DTOs (ChatRequestDto, ChatResponseDto, ChatSessionDto, ChatMessageDto, ComparisonRequestDto, ComparisonResultDto, ComparisonHistoryDto)
 │   ├── Models (ChatSession, ChatMessage, ComparisonResult, SessionComparison, AdminUser, PromptTemplate)
 │   ├── Services (IOpenAiService, OpenAiService, IChatHistoryService, ChatHistoryService, IPromptTemplateService, PromptTemplateService, IComparisonService, ComparisonService)
 │   ├── LLMProviders (OpenAiProvider.cs, GeminiProvider.cs, ClaudeProvider.cs)
 │   ├── Program.cs, appsettings.Development.json
 │
 ├── 2_OpenAIChatFrontEnd (Frontend)
 │   ├── Areas/Admin/Controllers (TemplateController.cs, ComparisonController.cs)
 │   ├── Areas/Admin/Views/Template (Index.cshtml, Create.cshtml, Edit.cshtml)
 │   ├── Areas/Admin/Views/Comparison (Index.cshtml, History.cshtml)
 │   ├── wwwroot/admin/js (template-admin.js, comparison.js, comparison-history.js)
 │   ├── Views/Home/Index.cshtml, Views/Shared/_Layout.cshtml
 │
 ├── sql (create_tables.sql, update_comparison_results.sql)
```

---

## Next Steps – Phase 2.5
- Real-time Streaming (ChatGPT-style, SignalR/SSE)
- Live word-by-word updates in UI
- Improve performance & responsiveness

# Phase 3 – Retrieval-Augmented Generation (RAG)

## 🔹 RAG Architecture

1. **Ingestion Layer**
   - Document upload, parsing, and chunking (PDF, DOCX, Markdown, SQL text).
   - Clean text storage + chunking by semantic boundaries.

2. **Embedding & Storage Layer**
   - Generate vector embeddings (OpenAI or HuggingFace).
   - Store in **Postgres + pgvector** or **Azure Cognitive Search**.
   - Maintain metadata in SQL (docId, chunkId, tags, owner, version).

3. **Retrieval Layer**
   - Semantic + hybrid search over embeddings.
   - Ranking, deduplication, filters.

4. **Augmentation + Generation Layer**
   - Retrieved chunks + user query → LLM.
   - Compare **baseline LLM vs RAG-enhanced** responses.
   - Debug mode: show retrieved chunks in Admin Panel.

---

## 🔄 Phase 3 Sub-Phases

## 📂 Repository Structure

AirNir/
│
├── docs/ # Documentation & Knowledge Base
│   ├── AirNir_KnowledgeBase_Phase1-3.1.docx
│   ├── AirNir_Phase3_Documentation.docx
│   ├── Phase3_README.md
│   ├── Phase3.1_Architecture.png
│   └── Updated_GenerativeAI_Learning_Tracker.xlsx
│
├── Library/ # Core Libraries
│   ├── ArNir.Core     # Entities, DTOs, Config
│   ├── ArNir.Data     # DbContext, EF Migrations
│   └── ArNir.Service  # Business logic (DocumentService, etc.)
│
├── Presentation/ # MVC Applications
│   ├── ArNir.Admin    # AdminLTE-based Admin Panel
│   ├── ArNir.Frontend # User-facing MVC app
│   └── ArNir.WebAPI   # API Layer (future React/Angular integration)
│
├── sql/ # SQL Scripts
│   ├── create_tables.sql
│   └── update_documents_chunks.sql
│
├── .gitignore
├── README.md
└── LICENSE


### Phase 3.1 – Document Ingestion & Chunking ✅ (Completed)
- Admin panel document upload (PDF, DOCX, TXT, Markdown).
- Parsing & validation with configurable AllowedTypes + MaxFileSize.
- Store text + chunks in SQL, original files in wwwroot/uploads.
- DTO-based service layer, business logic in DocumentService.
- AdminLTE UI: Upload, Edit (re-upload new file), Delete (with confirm modal), Details (preview).
- Preview: PDF inline, TXT as chunks, DOCX fallback download.
- Outcome: Robust ingestion pipeline, documents ready for embeddings.

### Phase 3.2 – Embeddings & Vector Storage
- Generate embeddings.
- Store in pgvector or Azure Cognitive Search.
- Link embeddings with metadata.

### Phase 3.3 – Retrieval Service
- Build `IRetrievalService`.
- API endpoint `/api/retrieval/search`.
- Top-k semantic search results.
- Admin Panel: show retrieval debug info.

### Phase 3.4 – RAG Pipeline Integration
- Build `IRagService`.
- Input: user query → retrieval → augmented prompt → LLM.
- Compare baseline vs RAG outputs.

### Phase 3.5 – Admin Panel Enhancements
- Add **Documents Page** (upload, list, delete, version).
- Add **RAG Comparison Page** (baseline vs RAG, chunk debug view).

### Phase 3.6 – Deployment & Optimization
- Deploy vector DB (Azure or pgvector).
- Optimize retrieval (indexes, caching).
- Add monitoring (queries/sec, storage growth).

---

![Updated Architecture – Phase 3.1](docs/Phase3_RAG_Architecture.png)

## ⚡ Expected Outcomes
- RAG-enabled chatbot in .NET + AdminLTE project.
- Admin panel for document + RAG debugging.
- Comparison: baseline vs RAG-enhanced responses.
- Scalable + production-ready retrieval pipeline.
