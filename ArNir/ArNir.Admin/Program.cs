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
using ArNir.RAG.Interfaces;
using ArNir.RAG.Pgvector.DependencyInjection;
using ArNir.Services;
using ArNir.Services.Interfaces;
using ArNir.Services.Mapping;
using ArNir.Services.Provider;
using ArNir.Tools.DependencyInjection;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<FileUploadSettings>(
    builder.Configuration.GetSection("FileUploadSettings"));

// MVC support
builder.Services.AddControllersWithViews();

// ── Cookie Authentication (Sprint 1) ─────────────────────────────────────
// Provides session-based login for the admin panel.
// app.UseAuthentication() is called BEFORE app.UseAuthorization() in the pipeline below.
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath         = "/Account/Login";
        options.AccessDeniedPath  = "/Account/AccessDenied";
        options.ExpireTimeSpan    = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
        options.Cookie.HttpOnly   = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    });

// Add DbContext + Services
// SQL Server DbContext (Documents + Chunks)
builder.Services.AddDbContextFactory<ArNirDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Postgres + pgvector
builder.Services.AddDbContextFactory<VectorDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Postgres"),
        npgsqlOptions => npgsqlOptions.MigrationsAssembly("ArNir.Data")
        .UseVector()));

// Register Embedding Provider (OpenAI) — ArNir.Services.Provider.IEmbeddingProvider
builder.Services.AddHttpClient<IEmbeddingProvider, OpenAiEmbeddingProvider>();

// Register Embedding Service
builder.Services.AddScoped<IEmbeddingService, EmbeddingService>();

builder.Services.AddScoped<IDocumentService, DocumentService>();
builder.Services.AddScoped<IRetrievalService, RetrievalService>();

// Register LLM Providers
builder.Services.AddScoped<OpenAiService>();
builder.Services.AddScoped<GeminiService>();
builder.Services.AddScoped<ClaudeService>();

// Register RAG service
builder.Services.AddScoped<IRagService, RagService>();

builder.Services.AddScoped<IRagHistoryRepository, RagHistoryRepository>();
builder.Services.AddScoped<IRagHistoryService, RagHistoryService>();

// Register AutoMapper
builder.Services.AddAutoMapper(typeof(MappingProfile));

// ── Phase 10 — module registrations ──────────────────────────────────────
builder.Services.AddArNirMemory();
builder.Services.AddArNirPromptEngine();
builder.Services.AddArNirAgents();
builder.Services.AddArNirTools();
builder.Services.AddArNirObservability();
// AddArNirRAG registers NullDocumentEmbedder + NullDocumentVectorStore (Singleton).
// The real Scoped implementations registered below take precedence (last registration wins).
builder.Services.AddArNirRAG();

// ── Sprint 1 — Replace null pipeline stubs with real PostgreSQL implementations ──
// PgvectorDocumentEmbedder  : calls OpenAI API via IEmbeddingProvider → returns float[]
// PgvectorDocumentVectorStore: resolves SQL FK then persists Embedding rows in pgvector
// Registered Scoped (depends on IDbContextFactory and IEmbeddingProvider, both Scoped).
builder.Services.AddArNirRagPgvector();

// Register ArNir.Core.Interfaces.IEmbeddingProvider (used by ArNir.RAG.Pgvector)
// by forwarding to the existing ArNir.Services.Provider.IEmbeddingProvider registration.
builder.Services.AddScoped<ArNir.Core.Interfaces.IEmbeddingProvider>(sp =>
    (ArNir.Core.Interfaces.IEmbeddingProvider)sp.GetRequiredService<IEmbeddingProvider>());

// Override to DB-backed implementations (LayeredPromptResolver, DbMetricCollector, etc.)
builder.Services.AddMemoryCache();
builder.Services.AddScoped<IPlatformSettingsService, PlatformSettingsService>();
builder.Services.AddScoped<IPromptVersionStore,      DbPromptVersionStore>();
builder.Services.AddScoped<IMetricCollector,         DbMetricCollector>();

// LayeredPromptResolver replaces the default CodePromptResolver
builder.Services.AddScoped<IPromptResolver, LayeredPromptResolver>();

// Sprint 2 — Background ingestion queue
builder.Services.AddArNirRAGBackgroundIngestion();

// Sprint 6 — Evaluation Layer (LLM-as-judge)
builder.Services.AddScoped<ILlmService, OpenAiService>();
builder.Services.AddScoped<ArNir.Observability.Interfaces.IEvaluationService, LlmEvaluationService>();
builder.Services.AddScoped<IEvaluationHistoryService, EvaluationHistoryService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Authentication MUST come before Authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
