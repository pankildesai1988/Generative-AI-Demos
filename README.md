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

---

## 📂 Repository Structure

generative-ai-mentor/
│
├── docs/ # Documentation & Knowledge Base
│ ├── GenerativeAI_KnowledgeBase.md
│ ├── GenerativeAI_KnowledgeBase.docx
│ ├── Updated_GenerativeAI_Learning_Tracker.xlsx
│ └── architecture-diagram.png # (optional)
│
├── src/ # Source Code
│ ├── backend/ # .NET Backend
│ │ ├── Controllers/
│ │ ├── Data/
│ │ ├── DTOs/
│ │ ├── Models/
│ │ ├── Services/
│ │ └── Program.cs
│ │
│ ├── frontend/ # Frontend
│ │ ├── wwwroot/js/
│ │ └── Views/Home/Index.cshtml
│ │
│ └── sql/ # SQL Scripts
│ ├── create_tables.sql
│ └── seed_templates.sql
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
---
# 📖 Learning Tracker #

**Progress is documented in:**

**docs/GenerativeAI_KnowledgeBase.md**

**docs/Updated_GenerativeAI_Learning_Tracker.xlsx**

## 🎯 Roadmap
**✅ Completed**

**Phase 1: Foundation**

**Phase 2.1: Backend + Frontend Integration**

**Phase 2.2: Prompt Templates + Clean UI**

**Phase 2.3: Template Management (Admin Panel, Versioning, Advanced Parameters, Live Preview)**



## 🚀 Phase Highlights

### Phase 2.3 – Admin Panel for Prompt Templates
- Integrated **AdminLTE** UI for `/Admin` area.
- Added **Custom JWT Authentication** (Login/Logout).
- Implemented full **CRUD for templates** via backend API.
- Built **Dynamic Parameter Editor** (type, required, regex, options).
- Enabled **Live Preview** with inline validation + tooltips + error list.
- Added **Validation Toggle** (persistent via localStorage) + reset button.
- Introduced **Template Versioning** (history, rollback, compare).

##⏳ In Progress

**Phase 2.4: Session Cloning for Cross-Model Comparisons**

## 🔜 Next

**Phase 3: Retrieval-Augmented Generation (RAG)**

**Phase 4: Enterprise Features (Security, Multi-tenancy, Analytics, Scaling)**

## 🛠️ Tech Stack

**Backend:** .NET 7, ASP.NET Core Web API

**Frontend:** HTML, Bootstrap, jQuery + Modular JS

**Database**: SQL Server (local & Azure SQL)

**AI Models**: OpenAI GPT-3.5, GPT-4o

**Cloud**: Azure App Service, Azure SQL, Azure App Config

## 📌 License

**MIT License** – feel free to use and adapt!

# 👨‍🏫 Author - pankildesai1988

**Built as part of my Generative AI Mentor Journey 🧑‍💻**

Learning → Building → Deploying → Scaling 🚀
