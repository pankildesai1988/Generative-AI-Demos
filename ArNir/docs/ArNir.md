ArNir/
├── ArNir.Core/
│   ├── DTOs/
│   │   ├── Chat/
│   │   │   ├── ChatRequestDto.cs - Encapsulates user chat queries with session/context metadata
│   │   │   ├── ChatResponseDto.cs - Contains AI response, chart data, insights, and suggested actions
│   │   │   ├── ChatQueryDto.cs - Wraps user query input with embedding and retrieval parameters
│   │   │   └── ChartItemDto.cs - Unified model for chart visualization data representation
│   │   ├── Analytics/
│   │   │   ├── AvgLatencyDto.cs - Aggregates average latency metrics by provider and model
│   │   │   ├── SlaComplianceDto.cs - Tracks SLA compliance rates by prompt style
│   │   │   ├── PromptStyleUsageDto.cs - Counts usage frequency of each prompt style
│   │   │   ├── TrendDto.cs - Time-series data for latency trends over date ranges
│   │   │   └── ProviderAnalyticsDto.cs - Comprehensive provider performance with SLA and feedback metrics
│   │   ├── Intelligence/
│   │   │   ├── UnifiedDashboardDto.cs - Aggregates KPIs, charts, alerts, and AI insights
│   │   │   └── AIInsightDto.cs - Represents AI-generated insights with patterns and recommendations
│   │   ├── Insights/
│   │   │   ├── InsightDto.cs - Structured insight data with summary, source, and metadata
│   │   │   └── RelatedInsightDto.cs - Contextual insights related to user queries via semantic recall
│   │   ├── Common/
│   │   │   ├── BaseDto.cs - Base class for all DTOs with common properties
│   │   │   └── PaginationDto.cs - Generic pagination metadata for list responses
│   │   └── Feedback/
│   │       ├── FeedbackDto.cs - User feedback on query accuracy and relevance
│   │       └── RatingDto.cs - Numeric rating data for feedback aggregation
│   │
│   ├── Entities/
│   │   ├── RagComparisonHistory.cs - Stores query execution history with latency and SLA metrics
│   │   ├── ChatMemory.cs - Persists user chat history for session context management
│   │   ├── ChatEmbedding.cs - Stores embeddings of chat messages for semantic recall
│   │   ├── UserFeedback.cs - Captures user feedback ratings and comments on results
│   │   ├── Document.cs - Represents knowledge base documents with metadata
│   │   ├── DocumentChunk.cs - Stores chunked document segments with embeddings
│   │   ├── Alert.cs - System alerts for anomalies and threshold violations
│   │   └── ExportHistory.cs - Tracks analytics and report export operations
│   │
│   ├── Interfaces/
│   │   ├── IChatInsightService.cs - Orchestrates chat processing and insight generation
│   │   ├── IRagService.cs - Manages RAG pipeline with retrieval and context-aware responses
│   │   ├── IInsightEngineService.cs - Generates AI-driven insights from analytics data
│   │   ├── IRagHistoryService.cs - Manages semantic recall and query history persistence
│   │   ├── IAnalyticsService.cs - Computes KPIs and aggregated metrics for dashboards
│   │   ├── IRetrievalService.cs - Handles vector search and document retrieval
│   │   ├── IEmbeddingService.cs - Generates and manages text embeddings
│   │   ├── IFeedbackService.cs - Processes and aggregates user feedback
│   │   ├── IVisualizationService.cs - Converts raw metrics to chart-ready data
│   │   ├── INotificationService.cs - Manages alerts and system notifications
│   │   ├── IPredictiveTrendService.cs - Forecasts future trends using historical data
│   │   ├── IAIInsightService.cs - Generates anomaly detection and insight summaries
│   │   ├── IIntelligenceService.cs - Unified service orchestrating all intelligence operations
│   │   ├── IChatEmbeddingService.cs - Handles embedding generation for chat messages
│   │   ├── INaturalQueryService.cs - Converts natural language to structured queries
│   │   ├── IActionEngineService.cs - Recommends and executes suggested actions
│   │   ├── ILlmService.cs - Abstract interface for LLM provider operations
│   │   ├── IDocumentService.cs - Manages document upload, chunking, and indexing
│   │   ├── IContextMemoryService.cs - Maintains multi-turn conversation context
│   │   ├── INaturalLanguageCommandService.cs - Interprets natural language commands
│   │   ├── IInsightHistoryService.cs - Persists and retrieves generated insights
│   │   ├── IExportService.cs - Exports analytics data to PDF/CSV formats
│   │   └── IExportHistoryService.cs - Tracks export operations and metadata
│   │
│   ├── Config/
│   │   ├── FileUploadSettings.cs - Configuration for document upload size and formats
│   │   └── OpenAiSettings.cs - API keys and model configuration for OpenAI provider
│   │
│   ├── Enums/
│   │   ├── PromptStyleEnum.cs - Defines prompt style variants (RAG, Baseline, Hybrid)
│   │   ├── SlaStatusEnum.cs - Enum for SLA status (Compliant, Violated, Warning)
│   │   └── ProviderEnum.cs - Supported LLM providers (OpenAI, Gemini, Claude)
│   │
│   └── Constants/
│       └── ApplicationConstants.cs - Global application constants and configuration values
│
├── ArNir.Services/
│   ├── AI/
│   │   ├── ChatInsightService.cs - Orchestrates end-to-end chat/insight workflow
│   │   ├── InsightEngineService.cs - Generates narrative insights and visualizations from metrics
│   │   ├── VisualizationService.cs - Transforms analytics data into chart-friendly format
│   │   ├── RagService.cs - Core RAG implementation with retrieval and context integration
│   │   ├── RagHistoryService.cs - Manages query history persistence and semantic recall
│   │   ├── ChatEmbeddingService.cs - Generates and stores embeddings for chat messages
│   │   ├── NaturalQueryService.cs - Parses natural language into structured queries
│   │   ├── ActionEngineService.cs - Recommends actions based on insights and context
│   │   ├── ContextMemoryService.cs - Maintains multi-turn conversation state
│   │   ├── AnomalyDetectionService.cs - Detects statistical anomalies in metrics
│   │   ├── PredictiveModelService.cs - Forecasts latency and performance trends
│   │   ├── NarrativeReportService.cs - Generates human-readable narrative summaries
│   │   ├── LlmEvaluationService.cs - LLM-as-judge RAG quality scoring (relevance + faithfulness)
│   │   ├── EvaluationHistoryService.cs - Paginated evaluation history, stats, persistence
│   │   └── Interfaces/
│   │       ├── IChatInsightService.cs - Service contract for chat processing
│   │       ├── IInsightEngineService.cs - Service contract for insight generation
│   │       ├── IVisualizationService.cs - Service contract for data visualization
│   │       └── IEvaluationHistoryService.cs - Evaluation history and statistics contract
│   │
│   ├── Analytics/
│   │   ├── AnalyticsService.cs - Computes aggregated metrics and KPIs for dashboards
│   │   ├── PredictiveTrendService.cs - Forecasts future performance using ML models
│   │   └── AIInsightService.cs - Generates anomaly-based and pattern-based insights
│   │
│   ├── Insights/
│   │   ├── InsightService.cs - Generates structured insights from various data sources
│   │   └── InsightHistoryService.cs - Stores and retrieves historical insights
│   │
│   ├── Provider/
│   │   ├── OpenAiService.cs - Implements OpenAI API integration for LLM operations
│   │   ├── GeminiService.cs - Implements Google Gemini API integration
│   │   ├── ClaudeService.cs - Implements Anthropic Claude API integration
│   │   ├── Interfaces/
│   │   │   └── IEmbeddingProvider.cs - Abstract interface for embedding providers
│   │   └── OpenAiEmbeddingProvider.cs - OpenAI text embedding implementation
│   │
│   ├── Retrieval/
│   │   ├── RetrievalService.cs - Performs vector similarity search and document retrieval
│   │   └── EmbeddingService.cs - Manages embedding generation and caching
│   │
│   ├── Feedback/
│   │   ├── FeedbackService.cs - Aggregates and analyzes user feedback ratings
│   │   └── ExportHistoryService.cs - Tracks export operations and metadata
│   │
│   ├── Document/
│   │   ├── DocumentService.cs - Handles document lifecycle (upload, parse, chunk, embed)
│   │   └── ChunkProcessor.cs - Splits documents into semantic chunks
│   │
│   ├── Notification/
│   │   └── NotificationService.cs - Manages alerts for anomalies and SLA violations
│   │
│   ├── Common/
│   │   └── Helper/
│   │       ├── JsonHelper.cs - JSON serialization and manipulation utilities
│   │       └── DateTimeHelper.cs - Date/time calculation and formatting utilities
│   │
│   ├── Mapping/
│   │   └── MappingProfile.cs - AutoMapper configuration for DTO-to-Entity mapping
│   │
│   ├── Interfaces/
│   │   ├── IIntelligenceService.cs - Unified intelligence orchestration service
│   │   ├── IRagService.cs - RAG pipeline operations contract
│   │   ├── IAnalyticsService.cs - Analytics computation service contract
│   │   ├── IRetrievalService.cs - Document retrieval service contract
│   │   ├── IEmbeddingService.cs - Embedding generation service contract
│   │   ├── IFeedbackService.cs - Feedback processing service contract
│   │   ├── INotificationService.cs - Notification and alert service contract
│   │   ├── IPredictiveTrendService.cs - Trend forecasting service contract
│   │   ├── IAIInsightService.cs - AI insight generation service contract
│   │   ├── IDocumentService.cs - Document management service contract
│   │   ├── IRagHistoryService.cs - RAG history persistence service contract
│   │   ├── ILlmService.cs - LLM provider abstraction contract
│   │   ├── IExportService.cs - Data export service contract
│   │   └── IExportHistoryService.cs - Export history tracking service contract
│   │
│   └── Extensions/
│       ├── ServiceCollectionExtensions.cs - Dependency injection setup extensions
│       └── StringExtensions.cs - String manipulation utility extensions
│
├── ArNir.Data/
│   ├── DbContexts/
│   │   ├── ArNirDbContext.cs - EF Core SQL Server context for relational data
│   │   └── VectorDbContext.cs - PostgreSQL context with pgvector for embeddings
│   │
│   ├── Repository/
│   │   ├── GenericRepository.cs - Generic CRUD operations for all entities
│   │   ├── RagRepository.cs - Specialized repository for RAG query history
│   │   ├── RagHistoryRepository.cs - Repository for semantic recall persistence
│   │   ├── DocumentRepository.cs - Repository for document management
│   │   ├── ChatMemoryRepository.cs - Repository for chat history persistence
│   │   └── Interfaces/
│   │       ├── IGenericRepository.cs - Generic repository contract
│   │       ├── IRagRepository.cs - RAG repository contract
│   │       ├── IRagHistoryRepository.cs - RAG history repository contract
│   │       └── IDocumentRepository.cs - Document repository contract
│   │
│   ├── Configurations/
│   │   ├── EntityConfigurations.cs - Fluent API configurations for all entities
│   │   ├── RagComparisonHistoryConfig.cs - RagComparisonHistory entity mapping
│   │   ├── ChatMemoryConfig.cs - ChatMemory entity mapping
│   │   └── DocumentConfig.cs - Document entity mapping
│   │
│   ├── Migrations/
│   │   ├── SqlServer/
│   │   │   └── [Migration files] - SQL Server schema migration history
│   │   ├── PostgreSQL/
│   │   │   └── [Migration files] - PostgreSQL schema migration history
│   │   └── Initial/
│   │       └── [Initial setup migrations] - Initial database schema creation
│   │
│   └── Seeding/
│       └── DataSeeder.cs - Initializes database with sample/reference data
│
├── Presentation/
│   ├── ArNir.API/ (REST API for frontend applications)
│   │   ├── Controllers/
│   │   │   ├── AnalyticsController.cs - Exposes analytics and metrics endpoints
│   │   │   ├── ChatController.cs - Handles chat request routing and responses
│   │   │   ├── FeedbackController.cs - Manages user feedback submission
│   │   │   ├── InsightsController.cs - Provides AI-generated insights endpoints
│   │   │   ├── IntelligenceController.cs - Main unified intelligence dashboard endpoint
│   │   │   ├── RagController.cs - RAG pipeline execution endpoints
│   │   │   ├── RetrievalController.cs - Document retrieval and search endpoints
│   │   │   ├── DocumentController.cs - Document upload and management endpoints
│   │   │   ├── ExportController.cs - Analytics export (PDF/CSV) endpoints
│   │   │   ├── EvaluationController.cs - LLM-as-judge evaluation endpoints (history, evaluate, stats)
│   │   │   ├── IntelligenceChatController.cs - Unified intelligence chat endpoint
│   │   │   ├── DocumentIngestController.cs - Document upload + RAG pipeline trigger
│   │   │   └── HealthController.cs - Health check and status endpoints
│   │   │
│   │   ├── Middleware/
│   │   │   ├── ExceptionHandlingMiddleware.cs - Global exception handling and logging
│   │   │   └── LoggingMiddleware.cs - Request/response logging and tracing
│   │   │
│   │   ├── Filters/
│   │   │   └── ValidateModelFilter.cs - Automatic model validation filter
│   │   │
│   │   ├── Program.cs - Application startup and configuration
│   │   ├── appsettings.json - Default configuration values
│   │   ├── appsettings.Development.json - Development environment overrides
│   │   ├── appsettings.Production.json - Production environment configuration
│   │   ├── ArNir.API.csproj - API project file with dependencies
│   │   └── Properties/
│   │       └── launchSettings.json - Debug and launch profiles
│   │
│   └── ArNir.Admin/ (MVC/Razor Pages admin dashboard)
│       ├── Controllers/
│       │   ├── AnalyticsController.cs - Analytics dashboard and metrics endpoints
│       │   ├── DocumentController.cs - Document upload and indexing UI
│       │   ├── EmbeddingController.cs - Embedding generation and management
│       │   ├── HomeController.cs - Main dashboard and home page
│       │   ├── RagComparisonController.cs - RAG baseline comparison view
│       │   ├── RagHistoryController.cs - Query history and semantic recall UI
│       │   ├── ReportsController.cs - Custom report generation and export
│       │   ├── RetrievalController.cs - Document search and retrieval UI
│       │   ├── FeedbackController.cs - Feedback review and analytics
│       │   ├── EvaluationController.cs - LLM-as-judge dashboard (KPIs, trends, DataTable)
│       │   ├── PromptTemplateController.cs - CRUD + versioning (History, Rollback, Compare)
│       │   ├── PlatformSettingsController.cs - Runtime config CRUD
│       │   ├── ProviderConfigController.cs - API key management (OpenAI/Gemini/Claude)
│       │   ├── ObservabilityDashboardController.cs - SLA metrics, latency trends
│       │   ├── NotificationController.cs - SLA breach alerts (bell icon, 30s polling)
│       │   ├── JobMonitorController.cs - Live queue depth + recent jobs
│       │   ├── AgentRunHistoryController.cs - Agent run history + manual trigger
│       │   ├── VectorStoreController.cs - Embeddings by model, orphan detection
│       │   ├── MemoryController.cs - Chat session management
│       │   └── AccountController.cs - Cookie auth login/logout
│       │
│       ├── Models/
│       │   ├── ChartViewModel.cs - Chart data presentation model
│       │   ├── AnalyticsViewModel.cs - Analytics page view model
│       │   └── RagHistoryViewModel.cs - RAG history page view model
│       │
│       ├── ViewModels/
│       │   ├── DashboardViewModel.cs - Main dashboard view data
│       │   ├── InsightViewModel.cs - Insight display view model
│       │   └── RagComparisonViewModel.cs - RAG comparison view model
│       │
│       ├── Views/
│       │   ├── Shared/
│       │   │   ├── _Layout.cshtml - Master page layout
│       │   │   ├── _Navigation.cshtml - Navigation bar component
│       │   │   └── _Footer.cshtml - Footer component
│       │   ├── Home/
│       │   │   ├── Index.cshtml - Home page
│       │   │   └── Dashboard.cshtml - Main analytics dashboard
│       │   ├── Analytics/
│       │   │   ├── Index.cshtml - Analytics list view
│       │   │   └── Details.cshtml - Analytics detail view
│       │   ├── RagHistory/
│       │   │   ├── Index.cshtml - Query history list
│       │   │   └── Details.cshtml - Query history detail with context
│       │   ├── Document/
│       │   │   ├── Upload.cshtml - Document upload form
│       │   │   └── List.cshtml - Document inventory list
│       │   ├── Reports/
│       │   │   └── Index.cshtml - Report generation and export UI
│       │   ├── Evaluation/
│       │   │   ├── Index.cshtml - LLM-as-judge dashboard (KPI cards, Chart.js trends, DataTable)
│       │   │   └── Details.cshtml - Single evaluation detail view
│       │   └── PromptTemplate/
│       │       ├── Index.cshtml - Template list with version/history buttons
│       │       ├── CreateEdit.cshtml - Version-aware create/edit form
│       │       ├── History.cshtml - Version timeline with compare selector + rollback
│       │       ├── Compare.cshtml - Side-by-side diff with JS line-by-line highlighting
│       │       └── Stats.cshtml - A/B testing chart (Chart.js)
│       │
│       ├── wwwroot/
│       │   ├── css/
│       │   │   ├── site.css - Global stylesheet
│       │   │   └── custom.css - Custom application styles
│       │   ├── js/
│       │   │   ├── site.js - Global JavaScript utilities
│       │   │   ├── chart.js - Chart.js wrapper and initialization
│       │   │   └── api-client.js - API client for AJAX calls
│       │   ├── images/
│       │   │   └── [Logo & assets] - Brand and application images
│       │   └── lib/
│       │       ├── bootstrap/ - Bootstrap CSS framework
│       │       └── chart.js/ - Chart.js library for visualizations
│       │
│       ├── Program.cs - Admin application startup and configuration
│       ├── appsettings.json - Default configuration values
│       ├── appsettings.Development.json - Development environment overrides
│       ├── appsettings.Production.json - Production environment configuration
│       ├── ArNir.Admin.csproj - Admin project file with dependencies
│       └── Properties/
│           └── launchSettings.json - Debug and launch profiles
│
├── ArNir.Tests/ (72 unit tests - xUnit + Moq + EF InMemory)
│   ├── Sprint1/ (12 tests)
│   │   ├── PgvectorDocumentEmbedderTests.cs - pgvector embedder tests
│   │   ├── IngestionQueueTests.cs - Background queue tests
│   │   ├── AccountControllerTests.cs - Auth controller tests
│   │   └── DocumentControllerTests.cs - Document upload tests
│   ├── Sprint2/ (5 tests)
│   │   ├── HomeControllerTests.cs - Health dashboard tests
│   │   ├── VectorStoreControllerTests.cs - Vector store tests
│   │   └── ProviderConfigControllerTests.cs - Provider config tests
│   ├── Sprint3/ (19 tests)
│   │   ├── EmbeddingControllerTests.cs - Embedding management tests
│   │   ├── MemoryControllerTests.cs - Memory panel tests
│   │   ├── JobMonitorControllerTests.cs - Job monitor tests
│   │   └── AgentRunHistoryControllerTests.cs - Agent run tests
│   ├── Sprint4/ (13 tests)
│   │   ├── RagHistoryControllerTests.cs - RAG history + feedback tests
│   │   ├── PromptTemplateControllerTests.cs - Template CRUD tests
│   │   └── NotificationControllerTests.cs - Notification tests
│   ├── Sprint5/ (5 tests)
│   │   └── DocumentIngestControllerApiTests.cs - API document ingest tests
│   ├── Sprint6/ (10 tests)
│   │   ├── LlmEvaluationServiceTests.cs - LLM-as-judge evaluation (6 tests)
│   │   └── EvaluationControllerAdminTests.cs - Admin eval dashboard (4 tests)
│   └── Sprint8/ (8 tests)
│       └── PromptVersioningTests.cs - Edit-creates-version, history, rollback, compare
│
├── Solution Items/
│   ├── .gitignore - Git ignore rules for build artifacts
│   ├── .env.example - Environment variable template
│   ├── docker-compose.yml - PostgreSQL + pgvector + .NET apps (profile: full)
│   ├── Dockerfile.admin - Multi-stage .NET 9 build for ArNir.Admin (port 5001)
│   ├── Dockerfile.api - Multi-stage .NET 9 build for ArNir.API (port 5000)
│   ├── README.md - Project overview, architecture diagram, quick start, API reference
│   └── CLAUDE.md - Sprint logs and development context
│
├── Docs/ (Comprehensive project documentation)
│   ├── ArNir-Architecture.md - Solution structure, RAG pipeline, DB schema, design patterns
│   ├── ArNir_KnowledgeBase.md - Full knowledge base for AI project context
│   ├── ArNir_KnowledgeBase.docx - Word version of knowledge base
│   ├── ArNir.md - File structure reference (this file)
│   ├── ArNir-Postman-Collection.json - All 12 API controllers with example requests
│   └── ArNir-Postman-Environment.json - baseUrl + adminUrl variables
│
├── ArNir.sln - Visual Studio solution file
│
└── .github/ (GitHub-specific configurations)
    ├── workflows/
    │   ├── build.yml - Build and test automation
    │   ├── deploy.yml - Deployment pipeline
    │   └── code-quality.yml - Code analysis checks
    └── ISSUE_TEMPLATE/
        ├── bug_report.md - Bug report template
        └── feature_request.md - Feature request template