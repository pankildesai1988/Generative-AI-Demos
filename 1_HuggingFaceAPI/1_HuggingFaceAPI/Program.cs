using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.SwaggerUI;
using System.Text;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

string apiKey = Environment.GetEnvironmentVariable("HF_API_KEY");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();


// Summarization
app.MapPost("/summarize", async ([FromBody] InputData input) =>
{
    string modelUrl = "https://api-inference.huggingface.co/models/facebook/bart-large-cnn";
    return Results.Content(await QueryModel(modelUrl, input.Text), "application/json");
});

// Translation (English → Hindi)
app.MapPost("/translate", async ([FromBody] InputData input) =>
{
    string modelUrl = "https://api-inference.huggingface.co/models/Helsinki-NLP/opus-mt-en-hi";
    return Results.Content(await QueryModel(modelUrl, input.Text), "application/json");
});

// Sentiment Analysis
app.MapPost("/sentiment", async ([FromBody] InputData input) =>
{
    string modelUrl = "https://api-inference.huggingface.co/models/finiteautomata/bertweet-base-sentiment-analysis";
    return Results.Content(await QueryModel(modelUrl, input.Text), "application/json");
});

app.MapGet("/", (Delegate)(() => "✅ Hugging Face .NET API is running! Go to /swagger to test."));


app.Run();
async Task<string> QueryModel(string modelUrl, string inputText)
{
    string str;
    using (HttpClient client = new HttpClient())
    {
        client.DefaultRequestHeaders.Add("Authorization", "Bearer " + apiKey);
        str = await (await client.PostAsync(modelUrl, (HttpContent)new StringContent(JsonSerializer.Serialize(new
        {
            inputs = inputText
        }), Encoding.UTF8, "application/json"))).Content.ReadAsStringAsync();
    }
    return str;
}
public record InputData(string Text);