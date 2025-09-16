# 🚀 Generative AI Mentor Project

This repository documents my **journey to mastering Generative AI** with a strong focus on **.NET applications, OpenAI, and Azure deployment**.
It contains **source code, SQL scripts, documentation, and learning trackers**.

---

## 📌 Features (So Far)

✅ .NET Core Web API backend
✅ Frontend (Bootstrap + Modular JS)
✅ Chat with OpenAI (GPT-3.5, GPT-4o)
✅ Streaming responses with typing animation
✅ Persistent chat history in SQL Server
✅ Multi-session management (create, load, clone, delete)
✅ Prompt templates with parameters (tone, length, style)
✅ Deployed to Azure App Service + Azure SQL
✅ Modularized frontend (`chat.js`, `sessions.js`, `templates.js`, `utils.js`, `main.js`)
✅ AdminLTE Panel for managing prompt templates
✅ Versioning & rollback for templates
✅ Cross-model comparison (OpenAI, Gemini, Claude-ready)
✅ Comparison results persisted in SQL Server
✅ AdminLTE pages for running comparisons + viewing history
✅ Side-by-side grid view for provider/model outputs
✅ Deduplication + error persistence (ErrorCode, ErrorMessage)
✅ JWT-secured Admin area for testing

---

## 📂 Repository Structure

generative-ai-mentor/
│
├── docs/ # Documentation & Knowledge Base
│ ├── GenerativeAI\_KnowledgeBase.md
│ ├── GenerativeAI\_KnowledgeBase.docx
│ ├── Updated\_GenerativeAI\_Learning\_Tracker.xlsx
│ └── architecture-diagram.png # (optional)
│
├── src/ # Source Code
│ ├── backend/ # .NET Backend
│ │ ├── Controllers/
│ │ ├── Data/
│ │ ├── DTOs/
│ │ ├── Models/
│ │ ├── Services/
│ │ ├── LLMProviders/
│ │ └── Program.cs
│ │
│ ├── frontend/ # Frontend
│ │ ├── Areas/Admin/Views/Comparison (Index, History)
│ │ ├── wwwroot/admin/js/comparison.js
│ │ ├── wwwroot/admin/js/comparison-history.js
│ │ └── Views/Home/Index.cshtml
│ │
│ └── sql/ # SQL Scripts
│ ├── create\_tables.sql
│ └── update\_comparison\_results.sql
│
├── .gitignore
├── README.md
└── LICENSE

---

## 🧑‍💻 Setup & Run Locally

### 1. Clone Repo

```bash
git clone https://github.com/<your-username>/generative-ai-mentor.git
cd generative-ai-mentor/src/backend
```

### 2. Configure

* Add your **OpenAI/Gemini/Claude API keys** to `appsettings.Development.json`
* Update SQL connection string

### 3. Run Backend

```bash
dotnet run
```

### 4. Run Frontend

* Open `https://localhost:7151/Admin`

---

# 📖 Learning Tracker

**Progress is documented in:**

* **docs/GenerativeAI\_KnowledgeBase.md**
* **docs/Updated\_GenerativeAI\_Learning\_Tracker.xlsx**

---

## 🎯 Roadmap

**✅ Completed**

* Phase 1: Foundation
* Phase 2.1: Backend + Frontend Integration
* Phase 2.2: Prompt Templates + Clean UI
* Phase 2.3: Template Management (Admin Panel, Versioning, Advanced Parameters, Live Preview)
* Phase 2.4: Session Cloning & Cross-Model Comparisons

**⏳ On hold**

* Phase 2.5: Real-time Streaming (ChatGPT-style, SignalR/SSE)

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

## 🚀 Phase 3 – Retrieval-Augmented Generation (RAG)

### 🔹 RAG Architecture
1. **Ingestion Layer ✅ Completed** 
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

### 🔄 Phase 3 Sub-Phases
- **3.1 Document Ingestion & Chunking ✅ Completed** → Upload docs, parse & chunk text, save in SQL.
- **3.2 Embeddings & Vector Storage** → Generate embeddings, store in pgvector/Azure Search.
- **3.3 Retrieval Service** → Build IRetrievalService, API `/api/retrieval/search`, show debug in Admin.
- **3.4 RAG Pipeline Integration** → Build IRagService, augment queries with retrieved chunks.
- **3.5 Admin Panel Enhancements** → Docs page + RAG comparison page.
- **3.6 Deployment & Optimization** → Deploy vector DB, optimize, add monitoring.

### ⚡ Expected Outcomes
- RAG-enabled chatbot integrated in .NET + AdminLTE project.
- Admin panel for document management + RAG debugging.
- Comparison between **baseline vs RAG-enhanced responses**.
- Scalable + production-ready retrieval pipeline.

* Phase 4: Enterprise Features (Security, Multi-tenancy, Analytics, Scaling)

---

## 🛠️ Tech Stack

**Backend:** .NET 9, ASP.NET Core Web API
**Frontend:** ASP.NET Core MVC, HTML, Bootstrap, jQuery + Modular JS, AdminLTE
**Database:** SQL Server (local & Azure SQL)
**AI Models:** OpenAI GPT-3.5, GPT-4o, Gemini, Claude (planned)
**Cloud:** Azure App Service, Azure SQL, Azure App Config

---

## 📌 License

**MIT License** – feel free to use and adapt!

---

# 👨‍🏫 Author - pankildesai1988

**Built as part of my Generative AI Mentor Journey 🧑‍💻**
Learning → Building → Deploying → Scaling 🚀
