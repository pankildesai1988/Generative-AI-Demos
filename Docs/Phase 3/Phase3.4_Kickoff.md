# Phase 3.4 Kickoff Plan – Retrieval-Augmented Generation (RAG)

This file contains the **full project history (up to Phase 3.3)** and the **kickoff plan for Phase 3.4**, so it can be used as context for a custom GPT.

---

## 🔹 Project Status Recap (Up to Phase 3.3)

✅ **Phase 1 – Foundation**  
- Explored Generative AI use cases in .NET (chatbots, summarization, code assist).  
- Learned prompt engineering (zero-shot, few-shot, role prompting).  

✅ **Phase 2 – Application Foundations**  
- **2.1 Backend + Frontend Integration**: Web API (`ChatController`), SQL persistence, modular frontend JS, Azure deployment.  
- **2.2 Prompt Templates + UI**: Template DB storage, live preview, parameterized prompts.  
- **2.3 Admin Panel**: AdminLTE integration, JWT authentication, CRUD for templates, versioning/rollback.  
- **2.4 Session Cloning & Cross-Model Comparison**: Side-by-side LLM provider outputs, persisted results, history view.  

✅ **Phase 3 – RAG Foundations**  
- **3.1 Document Ingestion & Chunking**: Upload (PDF, DOCX, TXT, Markdown), whitespace + special char cleaning, SQL Server storage, preview UI.  
- **3.2 Embeddings & Vector Storage**: Postgres + `pgvector`, EF Core, `EmbeddingService`, Admin UI embedding tests.  
- **3.3 Retrieval Service**:  
  - Implemented `IRetrievalService` (Semantic via pgvector, Keyword via SQL Server FTS, Hybrid).  
  - Hybrid fallback to semantic-only.  
  - Admin Debug UI (filters, counters, SLA monitoring, source tagging).  
  - SLA target: <300ms for Top-10 retrieval on ~10k chunks.  
  - **Important**: Admin UI uses **direct DI service calls**, not API endpoints.  

✅ **Updated Project Structure**  
```
/AirNir
├── Library
│   ├── ArNir.Core       → Entities, DTOs, Config, Validations
│   ├── ArNir.Data       → DbContexts (SQL Server + Postgres), EF Migrations
│   └── ArNir.Services   → Business logic Service, Interface, Helper, Mapping (EmbeddingService, RetrievalService, RagService)
├── Presentation
│   ├── ArNir.Admin      → AdminLTE UI Controllers, ViewModel, Views (embedding + retrieval test pages, RAG comparison)
│   └── ArNir.Frontend   → End-user search/chat (future, Phase 3.4+)
└── sql / docs           → Supporting schema + architecture diagrams
```

---

## 🔹 Phase 3.4 – RAG Pipeline Integration (Kickoff)

🎯 **Goal:** Integrate retrieval with LLM prompt augmentation to generate **context-aware answers** from uploaded documents.  

### 1. **Service Layer**
- Create `IRagService` + `RagService`.  
- Flow:  
  1. User query → RetrievalService (semantic/hybrid).  
  2. Retrieved chunks → formatted into context.  
  3. Augmented prompt → LLM provider (OpenAI GPT-4o baseline, extendable to Gemini/Claude).  
  4. Return **Baseline LLM answer vs RAG-enhanced answer**.  

### 2. **Prompt Engineering**
- Define RAG prompt template:  
  ```
  You are an AI assistant. Use the following context to answer the question.
  Context:
  {retrieved_chunks}
  Question:
  {user_query}
  ```
- Limit context to top-K chunks, token-aware trimming.  
- Add metadata (doc title, source) to improve grounding.  

### 3. **Admin Debug UI**
- Add **RAG Comparison Page**:  
  - Input: user query.  
  - Show side-by-side → Baseline LLM vs RAG-enhanced LLM.  
  - Collapsible “Retrieved Context” panel (debug mode).  
  - Latency + SLA tracking.  

### 4. **Frontend (Phase 3.4+)**
- ArNir.Frontend:  
  - Integrate Retrieval + RAG in end-user chat.  
  - Context-aware responses with fallback to baseline if retrieval = empty.  

### 5. **Testing Checklist**
- ✅ Query with document match → RAG must cite retrieved chunks.  
- ✅ Query with no match → fallback to baseline (no hallucination from empty context).  
- ✅ Compare accuracy between baseline vs RAG.  
- ✅ SLA: End-to-end latency < 1s (including retrieval + LLM call).  

---

## 🔹 Expected Outcomes
- A **working RAG pipeline** in .NET using your existing retrieval layer.  
- **Admin Debug UI** for baseline vs RAG comparison.  
- Ready-to-extend with **multi-LLM providers** for RAG.  
- Foundation for Phase 3.5 (Admin Enhancements) and Phase 3.6 (Deployment & Optimization).  

---

👉 Use this as the **kickoff context** for Phase 3.4 in your custom GPT.
