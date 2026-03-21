using ArNir.Agents.DependencyInjection;
using ArNir.Core.Config;
using ArNir.Data;
using ArNir.Data.Repositories;
using ArNir.Memory.DependencyInjection;
using ArNir.Observability.DependencyInjection;
using ArNir.Observability.Interfaces;
using ArNir.PromptEngine.DependencyInjection;
using ArNir.PromptEngine.Interfaces;
using ArNir.PromptEngine.Resolution;
using ArNir.RAG.DependencyInjection;
using ArNir.RAG.Hosting;
using ArNir.RAG.Pgvector.DependencyInjection;
using ArNir.Services;
using ArNir.Services.AI;
using ArNir.Services.AI.Interfaces;
using ArNir.Services.Interfaces;
using ArNir.Services.Mapping;
using ArNir.Services.Provider;
using ArNir.Tools.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<FileUploadSettings>(
    builder.Configuration.GetSection("FileUploadSettings"));    

// ✅ Configure QuestPDF (Community License)
QuestPDF.Settings.License = LicenseType.Community;

// Add services to the container.
// --- Services ---
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// --- DB Contexts ---
builder.Services.AddDbContextFactory<ArNirDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("SqlServer")));

// Postgres + pgvector (Phase 3.2)
builder.Services.AddDbContextFactory<VectorDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Postgres"),
        npgsqlOptions => npgsqlOptions.MigrationsAssembly("ArNir.Data")  // ✅ migrations in ArNir.Data
        .UseVector()));


// ------------------------------------------------------
// REPOSITORIES
// ------------------------------------------------------
builder.Services.AddScoped<IRagHistoryRepository, RagHistoryRepository>();

// ------------------------------------------------------
// PROVIDERS
// ------------------------------------------------------
builder.Services.AddHttpClient<IEmbeddingProvider, OpenAiEmbeddingProvider>(); // or your actual provider class

// ------------------------------------------------------
// BUSINESS SERVICES
// ------------------------------------------------------
builder.Services.AddScoped<IDocumentService, DocumentService>();
builder.Services.AddScoped<IRagService, RagService>();
builder.Services.AddScoped<IRetrievalService, RetrievalService>();
builder.Services.AddScoped<IRagHistoryService, RagHistoryService>();
builder.Services.AddScoped<IEmbeddingService, EmbeddingService>();
builder.Services.AddScoped<IFeedbackService, FeedbackService>();

// If you use LLM providers (OpenAI, Gemini, Claude)
builder.Services.AddScoped<OpenAiService>();
builder.Services.AddScoped<GeminiService>();
builder.Services.AddScoped<ClaudeService>();

// ------------------------------------------------------
// Insight Generation, Anomaly Detection, Predictive Modeling
// ------------------------------------------------------
builder.Services.AddHttpClient<ArNir.Services.AI.InsightEngineService>();
builder.Services.AddSingleton<AnomalyDetectionService>();
builder.Services.AddHttpClient<PredictiveModelService>();
builder.Services.AddHttpClient<NarrativeReportService>();
// ------------------------------------------------------
// Unified Intelligence Dashboard Services
// ------------------------------------------------------
builder.Services.AddScoped<IAnalyticsService, AnalyticsService>();
builder.Services.AddScoped<IInsightEngineService, ArNir.Services.InsightEngineService>();
builder.Services.AddScoped<IPredictiveTrendService, PredictiveTrendService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IIntelligenceService, IntelligenceService>();
builder.Services.AddHttpClient<IChatInsightService, ChatInsightService>();
builder.Services.AddScoped<IExportService, ExportService>();
// ------------------------------------------------------
// Predictive AI Insights & Export Analytics History
// ------------------------------------------------------
#pragma warning disable CS0618 // IAIInsightService kept for migration bridge — remove when IAIInsightGenerator is fully adopted
builder.Services.AddScoped<IAIInsightService, AIInsightService>();
#pragma warning restore CS0618
builder.Services.AddScoped<IExportHistoryService, ExportHistoryService>();
builder.Services.AddScoped<IIntelligenceService, IntelligenceService>();
builder.Services.AddScoped<ILlmService, OpenAiService>();
// Sprint 6 — Evaluation Layer (LLM-as-judge)
builder.Services.AddScoped<ArNir.Observability.Interfaces.IEvaluationService, ArNir.Services.LlmEvaluationService>();
builder.Services.AddScoped<ArNir.Services.Interfaces.IEvaluationHistoryService, ArNir.Services.EvaluationHistoryService>();
// ------------------------------------------------------
// NLP
// ------------------------------------------------------
builder.Services.AddScoped<IInsightHistoryService, InsightHistoryService>();
builder.Services.AddScoped<INaturalLanguageCommandService, NaturalLanguageCommandService>();
// ------------------------------------------------------
// Phase 7.0 – Advanced AI NLP & Contextual Chat Intelligence
// ------------------------------------------------------
builder.Services.AddScoped<IChatEmbeddingService, ChatEmbeddingService>();
builder.Services.AddScoped<IChatInsightService, ChatInsightService>();
builder.Services.AddScoped<IContextMemoryService, ContextMemoryService>();
builder.Services.AddScoped<INaturalQueryService, NaturalQueryService>();
builder.Services.AddScoped<IVisualizationService, VisualizationService>();
builder.Services.AddScoped<IActionEngineService, ActionEngineService>();

