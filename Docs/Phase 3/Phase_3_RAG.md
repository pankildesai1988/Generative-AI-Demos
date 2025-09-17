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
   - Added **Admin test UI** (`/Embedding/Test`) → input text, generate embedding, run similarity search.
   - Verified vectors persisted in Postgres + semantic similarity queries working.

3. **Retrieval Layer (Next – Phase 3.3)**
   - Semantic + hybrid search over embeddings.
   - Ranking, deduplication, filters.
   - Join results with `DocumentChunks` from SQL for full context.

4. **Augmentation + Generation Layer**
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
- Added Admin UI test page for embeddings & semantic search.
- **Outcome:** Fully functional embedding pipeline (MS SQL + Postgres integration).

### Phase 3.3 – Retrieval Service (Next)
- Build `IRetrievalService`.
- API endpoint `/api/retrieval/search`.
- Top-k semantic search results.
- Admin Panel: show retrieval debug info (linked with chunks).

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

![Updated Architecture – Phase 3.2](docs/Phase3_RAG_Architecture.png)

## ⚡ Expected Outcomes
- RAG-enabled chatbot in .NET + AdminLTE project.
- Admin panel for document + RAG debugging.
- Comparison: baseline vs RAG-enhanced responses.
- Scalable + production-ready retrieval pipeline.

