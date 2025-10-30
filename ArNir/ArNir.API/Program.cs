using ArNir.Data;
using ArNir.Data.Repositories;
using ArNir.Services;
using ArNir.Services.AI;
using ArNir.Services.Interfaces;
using ArNir.Services.Provider;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

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
builder.Services.AddScoped<IAIInsightService, AIInsightService>();
builder.Services.AddScoped<IExportHistoryService, ExportHistoryService>();
builder.Services.AddScoped<IIntelligenceService, IntelligenceService>();
builder.Services.AddScoped<ILlmService, OpenAiService>();
// ------------------------------------------------------
// NLP
// ------------------------------------------------------
builder.Services.AddScoped<IInsightHistoryService, InsightHistoryService>();
builder.Services.AddScoped<INaturalLanguageCommandService, NaturalLanguageCommandService>();

// ------------------------------------------------------
// CORS FOR FRONTEND
// ------------------------------------------------------
builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendPolicy", policy =>
    {
        policy.WithOrigins(
            "https://airnir-frontend.azurewebsites.net",
            "http://localhost:5173", // for local dev
            "http://localhost:3000"  // optional React port
        )
        .AllowAnyHeader()
        .AllowAnyMethod();
    });
});


builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("FrontendPolicy");

app.MapControllers();

app.Run();
