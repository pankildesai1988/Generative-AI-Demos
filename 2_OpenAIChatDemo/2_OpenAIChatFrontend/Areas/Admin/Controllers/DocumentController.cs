using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Text.Json;
using _2_OpenAIChatDemo.DTOs;

namespace _2_OpenAIChatDemo.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class DocumentController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonOptions;

        public DocumentController(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("BackendApi");
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
        }

        // 📄 List all documents
        public async Task<IActionResult> Index()
        {
            var response = await _httpClient.GetAsync("api/Document");
            if (!response.IsSuccessStatusCode) return View(new List<DocumentResponseDto>());

            var json = await response.Content.ReadAsStringAsync();
            var docs = JsonSerializer.Deserialize<List<DocumentResponseDto>>(json, _jsonOptions) ?? new List<DocumentResponseDto>();

            return View(docs);
        }

        // 📄 View document details
        public async Task<IActionResult> Details(int id)
        {
            var response = await _httpClient.GetAsync($"api/Document/{id}");
            if (!response.IsSuccessStatusCode) return NotFound();

            var json = await response.Content.ReadAsStringAsync();
            var doc = JsonSerializer.Deserialize<DocumentResponseDto>(json, _jsonOptions);

            return View(doc);
        }

        // 📄 Upload form
        [HttpGet]
        public IActionResult Upload() => View();

        // 📄 Upload document → forwards to backend API
        [HttpPost]
        public async Task<IActionResult> Upload(IFormFile file, string uploadedBy)
        {
            if (file == null || file.Length == 0)
            {
                TempData["Error"] = "Please select a valid file.";
                return RedirectToAction("Upload");
            }

            using var formData = new MultipartFormDataContent();
            using var stream = file.OpenReadStream();
            var fileContent = new StreamContent(stream);
            fileContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);

            formData.Add(fileContent, "file", file.FileName);
            formData.Add(new StringContent(uploadedBy ?? "admin"), "uploadedBy");

            var response = await _httpClient.PostAsync("api/Document/upload-file", formData);

            if (response.IsSuccessStatusCode)
            {
                TempData["Success"] = "File uploaded successfully!";
                return RedirectToAction("Index");
            }

            TempData["Error"] = "Upload failed.";
            return RedirectToAction("Upload");
        }
    }
}
