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
✅ Document ingestion & chunking pipeline (SQL Server)  
✅ Embedding generation + vector storage with **pgvector (Postgres)**  
✅ Admin test UI for embeddings + similarity search  

---

## 📂 Repository Structure

generative-ai-mentor/  
│
├── docs/ # Documentation & Knowledge Base  
│ ├── GenerativeAI_KnowledgeBase.md  
│ ├── GenerativeAI_KnowledgeBase.docx  
│ ├── Updated_GenerativeAI_Learning_Tracker.xlsx  
│ └── Phase3_RAG_Architecture.png  # Updated Phase 3.2 architecture diagram  
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
│ ├── create_tables.sql  
│ ├── update_documents_chunks.sql  
│ └── update_embeddings.sql  
│
├── .gitignore  
├── README.md  
└── LICENSE  

---

## 📂 ArNir Project Structure (RAG Implementation)

AirNir/  
│
├── Library/  
│ ├── ArNir.Core       → Entities, DTOs, Config  
│ ├── ArNir.Data       → DbContexts (SQL Server + Postgres), EF Migrations  
│ └── ArNir.Service    → Business logic (EmbeddingService, RetrievalService, RagService)  
│
├── Presentation/  
│ ├── ArNir.Admin      → AdminLTE (document upload, debug embeddings, retrieval UI, RAG comparison)  
│ └── ArNir.Frontend   → End-user search/chat interface  
│
├── sql/  
│ ├── create_tables.sql              → SQL Server (Documents, DocumentChunks)  
│ ├── update_documents_chunks.sql    → SQL Server schema updates  
│ └── update_embeddings.sql          → Postgres embeddings schema (pgvector)  
│
└── docs/  
   └── Phase3_RAG_Architecture.png   → RAG architecture diagram  

---

## 🧑‍💻 Setup & Run Locally

### 1. Clone Repo

```bash
git clone https://github.com/<your-username>/generative-ai-mentor.git
cd generative-ai-mentor/src/backend
```

### 2. Configure

* Add your **OpenAI/Gemini/Claude API keys** to `appsettings.Development.json` or via **User Secrets**.  
* Update SQL Server + Postgres (pgvector) connection strings.  

### 3. Run Backend

```bash
dotnet run
```

### 4. Run Frontend

* Open `https://localhost:7151/Admin`

---

# 📖 Learning Tracker

**Progress is documented in:**

* **docs/GenerativeAI_KnowledgeBase.md**  
* **docs/Updated_GenerativeAI_Learning_Tracker.xlsx**  
* **docs/Phase3_RAG_Architecture.png** (latest architecture diagram after Phase 3.2)

---

## 🎯 Roadmap

**✅ Completed**

* Phase 1: Foundation  
* Phase 2.1: Backend + Frontend Integration  
* Phase 2.2: Prompt Templates + Clean UI  
* Phase 2.3: Template Management (Admin Panel, Versioning, Advanced Parameters, Live Preview)  
* Phase 2.4: Session Cloning & Cross-Model Comparisons  
* Phase 3.1: Document Ingestion & Chunking  
* Phase 3.2: Embeddings & Vector Storage (Postgres + pgvector, EF Core integration, Admin test UI)  

**⏳ In Progress**

* Phase 3.3: Retrieval Service (semantic + hybrid search, Admin debug view)

**🛠 Planned**

* Phase 3.4: RAG Pipeline Integration (baseline vs RAG-enhanced)
* Phase 3.5: Admin Panel Enhancements (Docs page + RAG comparison page)
* Phase 3.6: Deployment & Optimization (Azure/Postgres, indexing, caching, monitoring)
* Phase 4: Enterprise Features (Security, Multi-tenancy, Analytics, Scaling)

---

## 🛠️ Tech Stack

**Backend:** .NET 9, ASP.NET Core Web API  
**Frontend:** ASP.NET Core MVC, HTML, Bootstrap, jQuery + Modular JS, AdminLTE  
**Database:** SQL Server (local & Azure SQL), Postgres (pgvector for embeddings)  
**AI Models:** OpenAI GPT-3.5, GPT-4o, Gemini, Claude (planned)  
**Cloud:** Azure App Service, Azure SQL, Azure App Config, Docker (Postgres + pgvector, pgAdmin)  

---

## 📌 License

**MIT License** – feel free to use and adapt!

---

# 👨‍🏫 Author - pankildesai1988

**Built as part of my Generative AI Mentor Journey 🧑‍💻**  
Learning → Building → Deploying → Scaling 🚀

