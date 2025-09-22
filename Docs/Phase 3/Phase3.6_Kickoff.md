# 📊 Phase 3.6 – Analytics & Insights (Kickoff Plan)

## 📜 Background & History

### **Phase 1 – Foundation**
- Explored Generative AI use cases in .NET apps (chatbots, summarization, content generation, code assist).
- Tested HuggingFace models + Azure deployment.
- Learned Prompt Engineering basics: Zero-Shot, Few-Shot, Role Prompting.

### **Phase 2 – .NET Integration**
- **Backend (2.1):** .NET Core Web API with `ChatController`, services layer (`IOpenAiService`, `IChatHistoryService`), persistence in SQL Server.  
- **Frontend (2.2):** Modular JS (chat, sessions, templates, utils, main), with streaming, typing dots, prompt preview.  
- **Deployment (2.3):** Azure App Service + SQL Azure.  
- **Prompt Templates (2.4):** CRUD templates stored in DB with live preview.  

### **Phase 3 – RAG Enhancements**
- **3.1 – Architecture Foundations:** Modular RAG pipeline, DTOs, retrieval + OpenAI abstraction.  
- **3.2 – Service Integration:** Hybrid retrieval with embeddings, prompt building.  
- **3.3 – Admin UI:** Comparison UI, history modal.  
- **3.4 – Prompt Engineering:** Added Zero-Shot, Few-Shot, Role, RAG-Augmented, Hybrid Role+RAG. Logged `PromptStyle` in DB.  
- **3.5 – History & Docs Enhancements:**  
  - History filters (SLA, date, PromptStyle).  
  - Details modal with expandable chunks.  
  - Compare Mode (multiple runs side-by-side).  
  - Export CSV/Excel.  
  - **Docs Module**: Upload/Edit/Delete/Rebuild embeddings.  
  - Migrated to Bootstrap 5 UI.  

---

## 🎯 Objectives for Phase 3.6
- Provide **Analytics & Insights** into RAG experiments.  
- Visualize trends: SLA compliance, latency, PromptStyle usage, performance comparisons.  
- Enable admins to make **data-driven decisions** on prompt engineering & retrieval strategies.  

---

## 🔹 Planned Features

### 1. **Analytics Dashboard (AdminLTE)**
- SLA compliance rate (✅ vs ⚠️).  
- Avg retrieval, LLM, and total latency.  
- Query volume over time.  
- PromptStyle distribution.  

### 2. **Charts & Visualizations**
- **Bar charts**: Avg latency per PromptStyle.  
- **Line charts**: SLA compliance trends over time.  
- **Pie charts**: Query distribution by PromptStyle or document source.  
- **Heatmaps**: SLA performance by hour/day.  

### 3. **Filters**
- Date range.  
- SLA status.  
- PromptStyle.  
- Provider/Model.  

### 4. **Export**
- Export chart data to CSV/Excel.  

---

## 🔹 Technical Approach

### Backend – Business Services
- Extend `RagHistoryService` with **aggregation methods**:  
  - `GetAverageLatencies()` – avg retrieval/LLM/total latency.  
  - `GetSlaCompliance()` – % of responses within SLA.  
  - `GetPromptStyleUsage()` – count/distribution by PromptStyle.  
  - `GetTrends()` – SLA & latency over time.  
- Use **LINQ queries** directly over `RagComparisonHistories`.  
- Return DTOs for MVC Views.  

### Frontend – AdminLTE MVC
- New **Analytics Controller & Views** (`/Analytics`).  
- Views call business services → pass DTOs → render with **Chart.js**.  
- Bootstrap 5 cards for clean grid layout.  
- Drill-down views per PromptStyle and SLA status.  

### Database
- No schema changes (data already logged: PromptStyle, SLA, latencies).  
- Add **indexes** on `PromptStyle`, `CreatedAt`, `IsWithinSla` for performance.  

---

## 🔹 Updated Project Structure (till Phase 3.5)

```
/ArNir
├── Library
│   ├── ArNir.Core → Entities, DTOs, Config, Validations
│   ├── ArNir.Data → DbContexts (SQL Server + Postgres), 
│   │                 EF Core migrations (separate SqlServer/Postgres folders)
│   └── ArNir.Services → Business logic 
│                        (EmbeddingService, RetrievalService, RagService, RagHistoryService)

├── Presentation
│   ├── ArNir.Admin → AdminLTE UI (ASP.NET Core MVC project)
│   │   ├── Views
│   │   ├── wwwroot/js
│   │   ├── Controllers
│   │   │   ├── Docs (Upload/Edit/Delete, Rebuild Embeddings)
│   │   │   ├── Embedding Test Page (/Embedding/Test)
│   │   │   ├── Retrieval Test Page (/Retrieval/Test)
│   │   │   ├── RAG Comparison Page (/RagComparison)
│   │   │   ├── RAG History Page (/RagHistory)
│   │   │   └── Analytics Dashboard (new in Phase 3.6)
│   └── ArNir.Frontend → End-user search/chat interface (planned Phase 3.6+)

├── sql
│   ├── create_tables.sql
│   ├── update_documents_chunks.sql
│   ├── update_embeddings.sql
│   └── update_rag_history.sql

└── docs
    ├── Phase3
    │   ├── Phase3_RAG_Architecture.png
    │   ├── Phase3.3_Architecture.png
    │   ├── Phase3.4_Architecture.png
    │   ├── Phase3.5_Architecture.png
    │   ├── Phase3.5_Technical_Architecture.png
    │   ├── Phase3.4_RAG.md
    │   ├── Phase3_RAG.md
    │   └── Phase_3_RAG.md
    ├── GenerativeAI_KnowledgeBase.md
    ├── GenerativeAI_KnowledgeBase_Phsase3.docx
    ├── README.md
    └── CHANGELOG.md
```

---

## 📅 Timeline
- **Week 1:** Extend RagHistoryService with aggregate queries.  
- **Week 2:** Build Analytics Controller & Views (Overview page).  
- **Week 3:** Add charts (SLA trend, PromptStyle performance, heatmap).  
- **Week 4:** Testing + Documentation updates.  

---

## ✅ Deliverables
1. Extended **RagHistoryService** with analytics methods.  
2. **Analytics Dashboard (AdminLTE)** with interactive charts.  
3. Filters & Export support.  
4. Updated **KnowledgeBase, README, CHANGELOG** with Phase 3.6 progress.