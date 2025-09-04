using _2_OpenAIChatFrontend.Helper;
using _2_OpenAIChatFrontend.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Text;
using System.Text.Json;

namespace _2_OpenAIChatFrontend.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly HttpClient _httpClient;
        public HomeController(ILogger<HomeController> logger, IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _httpClient = httpClientFactory.CreateClient();
            _httpClient.BaseAddress = new Uri("http://localhost:5000"); // Backend API URL
        }

        public IActionResult Index()
        {
            if (HttpContext.Session.Get<List<ChatMessage>>("ChatHistory") == null)
            {
                HttpContext.Session.Set("ChatHistory", new List<ChatMessage>());
            }
            return View(HttpContext.Session.Get<List<ChatMessage>>("ChatHistory"));
        }


        [HttpPost]
        public async Task<IActionResult> Chat(string message)
        {
            var chatHistory = HttpContext.Session.Get<List<ChatMessage>>("ChatHistory") ?? new List<ChatMessage>();

            // Add user message to history
            chatHistory.Add(new ChatMessage { Role = "user", Content = message });

            // Call backend API
            var payload = JsonSerializer.Serialize(new { Message = message });
            var content = new StringContent(payload, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("/api/chat/send", content);
            var responseString = await response.Content.ReadAsStringAsync();

            // Add AI response to history
            chatHistory.Add(new ChatMessage { Role = "assistant", Content = responseString });

            HttpContext.Session.Set("ChatHistory", chatHistory);

            return View("Index", chatHistory);
        }

        public IActionResult Chat()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> ChatMessage([FromBody] ChatRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Message))
                return BadRequest("Message cannot be empty");

            var payload = JsonSerializer.Serialize(new { request.Message });
            var content = new StringContent(payload, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("/api/chat/send", content);
            var responseString = await response.Content.ReadAsStringAsync();

            return Json(new { user = request.Message, ai = responseString });
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