// ------------------------------------------------------
// Phase 9 — new module registrations
// ------------------------------------------------------
builder.Services.AddArNirMemory();         // IEpisodicMemory + ISemanticMemory (NullSemanticMemory dev stub)
builder.Services.AddArNirPromptEngine();   // IPromptResolver (CodePromptResolver — 3-layer chain)
builder.Services.AddArNirAgents();         // IToolRegistry + IPlannerAgent
builder.Services.AddArNirTools();          // DocumentLookupTool + WebFetchTool
builder.Services.AddArNirObservability();  // SlaAlertRule (5 000 ms default)
builder.Services.AddArNirRAG();            // IIngestionPipeline + parsers + chunker + null stubs
builder.Services.AddArNirRagPgvector();              // real pgvector (overrides null stubs)
builder.Services.AddArNirRAGBackgroundIngestion();    // IngestionQueue + IngestionWorker
builder.Services.AddScoped<ArNir.Core.Interfaces.IEmbeddingProvider>(sp =>
    (ArNir.Core.Interfaces.IEmbeddingProvider)sp.GetRequiredService<IEmbeddingProvider>());

// ------------------------------------------------------
// Phase 10 — upgrade to DB-backed implementations
// ------------------------------------------------------
builder.Services.AddMemoryCache();
builder.Services.AddScoped<IPlatformSettingsService, PlatformSettingsService>();
builder.Services.AddScoped<IPromptVersionStore,      DbPromptVersionStore>();
builder.Services.AddScoped<IMetricCollector,         DbMetricCollector>();
// LayeredPromptResolver replaces the Singleton CodePromptResolver registered by AddArNirPromptEngine()
builder.Services.AddScoped<IPromptResolver,          LayeredPromptResolver>();

// ------------------------------------------------------
// CORS FOR FRONTEND
// ------------------------------------------------------
builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendPolicy", policy =>
    {
        policy.WithOrigins(
            "https://genaiadmin.empiricaledge.site",
            "https://genai.empiricaledge.site",
            "https://www.genai.empiricaledge.site",
            "http://localhost:5173", // for local dev
            "http://localhost:3000", // optional React port
            "http://localhost:3001", // healthcare demo
            "http://localhost:3002", // ecommerce demo
            "http://localhost:3003"  // finance demo
        )
        .AllowAnyHeader()
        .AllowAnyMethod();
    });
});

builder.Services.AddAutoMapper(typeof(Program).Assembly);
builder.Services.AddAutoMapper(typeof(MappingProfile).Assembly);

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
//    app.UseSwagger();
//    app.UseSwaggerUI();
//}

// Always enable Swagger (both Dev & Prod)
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "ArNir API v1");
    c.RoutePrefix = "swagger";
});

app.UseHttpsRedirection();

app.UseCors("FrontendPolicy");

app.MapControllers();

app.Run();
