using _2_OpenAIChatDemo.Data;
using _2_OpenAIChatDemo.Services;
using _2_OpenAIChatDemo.Settings;
using Microsoft.EntityFrameworkCore;
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
Console.WriteLine($"Running in {env}, AllowedOrigins = {string.Join(", ", allowedOrigins ?? Array.Empty<string>())}");


// Add services to the container.
builder.Services.AddScoped<IOpenAiService, OpenAiService>();
builder.Services.AddScoped<IChatHistoryService, ChatHistoryService>();
builder.Services.AddScoped<ITemplateService, TemplateService>();


// Add Controllers + HttpClient factory
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    });
builder.Services.AddHttpClient();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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
    app.UseSwaggerUI();
}

app.MapGet("/", () => "✅ OpenAI .NET API Demo is running...");

app.UseHttpsRedirection();

// ✅ Use CORS
app.UseCors("AllowFrontend");

app.UseAuthorization();

app.MapControllers();

app.Run();
