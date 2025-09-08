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
