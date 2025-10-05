using _2_OpenAIChatDemo.Data;
using _2_OpenAIChatDemo.Filters;
using _2_OpenAIChatDemo.LLMProviders;
using _2_OpenAIChatDemo.Services;
using _2_OpenAIChatDemo.Settings;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// ✅ Get connection string from Azure App Service Configuration
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<ChatDbContext>(options =>
    options.UseSqlServer(connectionString));

// ✅ Pick AllowedOrigins based on environment
string env = builder.Environment.EnvironmentName;
var allowedOriginsConfig = builder.Configuration.GetValue<string>($"AllowedOrigins:{env}");
var allowedOrigins = allowedOriginsConfig?.Split(';', StringSplitOptions.RemoveEmptyEntries) 
    ?? new[] { "https://openai-frontend-g7cfetakc8bxagfa.centralus-01.azurewebsites.net" };


// ✅ Existing services
builder.Services.AddControllers();
builder.Services.AddHttpClient();
builder.Services.Configure<OpenAISettings>(builder.Configuration.GetSection("OpenAI"));

// ✅ Core services
builder.Services.AddScoped<IChatHistoryService, ChatHistoryService>();
builder.Services.AddScoped<IPromptTemplateService, PromptTemplateService>();
builder.Services.AddScoped<IOpenAiService, OpenAiService>();

//Authentication services
builder.Services.AddScoped<IAdminAuthService, AdminAuthService>();


// ✅ New providers
builder.Services.AddScoped<ILlmProvider, OpenAiProvider>();
builder.Services.AddScoped<ILlmProvider, ClaudeProvider>();
builder.Services.AddScoped<ILlmProvider, GeminiProvider>();

// ✅ Comparison service
builder.Services.AddScoped<IComparisonService, ComparisonService>();

// Document and Chunking services
builder.Services.AddScoped<IDocumentService, DocumentService>();
builder.Services.AddScoped<IChunkingService, ChunkingService>();

// JWT Auth
var jwtSettings = builder.Configuration.GetSection("Jwt");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(jwtSettings["Key"]))
    };
});

// Add Controllers + HttpClient factory
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    });
builder.Services.AddHttpClient();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Generative AI API",
        Version = "v1",
        Description = "API for Document Ingestion & Chunking (Phase 3.1)"
    });

    c.OperationFilter<FileUploadOperationFilter>(); // your custom filter
});

builder.Services.Configure<OpenAISettings>(builder.Configuration.GetSection("OpenAI"));

// ✅ Configurable CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        if (allowedOrigins != null && allowedOrigins.Length > 0)
        {
            policy.WithOrigins(allowedOrigins)
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        }
        else
        {
            // fallback: block everything (safer than AllowAnyOrigin)
            policy.DisallowCredentials();
        }
    });
});



builder.Services.AddApplicationInsightsTelemetry();
builder.Services.AddMemoryCache();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        //c.SwaggerEndpoint("/swagger/v1/swagger.json", "Generative AI API v1");
        //c.RoutePrefix = string.Empty; // Swagger UI at root URL
    });
}

app.MapGet("/", () => "✅ OpenAI .NET API Demo is running...");

app.UseHttpsRedirection();

// ✅ Use CORS
app.UseCors("AllowFrontend");

app.UseAuthentication();  // ✅ must come before Authorization

app.UseAuthorization();

app.MapControllers();

app.Run();


string HashPassword(string password)
{
    using var sha256 = SHA256.Create();
    var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
    return BitConverter.ToString(bytes).Replace("-", "").ToLower();
}