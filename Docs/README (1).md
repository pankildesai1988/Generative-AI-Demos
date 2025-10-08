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
│   ├── ArNir.Admin      → AdminLTE UI Controllers, ViewModel, Views (embedding + retrieval test pages, RAG comparison)
│   └── ArNir.Frontend   → End-user search/chat interface (future, Phase 3.4+)
│
├── sql
│   ├── create_tables.sql
│   └── update_documents_chunks.sql
│
└── docs
    └── Phase3_RAG_Architecture.png
```

---

## 📖 Phase 3 Progress

- **Phase 3.1 – Document Ingestion & Chunking** ✅ Completed  
- **Phase 3.2 – Embeddings & Vector Storage** ✅ Completed  
- **Phase 3.3 – Retrieval Service** ✅ Completed
- **Phase 3.4 – RAG Pipeline Integration** ✅ Completed  
![Phase 3.3 – Retrieval Service Architecture](docs/Phase3_RAG_Architecture.png)  

---

## 📍 Roadmap

- ✅ Phase 1: Foundation (use cases, HuggingFace models, Azure, prompt engineering)
- ✅ Phase 2: .NET Backend + Frontend Integration (Chat API, sessions, templates, admin panel, comparisons)
- ✅ Phase 3.1: Ingestion Layer (document upload, chunking, SQL Server)
- ✅ Phase 3.2: Embedding & Storage Layer (OpenAI embeddings, Postgres + pgvector)
- ✅ Phase 3.3: Retrieval Service (semantic + hybrid search, Admin debug view)
- ✅ Phase 3.4: Augmentation + Generation Layer (RAG pipeline integration with baseline vs RAG-enhanced answers, Admin Debug UI, history logging)
- ⏳ Phase 3.5: Admin Enhancements (RAG History Page – browse and analyze past runs)
- ⏳ Phase 3.6: End-user Chat UI with RAG backend

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
