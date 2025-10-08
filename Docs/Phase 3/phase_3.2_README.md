# 📘 Phase 3.2 – Embeddings & Vector Storage

This phase introduces **OpenAI embeddings** for document chunks and stores them in **Postgres with pgvector** to enable semantic similarity search.

---

## 🎯 Objectives
- Generate embeddings for document chunks using **OpenAI**.
- Store embeddings in **Postgres** with the `pgvector` extension.
- Provide an **Admin UI** to test embedding generation + similarity search.
- Cross-referenced in:
  - Main project [README.md](../README.md)
  - Knowledge Base [GenerativeAI_KnowledgeBase.md](../docs/GenerativeAI_KnowledgeBase.md)
  - Detailed [Phase3.2_Documentation.docx](../docs/Phase3.2_Documentation.docx)

---

## 📂 Project Structure
```
/AirNir
├── Library
│   ├── ArNir.Core       → Entities, DTOs, Config
│   ├── ArNir.Data       → DbContexts (SQL Server + Postgres), EF Migrations
│   └── ArNir.Service    → Business logic (EmbeddingService, RetrievalService)
├── Presentation
│   ├── ArNir.Admin      → AdminLTE UI (embedding test page)
│   └── ArNir.Frontend   → End-user search/chat (future)
├── sql
│   ├── create_tables.sql              → SQL Server (Documents, Chunks)
│   ├── update_documents_chunks.sql    → SQL Server updates
│   └── update_embeddings.sql          → Postgres schema for embeddings (pgvector)
└── docs
    ├── Phase3.2_Architecture.png      → Embedding & Vector Storage Architecture
    ├── Phase3.2_Documentation.docx    → Detailed documentation
    └── Phase3.2_README.md             → Summary README
```

---

## ⚙️ Implementation
- **Postgres + pgvector Setup**
  - Dockerized Postgres with `ankane/pgvector` image.
  - Enabled extension: `CREATE EXTENSION vector;`.

- **DbContexts**
  - `ArNirDbContext` → SQL Server (Documents, Chunks).
  - `VectorDbContext` → Postgres (Embeddings).

- **Embedding Entity**
  ```csharp
  public class Embedding
  {
      public Guid EmbeddingId { get; set; }
      public Guid ChunkId { get; set; }
      public Vector Vector { get; set; }
      public string Model { get; set; } = "text-embedding-ada-002";
      public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
  }
  ```

- **EmbeddingService**
  - Fetches chunks from SQL Server.
  - Calls OpenAI API for embeddings.
  - Saves embeddings into Postgres.

- **Admin Test UI**
  - `/Embedding/Test` page.
  - Input text → Generate embedding → Run similarity search.
  - Shows top-k matches with metadata.

---

## ✅ Outcome
- Documents + chunks remain in **SQL Server**.
- Embeddings stored in **Postgres with pgvector**.
- Admin UI validates end-to-end embedding + similarity search.
- Ready for **Phase 3.3 – Retrieval Service**.

---

## 📖 Next Step
🔜 **Phase 3.3 – Retrieval Service**  
- Implement `IRetrievalService`.  
- Join results with SQL Server chunks.  
- Build semantic + hybrid search with debug view in Admin Panel.

