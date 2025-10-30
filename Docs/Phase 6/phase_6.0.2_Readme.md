# 🚀 Phase 6.0.2 – GPT Commentary Engine (Kickoff)

## 📘 Overview
This phase integrates **OpenAI GPT-4o-mini** into the backend to generate **dynamic natural-language summaries** and **interactive insights** for analytics data.  
It introduces a new service (`ChatInsightService`) and endpoint (`/api/intelligence/chat`) that allows both:
- automated GPT summaries on the dashboard, and  
- ad-hoc natural-language queries about performance data (e.g., “Which model had the highest SLA last week?”).

## 🎯 Objectives
| # | Goal | Description |
|--:|------|-------------|
| 1 | LLM Integration | Connect OpenAI GPT-4o-mini for insight generation. |
| 2 | Prompt Template Design | Define structured prompts for analytics summarization and Q&A. |
| 3 | Backend Service Layer | Add `ChatInsightService` implementing GPT logic. |
| 4 | API Endpoint | Expose `/api/intelligence/chat` for real-time commentary. |
| 5 | Mock + Live Modes | Enable mock output for offline/local testing. |

## 🧩 Key Components
### 📁 Project Structure Additions
```
/AirNir
├── Library
│   └── ArNir.Services.AI
│       ├── ChatInsightService.cs
│       └── Interfaces/IChatInsightService.cs
│
├── Presentation
│   └── ArNir.Api
│       └── Controllers/
│           └── IntelligenceChatController.cs
└── docs
    └── Phase6.0.2_Architecture.png
```

## ⚙️ Backend Implementation
### 🧠 `ChatInsightService.cs`
```csharp
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using ArNir.Services.Interfaces;

namespace ArNir.Services.AI
{
    public class ChatInsightService : IChatInsightService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly string _model;

        public ChatInsightService(HttpClient httpClient, IConfiguration config)
        {
            _httpClient = httpClient;
            _apiKey = config["OpenAI:ApiKey"] ?? string.Empty;
            _model = config["OpenAI:Model"] ?? "gpt-4o-mini";
        }

        public async Task<string> GenerateInsightAsync(string userPrompt)
        {
            var body = new
            {
                model = _model,
                messages = new[]
                {
                    new { role = "system", content = "You are an AI analytics assistant summarizing RAG provider performance." },
                    new { role = "user", content = userPrompt }
                }
            };

            using var req = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/chat/completions");
            req.Headers.Add("Authorization", $"Bearer {_apiKey}");
            req.Content = JsonContent.Create(body);

            var res = await _httpClient.SendAsync(req);
            var json = await res.Content.ReadAsStringAsync();

            using var doc = JsonDocument.Parse(json);
            return doc.RootElement
                      .GetProperty("choices")[0]
                      .GetProperty("message")
                      .GetProperty("content")
                      .GetString() ?? "(No response)";
        }
    }
}
```

### 💬 Interface: `IChatInsightService.cs`
```csharp
using System.Threading.Tasks;

namespace ArNir.Services.Interfaces
{
    public interface IChatInsightService
    {
        Task<string> GenerateInsightAsync(string userPrompt);
    }
}
```

### 🌐 Controller: `IntelligenceChatController.cs`
```csharp
using Microsoft.AspNetCore.Mvc;
using ArNir.Services.Interfaces;

namespace ArNir.Api.Controllers
{
    [ApiController]
    [Route("api/intelligence/chat")]
    public class IntelligenceChatController : ControllerBase
    {
        private readonly IChatInsightService _chatService;

        public IntelligenceChatController(IChatInsightService chatService)
        {
            _chatService = chatService;
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] ChatRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Prompt))
                return BadRequest("Prompt cannot be empty.");

            var response = await _chatService.GenerateInsightAsync(request.Prompt);
            return Ok(new { insight = response });
        }
    }

    public class ChatRequest
    {
        public string Prompt { get; set; } = string.Empty;
    }
}
```

## 🔗 Example Request / Response
### Request
```json
POST /api/intelligence/chat
{
  "prompt": "Summarize performance of all providers in the past 7 days."
}
```

### Response
```json
{
  "insight": "OpenAI maintained top SLA performance (98%), Gemini showed stable latency, and Claude improved by 4% compared to last week."
}
```

## 🧩 Configuration
### `appsettings.json`
```json
"OpenAI": {
  "ApiKey": "YOUR_API_KEY_HERE",
  "Model": "gpt-4o-mini"
}
```

### Dependency Injection
```csharp
builder.Services.AddHttpClient<IChatInsightService, ChatInsightService>();
```

## 🧪 Testing Modes
| Mode | Behavior |
|------|-----------|
| **Mock Mode** | Returns fixed string summary for offline/local testing. |
| **Live Mode** | Calls OpenAI API using provided API key. |

## ✅ Outcome
- Introduced **ChatInsightService** for GPT-based commentary.  
- New **`/api/intelligence/chat`** endpoint for LLM insights.  
- Backend ready for integration with frontend “Insight Chat Panel”.  
- Enables both **automated summaries** and **on-demand analytics queries**.  

## 🔮 Next Phase – 6.0.3: Intelligence Dashboard MVP
| Focus | Deliverables |
|--------|---------------|
| **Frontend Integration** | React Intelligence Dashboard (KPI + Charts + GPT Panel). |
| **Data Binding** | Consume `/api/intelligence/dashboard` + `/api/intelligence/chat`. |
| **Realtime Updates** | GPT commentary refresh on filter changes. |
