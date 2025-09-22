using ArNir.Core.Config;
using ArNir.Data;
using ArNir.Data.Repositories;
using ArNir.Services;
using ArNir.Services.Interfaces;
using ArNir.Services.Mapping;
using ArNir.Services.Provider;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<FileUploadSettings>(
    builder.Configuration.GetSection("FileUploadSettings"));

// MVC support
builder.Services.AddControllersWithViews();
//.AddRazorRuntimeCompilation();

// Add DbContext + Services
// Add SQL Server DbContext (Documents + Chunks)
builder.Services.AddDbContextFactory<ArNirDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("SqlServer")));

// Postgres + pgvector (Phase 3.2)
builder.Services.AddDbContextFactory<VectorDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Postgres"),
        npgsqlOptions => npgsqlOptions.MigrationsAssembly("ArNir.Data")  // ✅ migrations in ArNir.Data
        .UseVector()));

// Register Embedding Provider (OpenAI)
builder.Services.AddHttpClient<IEmbeddingProvider, OpenAiEmbeddingProvider>();

// Register Embedding Service
builder.Services.AddScoped<IEmbeddingService, EmbeddingService>();

builder.Services.AddScoped<IDocumentService, DocumentService>();

builder.Services.AddScoped<IRetrievalService, RetrievalService>();

builder.Services.AddScoped<IOpenAiService, OpenAiService>();
builder.Services.AddScoped<IRagService, RagService>();

builder.Services.AddScoped<IRagHistoryRepository, RagHistoryRepository>();
builder.Services.AddScoped<IRagHistoryService, RagHistoryService>();

// ✅ Register AutoMapper
builder.Services.AddAutoMapper(typeof(MappingProfile));

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

//app.MapControllerRoute(
//    name: "areas",
//    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
