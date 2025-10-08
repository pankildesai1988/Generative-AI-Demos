using ArNir.Core.DTOs.Embeddings;
using ArNir.Data;
using ArNir.Services.Interfaces;
using ArNir.Services.Provider;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pgvector;
using Pgvector.EntityFrameworkCore;

namespace ArNir.Admin.Controllers
{
    public class EmbeddingController : Controller
    {
        private readonly IEmbeddingService _embeddingService;
        private readonly IEmbeddingProvider _embeddingProvider;
        private readonly VectorDbContext _vectorContext;

        public EmbeddingController(IEmbeddingService embeddingService,
            IEmbeddingProvider embeddingProvider,
            VectorDbContext vectorContext)
        {
            _embeddingService = embeddingService;
            _embeddingProvider = embeddingProvider;
            _vectorContext = vectorContext;
        }

        public IActionResult Index() => View();

        [HttpPost]
        public async Task<IActionResult> Generate(int documentId)
        {
            var result = await _embeddingService.GenerateForDocumentAsync(
                new EmbeddingRequestDto { DocumentId = documentId }
            );

            TempData["Message"] = $"Generated {result.Count} embeddings!";
            return RedirectToAction("Index");
        }

        public IActionResult Test() => View();

        [HttpPost]
        public async Task<IActionResult> Test(string inputText)
        {
            if (string.IsNullOrWhiteSpace(inputText))
            {
                TempData["Error"] = "Please enter some text.";
                return RedirectToAction("Test");
            }

            // ✅ Generate embedding via OpenAI
            var vectorArray = await _embeddingProvider.GenerateEmbeddingAsync(inputText, "text-embedding-ada-002");
            var queryVector = new Vector(vectorArray);

            // ✅ Run similarity search in Postgres
            var results = await _vectorContext.Embeddings
                .OrderBy(e => e.Vector.L2Distance(queryVector)) // nearest neighbors
                .Take(5)
                .ToListAsync();

            ViewBag.Query = inputText;
            ViewBag.Results = results;

            return View();
        }
    }
}
