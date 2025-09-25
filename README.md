# 🚀 Generative AI Mentor Project

This repository documents my **journey to mastering Generative AI** with a strong focus on **.NET applications, OpenAI, and Azure deployment**.

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
✅ Retrieval Service with **Semantic, Keyword & Hybrid Search** + Admin Debug UI (filter, counters, SLA monitoring, fallback)  
✅ RAG pipeline with baseline vs RAG-enhanced answers  
✅ DTOs for structured outputs (`RagResultDto`, `RagChunkDto`)  
✅ Admin Debug UI (RAG Comparison Page with side-by-side answers, context, latency, SLA badge, spinner)  
✅ History logging into SQL Server (`RagComparisonHistories`)  
✅ **RAG History Page** with filters, compare mode, CSV/Excel export  
✅ **Docs Module** for document lifecycle (Upload/Edit/Delete/Rebuild embeddings)  
✅ **Analytics Dashboard** (SLA Compliance, Latencies, PromptStyle Distribution, Trends) with filters + drill-down to history  

---

## 📂 AirNir Project Structure (RAG Implementation)

```
/AirNir
├── Library
│   ├── ArNir.Core       → Entities, DTOs, Config, Validations
│   ├── ArNir.Data       → DbContexts (SQL Server + Postgres), EF Migrations
│   └── ArNir.Services   → Business logic Service, Interface, Helper, Mapping (EmbeddingService, RetrievalService, RagService)
│
├── Presentation
│   ├── ArNir.Admin      → AdminLTE UI Controllers, ViewModel, Views (embedding + retrieval test pages, RAG comparison, history, analytics)
│   └── ArNir.Frontend   → End-user search/chat interface (future, Phase 3.7+)
│
├── sql
│   ├── create_tables.sql
│   ├── update_documents_chunks.sql
│   ├── update_embeddings.sql
│   └── update_rag_history.sql
│
└── docs
    ├── Phase3_RAG_Architecture.png
    ├── Phase3.3_Architecture.png
    ├── Phase3.4_Architecture.png
    ├── Phase3.5_Architecture.png
    ├── Phase3.6_Architecture.png
    └── GenerativeAI_KnowledgeBase.md
```

---

## 📖 Phase 3 Progress

- **Phase 3.1 – Document Ingestion & Chunking** ✅ Completed  
- **Phase 3.2 – Embeddings & Vector Storage** ✅ Completed  
- **Phase 3.3 – Retrieval Service** ✅ Completed  
- **Phase 3.4 – Prompt Engineering & RAG Pipeline** ✅ Completed  
- **Phase 3.5 – RAG History Enhancements & Docs Module** ✅ Completed  
- **Phase 3.6 – Analytics & Insights Dashboard** ✅ Completed  

![Phase 3.6 – Analytics Architecture](docs/Phase3.6_Architecture.png)  

---

## 📍 Roadmap

- ✅ Phase 1: Foundation (use cases, HuggingFace models, Azure, prompt engineering)
- ✅ Phase 2: .NET Backend + Frontend Integration (Chat API, sessions, templates, admin panel, comparisons)
- ✅ Phase 3.1: Ingestion Layer (document upload, chunking, SQL Server)
- ✅ Phase 3.2: Embedding & Storage Layer (OpenAI embeddings, Postgres + pgvector)
- ✅ Phase 3.3: Retrieval Service (semantic + hybrid search, Admin debug view)
- ✅ Phase 3.4: Augmentation + Generation Layer (RAG pipeline integration with baseline vs RAG-enhanced answers, Admin Debug UI, history logging)
- ✅ Phase 3.5: Admin Enhancements (History Page, Docs Module)
- ✅ Phase 3.6: Analytics & Insights (Dashboard, Filters, Drill-down)
- ⏳ Phase 3.7: Advanced Analytics (Provider/Model analytics, exports, KPI widgets)

---

## 🛠️ Tech Stack

- **Backend:** .NET 9, ASP.NET Core Web API  
- **Frontend:** ASP.NET Core MVC, HTML, Bootstrap, jQuery + Modular JS, AdminLTE  
- **Database:** SQL Server, Postgres (pgvector)  
- **AI Models:** OpenAI GPT-3.5, GPT-4o, Gemini, Claude (planned)  
- **Cloud:** Azure App Service, Azure SQL, Docker (Postgres + pgvector)  

---

## 📌 License

MIT License – feel free to use and adapt!

---

👨‍🏫 **Author – pankildesai1988**  
Learning → Building → Deploying → Scaling 🚀