using _2_OpenAIChatDemo.Data;
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
var allowedOrigins = builder.Configuration.GetValue<string>($"AllowedOrigins:{env}")?
    .Split(';', StringSplitOptions.RemoveEmptyEntries);

// Add services to the container.

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
