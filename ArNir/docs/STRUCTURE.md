# ArNir.Frontend.React - Project Structure

```
ArNir.Frontend.React/
├── public/
│   └── vite.svg - Default Vite project logo asset
│
├── src/
│   ├── api/
│   │   ├── client.js - Axios HTTP client configuration with base URL and interceptors
│   │   ├── analytics.js - API endpoints for analytics data retrieval and metrics computation
│   │   ├── chat.js - API client for chat queries, responses, and messaging operations
│   │   └── intelligence.js - API service for unified intelligence dashboard and insights
│   │
│   ├── components/
│   │   ├── analytics/
│   │   │   ├── AnalyticsCharts.jsx - Renders interactive charts for KPI and performance metrics
│   │   │   ├── AnalyticsDashboard.jsx - Main analytics page layout with filters and charts
│   │   │   ├── ExportButton.jsx - Button component to export analytics data as PDF/CSV
│   │   │   ├── FeedbackModal.jsx - Modal for collecting user feedback on analytics accuracy
│   │   │   ├── FiltersBar.jsx - Filter controls for date range, provider, and prompt style
│   │   │   ├── KPIWidget.jsx - Displays key performance indicator cards with metrics
│   │   │   └── index.js - Barrel export for analytics components
│   │   │
│   │   ├── chat/
│   │   │   ├── Chat.jsx - Main chat interface with message history and input field
│   │   │   ├── ChatInsightBox.jsx - Displays AI insights derived from chat queries
│   │   │   ├── HighlightedMessage.jsx - Assistant bubble; react-markdown + KaTeX math + term badges
│   │   │   ├── DocumentSelector.jsx - Multi-doc scope picker for RAG queries
│   │   │   ├── ExportChatButton.jsx - Export chat transcript (jsPDF)
│   │   │   ├── PdfViewer.jsx / PdfJsViewer.jsx - Inline source doc viewer, bbox highlight (null-safe)
│   │   │   ├── SourceDocPanel.jsx - Retrieved chunk source panel
│   │   │   └── InsightChartCard.jsx - Chart visualization component for chat insights
│   │   │
│   │   ├── insights/
│   │   │   ├── ActionButtons.jsx - Action recommendation buttons based on insights
│   │   │   ├── AnomalyList.jsx - Lists detected anomalies with severity indicators
│   │   │   ├── DataInputBox.jsx - Input field for entering analysis/query data
│   │   │   ├── InsightSummary.jsx - Displays narrative summary of generated insights
│   │   │   ├── PredictionChart.jsx - Forecasting and trend prediction visualization
│   │   │   ├── ReportPreview.jsx - Preview panel for generated report content
│   │   │   ├── TrendSummaryBox.jsx - Summary card for trend data and patterns
│   │   │   └── index.js - Barrel export for insights components
│   │   │
│   │   ├── intelligence/
│   │   │   ├── AlertList.jsx - Displays system alerts and anomaly notifications
│   │   │   ├── ExportPanel.jsx - Panel controls for exporting intelligence data
│   │   │   ├── FiltersBar.jsx - Filtering options for intelligence dashboard
│   │   │   ├── IntelligenceDashboard.jsx - Unified dashboard with all intelligence features
│   │   │   ├── InsightChartCard.jsx - Chart card component for intelligence insights
│   │   │   ├── InsightChatBox.jsx - Chat interface within intelligence dashboard
│   │   │   ├── InsightFeed.jsx - Feed display for streaming insights and updates
│   │   │   ├── KPIGroup.jsx - Groups multiple KPI widgets for organized display
│   │   │   ├── KPIInlineWidget.jsx - Inline KPI display with real-time metrics
│   │   │   ├── SemanticRecallPanel.jsx - Panel for displaying semantically related queries
│   │   │   ├── UnifiedCharts.jsx - Combined chart visualization for multiple metrics
│   │   │   └── index.js - Barrel export for intelligence components
│   │   │
│   │   ├── shared/
│   │   │   ├── ErrorBanner.jsx - Error notification banner display component
│   │   │   ├── Loader.jsx - Loading spinner and skeleton components
│   │   │   └── index.js - Barrel export for shared components
│   │   │
│   │   └── ui/
│   │       ├── button.jsx - Reusable button component with variants and states
│   │       ├── card.jsx - Reusable card container component for content organization
│   │       └── input.jsx - Reusable input field component with validation
│   │
│   ├── utils/
│   │   ├── normalizeMath.js - Coerce \(..\)/\[..\]/parenthesised-LaTeX → $..$/$$..$$ for KaTeX
│   │   ├── highlightTerms.js - Extract entity/number/code terms for message badges
│   │   └── exportChat.js - Build chat transcript PDF (jsPDF)
│   │
│   ├── hooks/
│   │   ├── useChat.js - RAG chat (provider/model/promptStyle, optional documentIds scope)
│   │   ├── useChatStream.js - SSE streaming chat w/ fallback
│   │   ├── useDocumentList.js - Fetch GET /api/documents for doc scope
│   │   ├── useFileUpload.js - Drag-drop upload + validation
│   │   ├── useFocusTrap.js / useKeyboardNav.js - Accessibility hooks
│   │
│   ├── pages/
│   │   ├── AnalyticsPage.jsx - Full-page analytics dashboard and metrics view
│   │   ├── ChatInsightsPage.jsx - Combined chat and insights page layout
│   │   ├── InsightChatPage.jsx - Insight-focused page with chat capabilities
│   │   ├── InsightsPage.jsx - Standalone insights exploration and analysis page
│   │   └── IntelligencePage.jsx - Unified intelligence dashboard page
│   │
│   ├── assets/
│   │   └── react.svg - React project logo and branding asset
│   │
│   ├── App.jsx - Root component with routing and main layout wrapper
│   ├── App.css - Global application and component-specific styles
│   ├── index.css - Global CSS reset and base element styling
│   └── main.jsx - Application entry point and React DOM rendering
│
├── .gitignore - Git ignore rules for node_modules, build artifacts, and IDE files
├── .vite/ - Vite development server cache directory
├── dist/ - Production build output directory
├── eslint.config.js - ESLint configuration for code quality and linting rules
├── index.html - Main HTML entry point with root div and script tags
├── package.json - Project dependencies, scripts, and metadata
├── package-lock.json - Locked dependency versions for consistent installations
├── postcss.config.js - PostCSS configuration for Tailwind CSS processing
├── README.md - Project overview and setup instructions
├── tailwind.config.js - Tailwind CSS configuration with theme customization
├── vite.config.js - Vite bundler configuration with plugins and optimization
└── node_modules/ - Installed npm dependencies (not tracked in version control)
```

## Directory Overview

### `/src/api/`
RESTful API client layer handling all backend service communication with auto-retry logic and error handling.

### `/src/components/`
Modular React components organized by feature domain (analytics, chat, insights, intelligence) with shared UI utilities.

### `/src/pages/`
Top-level page components representing distinct application routes and feature pages.

### `/src/assets/`
Static assets including images, icons, and media files used throughout the application.

## Key Technologies

- **React** - UI component library and framework
- **Vite** - Fast development server and build tool
- **Tailwind CSS** - Utility-first CSS framework
- **Axios** - HTTP client for API communication
- **react-markdown + remark-math + rehype-katex + KaTeX** - Render markdown + LaTeX math in RAG answers
- **jsPDF** - Chat transcript export
- **ESLint** - Code quality and linting tool

## Development Workflow

1. **Components** → Reusable building blocks organized by feature
2. **Pages** → Combine components into full-page views
3. **API** → Centralized backend communication layer
4. **UI** → Primitive components for consistent styling

