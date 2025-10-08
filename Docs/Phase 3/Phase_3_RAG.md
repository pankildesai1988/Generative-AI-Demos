# Phase 3 – Retrieval-Augmented Generation (RAG)

## 🔹 RAG Architecture

1. **Ingestion Layer ✅ Completed (Phase 3.1)**
   - Document upload, parsing, and chunking (PDF, DOCX, Markdown, SQL text).
   - Clean text storage + chunking by semantic boundaries.
   - Stored documents + chunks in **MS SQL**.

2. **Embedding & Storage Layer ✅ Completed (Phase 3.2)**
   - Implemented **OpenAI-based embedding generation** (`text-embedding-ada-002`).
   - Added **Pgvector.EntityFrameworkCore** support for `vector(1536)` type.
   - Set up **Postgres (Docker + pgvector)** for vector storage.
   - Created `EmbeddingService` to fetch chunks from SQL → generate embeddings → store in Postgres.
   - Added **Admin test UI** (`/Embedding/Test`) → directly injected and called business services (no API endpoints).
   - Verified vectors persisted in Postgres + semantic similarity queries working.

3. **Retrieval Layer ✅ Completed (Phase 3.3)**
   - Implemented **`IRetrievalService` + `RetrievalService`**.
   - Supports **Semantic Search (pgvector)**, **Keyword Search (SQL Server FTS)**, and **Hybrid Search** (merge + re-rank).
   - **Hybrid Fallback** → if keyword search returns 0, retrieval falls back to semantic only.
   - Added **Admin Debug UI**:
     - Directly calls business services via DI (not API endpoints).
     - Semantic vs Hybrid results side by side.
     - Source tagging: 🔎 Semantic | 📑 Keyword | ⚡ Hybrid.
     - Filter dropdown (All, Semantic, Keyword, Hybrid).
     - Summary counter showing distribution of result types.
     - SLA monitoring: ✅ OK if ≤ 300ms, ⚠️ Slow if > 300ms.
     - Slow queries auto-highlight (red badge + light red background).
   - Query sanitization for SQL Server FTS (tokenized, handles multi-word + special chars).
   - Verified retrieval latency: **Top-10 < 300ms on ~10k chunks** (meets SLA).

4. **Augmentation + Generation Layer (Next – Phase 3.4)**
   - Retrieved chunks + user query → LLM.
   - Compare **baseline LLM vs RAG-enhanced** responses.
   - Debug mode: show retrieved chunks in Admin Panel.

---

## 🔄 Phase 3 Sub-Phases

### Phase 3.1 – Document Ingestion & Chunking ✅ Completed
- Admin panel document upload (PDF, DOCX, TXT, Markdown).
- Parsing & validation with configurable AllowedTypes + MaxFileSize.
- Store text + chunks in SQL, original files in wwwroot/uploads.
- DTO-based service layer, business logic in DocumentService.
- AdminLTE UI: Upload, Edit (re-upload new file), Delete (with confirm modal), Details (preview).
- Preview: PDF inline, TXT as chunks, DOCX fallback download.
- **Outcome:** Robust ingestion pipeline, documents ready for embeddings.

### Phase 3.2 – Embeddings & Vector Storage ✅ Completed
- Installed **pgvector extension** (Docker-based Postgres).
- Added `VectorDbContext` + `Embedding` entity with `Pgvector.Vector`.
- EF migrations → `embeddings` table (`vector(1536)`).
- Implemented `EmbeddingService` for chunk embeddings.
- Verified insert + similarity queries in Postgres.
- Added Admin UI test page for embeddings & semantic search (direct service DI calls, no API).
- **Outcome:** Fully functional embedding pipeline (MS SQL + Postgres integration).

### Phase 3.3 – Retrieval Service ✅ Completed
- Implemented `RetrievalService` (Semantic, Keyword, Hybrid).
- Hybrid fallback to semantic-only if no keyword hits.
- Added Admin Debug UI for side-by-side retrieval comparison (direct service calls, no API).
- Added filtering, source tagging, summary counters, SLA checks, and query sanitization.
- **Outcome:** Transparent, debuggable, production-ready retrieval service.

### Phase 3.4 – RAG Pipeline Integration (Next)
- ✅ Backend: `IRagService` + `RagService` with baseline vs RAG comparison  
- ✅ DTOs for structured results (`RagResultDto`, `RagChunkDto`)  
- ✅ History logging in SQL Server (`RagComparisonHistories`)  
- ✅ Admin Debug UI with:
   - Query input
   - Side-by-side answers
   - Retrieved context panel
   - Latency metrics + SLA badge
   - Processing spinner  
- ✅ Clean separation of SQL Server vs Postgres migrations

### Phase 3.5 – Admin Panel Enhancements
- Add **Documents Page** (upload, list, delete, version).
- Add **RAG Comparison Page** (baseline vs RAG, chunk debug view).

### Phase 3.6 – Deployment & Optimization
- Deploy vector DB (Azure or pgvector).
- Optimize retrieval (indexes, caching).
- Add monitoring (queries/sec, storage growth).

---

![Updated Architecture – Phase 3.3](docs/Phase3_RAG_Architecture.png)

## ⚡ Expected Outcomes
- RAG-enabled chatbot in .NET + AdminLTE project.
- Admin panel for document + RAG debugging.
- Comparison: baseline vs RAG-enhanced responses.
- Scalable + production-ready retrieval pipeline.

