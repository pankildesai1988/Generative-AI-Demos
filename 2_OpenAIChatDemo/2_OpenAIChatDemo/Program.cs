using _2_OpenAIChatDemo.Data;
using _2_OpenAIChatDemo.Settings;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// ✅ Get connection string from Azure App Service Configuration
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<ChatDbContext>(options =>
    options.UseSqlServer(connectionString));

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

builder.Services.AddCors(options =>
{
    //options.AddPolicy("AllowFrontend",
    //    policy =>
    //    {
    //        policy.WithOrigins("https://localhost:7151") // frontend URL
    //              .AllowAnyHeader()
    //              .AllowAnyMethod();
    //    });
    options.AddPolicy("AllowFrontend",
        policy =>
        {
            policy.WithOrigins("https://openai-frontend-g7cfetakc8bxagfa.centralus-01.azurewebsites.net/") // frontend URL
                  .AllowAnyHeader()
                  .AllowAnyMethod();
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

app.UseAuthorization();

// ✅ Use CORS
app.UseCors("AllowFrontend");

app.MapControllers();

app.Run();
