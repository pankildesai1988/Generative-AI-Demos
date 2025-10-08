# Phase 3.3 – Retrieval Service Documentation

## 🔹 Overview
The Retrieval Service is responsible for fetching relevant document chunks to support **Retrieval-Augmented Generation (RAG)**. It combines **semantic search (vector similarity)** with **keyword-based search (SQL Server Full-Text Search)** to provide robust and performant retrieval.

Admin Test UI is implemented using **direct business service DI calls** (no API endpoints).

---

## 🔹 Project Structure (Updated)
```
/AirNir
├── Library
│   ├── ArNir.Core       → Entities, DTOs, Config, Validations
│   ├── ArNir.Data       → DbContexts (SQL Server + Postgres), EF Migrations
│   └── ArNir.Services   → Business logic Service, Interface, Helper, Mapping (EmbeddingService, RetrievalService)
├── Presentation
│   ├── ArNir.Admin      → AdminLTE UI Controllers, ViewModel, Views (embedding + retrieval test pages)
│   └── ArNir.Frontend   → End-user search/chat (future, Phase 3.4+)
```

---

## 🔹 Features

### 1. Semantic Search (pgvector)
- Uses embeddings stored in **Postgres with pgvector**.
- Similarity computed using `<=>` operator.
- Returns top-K semantically similar chunks.

### 2. Keyword Search (SQL Server FTS)
- Uses **SQL Server Full-Text Search** (`CONTAINS` + `FORMSOF(INFLECTIONAL, ...)`).
- Query is **tokenized and sanitized** to prevent SQL errors with multi-word and special characters.
- Supports stemming/inflection (e.g., *run* → *running*, *ran*).

### 3. Hybrid Search
- Merges results from semantic + keyword searches.
- Weighted scoring: **70% semantic + 30% keyword**.
- **Hybrid fallback**: if no keyword results, defaults to semantic-only.

### 4. SLA Monitoring & Debugging
- Latency logs: Embedding, Semantic, Chunk Fetch, Keyword, Total.
- SLA: **< 300ms** for Top-10 retrieval on ~10k chunks.
- Admin Debug UI:
  - Side-by-side Semantic vs Hybrid results.
  - Source tagging: 🔎 Semantic | 📑 Keyword | ⚡ Hybrid.
  - Filter dropdown (All, Semantic, Keyword, Hybrid).
  - Summary counter per result type.
  - Latency badges: ✅ OK ≤ 300ms, ⚠️ Slow > 300ms.
  - Background highlight for slow queries.

---

## 🔹 Technical Details

### Service Interface
```csharp
public interface IRetrievalService
{
    Task<List<ChunkResultDto>> SearchAsync(string query, int topK = 5, bool useHybrid = false);
}
```

### RetrievalService Implementation Highlights
- **Postgres** → semantic vector search.
- **SQL Server** → keyword FTS query.
- **Merge logic** → hybrid results with weighted scores.
- **Fallback mode** → hybrid falls back to semantic-only if keyword = 0.

### DTO Extension
```csharp
public class ChunkResultDto
{
    public Guid ChunkId { get; set; }
    public Guid DocumentId { get; set; }
    public string Text { get; set; }
    public double Score { get; set; }
    public Dictionary<string, string> Metadata { get; set; } = new();
    public string Source { get; set; } = "Semantic"; // Semantic | Keyword | Hybrid
}
```

---

## 🔹 Admin Debug UI
- Built with **AdminLTE**.
- Uses **direct DI calls to RetrievalService** (no API).
- Features:
  - Query input (text + topK).
  - Tables for Semantic + Hybrid results.
  - Source badges + color coding.
  - Dropdown filter & summary counter.
  - SLA badges + red highlights for slow results.

---

## 🔹 Test Checklist (Phase 3.3)
1. Upload DOCX/PDF/TXT → verify clean chunking.
2. Delete document → embeddings removed.
3. Run **semantic-only** query.
4. Run **keyword-only** query.
5. Run **hybrid** query (both semantic + keyword hits).
6. Run hybrid query with **no keyword hits** → fallback works.
7. Validate SLA logging & UI highlights.
8. Edge cases: stop words, special characters, empty DB.

---

## 🔹 Outcome
- Retrieval Service is **production-ready**.
- Provides **robust hybrid search**.
- Integrated with Admin Panel for debugging.
- Meets **SLA < 300ms** on ~10k chunks.
- Ready for Phase 3.4 – RAG Pipeline Integration.

