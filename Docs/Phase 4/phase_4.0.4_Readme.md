# Phase 4.0.4 – Frontend Analytics Dashboard

## 📅 Phase Timeline
- **Start:** 2025-10-12
- **End:** Current
- **Previous:** Phase 4.0.3 (Feedback & Analytics Integration)
- **Next:** Phase 4.0.5 (Admin Reports & Data Export)

---

## 🎯 Objectives

This phase adds a **React-based Analytics Dashboard** for live visualization of RAG system metrics, model latency, SLA compliance, and user feedback quality.

### Goals:
1. 📊 Display RAG & Feedback analytics in real time.
2. ⭐ Show KPI cards for Total Runs, SLA %, Avg Latency, Avg Rating.
3. 📈 Render bar, pie, and trend charts using **Recharts**.
4. ⚙️ Filter analytics by provider, model, and date range.
5. 📤 Export analytics data to CSV/Excel.
6. 🌐 Integrate directly with backend API at **https://localhost:5001**.

---

## 🧩 Project Structure Updates

```
/AirNir
├── Presentation
│   └── ArNir.Frontend.React
│       ├── src
│       │   ├── api
│       │   │   └── analytics.js
│       │   ├── components
│       │   │   ├── AnalyticsDashboard.jsx
│       │   │   ├── AnalyticsCharts.jsx
│       │   │   ├── KPIWidget.jsx
│       │   │   ├── ExportButton.jsx
│       │   │   └── FiltersBar.jsx
│       │   ├── pages
│       │   │   └── AnalyticsPage.jsx
│       │   └── App.jsx (updated route)
│       ├── package.json
│       └── vite.config.js
│
└── docs
    └── Phase4.0.4_Architecture.png
```

---

## 🔗 Backend Integration

### Base URL
```
https://localhost:5001
```

### Endpoints Used
| Endpoint | Method | Description |
|-----------|---------|-------------|
| `/api/analytics/provider` | `GET` | Fetch aggregated RAG analytics |
| `/api/feedback/average` | `GET` | Fetch average feedback rating |

---

## 🔁 Data Flow Overview

```
[React Analytics UI]
   ↓
[Axios API Calls → https://localhost:5001]
   ↓
[.NET Backend: AnalyticsController + RagService]
   ↓
[SQL Database: RAG History + Feedback Tables]
   ↓
[Aggregated Metrics → React Recharts Visualization]
```

---

## ✅ Phase Outcomes

| Area | Enhancement | Status |
|------|--------------|--------|
| Frontend | New `/analytics` React Route | ✅ |
| Visualization | KPI Widgets + Charts (Recharts) | ✅ |
| Backend | Live metrics via `/api/analytics/provider` | ✅ |
| Export | CSV/Excel Support | ✅ |
| Filters | Provider + Model | ✅ |

---

## 🚀 Next Phase: 4.0.5 – Admin Reports & Data Export

- Extend dashboard with date range filters  
- Export analytics summary to Excel/PDF  
- Add Admin panel tab: “Model Comparison Reports”  
- Integrate automated weekly email report (optional)
