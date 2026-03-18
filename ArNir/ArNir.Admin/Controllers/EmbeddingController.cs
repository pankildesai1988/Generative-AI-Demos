using ArNir.Admin.Models;
using ArNir.Core.DTOs.Embeddings;
using ArNir.Data;
using ArNir.RAG.Hosting;
using ArNir.RAG.Models;
using ArNir.Services.Interfaces;
using ArNir.Services.Provider;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pgvector;
using Pgvector.EntityFrameworkCore;

namespace ArNir.Admin.Controllers
{
    /// <summary>
    /// Admin panel for managing embeddings: statistics, bulk rebuild, and per-model deletion.
    /// </summary>
    [Authorize]
    public class EmbeddingController : Controller
    {
        private readonly IEmbeddingService _embeddingService;
        private readonly IEmbeddingProvider _embeddingProvider;
        private readonly IDbContextFactory<VectorDbContext> _pgFactory;
        private readonly IDbContextFactory<ArNirDbContext> _sqlFactory;
        private readonly IngestionQueue _ingestionQueue;
        private readonly IDocumentService _documentService;

        /// <summary>Initialises the controller with all required services.</summary>
        public EmbeddingController(
            IEmbeddingService embeddingService,
            IEmbeddingProvider embeddingProvider,
            IDbContextFactory<VectorDbContext> pgFactory,
            IDbContextFactory<ArNirDbContext> sqlFactory,
            IngestionQueue ingestionQueue,
            IDocumentService documentService)
        {
            _embeddingService = embeddingService;
            _embeddingProvider = embeddingProvider;
            _pgFactory = pgFactory;
            _sqlFactory = sqlFactory;
            _ingestionQueue = ingestionQueue;
            _documentService = documentService;
        }

        /// <summary>Renders the Embeddings dashboard with live statistics.</summary>
        public async Task<IActionResult> Index()
        {
            var vm = new EmbeddingStatsViewModel();
            try
            {
                await using var pgCtx = await _pgFactory.CreateDbContextAsync();
                vm.TotalEmbeddings = await pgCtx.Embeddings.LongCountAsync();
                vm.ByModel = await pgCtx.Embeddings
                    .GroupBy(e => e.Model)
                    .Select(g => new ModelStatEntry { Model = g.Key, Count = g.LongCount() })
                    .ToListAsync();
                vm.OldestCreatedAt = await pgCtx.Embeddings
                    .OrderBy(e => e.CreatedAt)
                    .Select(e => (DateTime?)e.CreatedAt)
                    .FirstOrDefaultAsync();
                vm.NewestCreatedAt = await pgCtx.Embeddings
                    .OrderByDescending(e => e.CreatedAt)
                    .Select(e => (DateTime?)e.CreatedAt)
                    .FirstOrDefaultAsync();
            }
            catch { /* PG unavailable — leave defaults */ }

            try
            {
                await using var sqlCtx = await _sqlFactory.CreateDbContextAsync();
                vm.TotalDocuments = await sqlCtx.Documents.CountAsync();
            }
            catch { /* SQL unavailable — leave 0 */ }

            return View(vm);
        }

        /// <summary>Generates embeddings for a specific document (legacy action).</summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Generate(int documentId)
        {
            var result = await _embeddingService.GenerateForDocumentAsync(
                new EmbeddingRequestDto { DocumentId = documentId }
            );

            TempData["Message"] = $"Generated {result.Count} embeddings!";
            return RedirectToAction("Index");
        }

        /// <summary>Renders the similarity-search test page.</summary>
        public IActionResult Test() => View();

        /// <summary>Runs a similarity search against the vector store for the supplied input text.</summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Test(string inputText)
        {
            if (string.IsNullOrWhiteSpace(inputText))
            {
                TempData["Error"] = "Please enter some text.";
                return RedirectToAction("Test");
            }

            var vectorArray = await _embeddingProvider.GenerateEmbeddingAsync(inputText, "text-embedding-ada-002");
            var queryVector = new Vector(vectorArray);

            await using var pgCtx = await _pgFactory.CreateDbContextAsync();
            var results = await pgCtx.Embeddings
                .OrderBy(e => e.Vector.L2Distance(queryVector))
                .Take(5)
                .ToListAsync();

            ViewBag.Query = inputText;
            ViewBag.Results = results;

            return View();
        }

        // GET /Embedding/Stats
        /// <summary>Returns embedding statistics as JSON (total, byModel, oldest, newest).</summary>
        [HttpGet]
        public async Task<IActionResult> Stats()
        {
            long total = 0;
            List<ModelStatEntry> byModel = new();
            DateTime? oldest = null;
            DateTime? newest = null;

            try
            {
                await using var pgCtx = await _pgFactory.CreateDbContextAsync();
                total = await pgCtx.Embeddings.LongCountAsync();
                byModel = await pgCtx.Embeddings
                    .GroupBy(e => e.Model)
                    .Select(g => new ModelStatEntry { Model = g.Key, Count = g.LongCount() })
                    .ToListAsync();
                oldest = await pgCtx.Embeddings
                    .OrderBy(e => e.CreatedAt)
                    .Select(e => (DateTime?)e.CreatedAt)
                    .FirstOrDefaultAsync();
                newest = await pgCtx.Embeddings
                    .OrderByDescending(e => e.CreatedAt)
                    .Select(e => (DateTime?)e.CreatedAt)
                    .FirstOrDefaultAsync();
            }
            catch { /* PG unavailable */ }

            return Json(new
            {
                total,
                byModel = byModel.Select(m => new { m.Model, m.Count }),
                oldest = oldest?.ToString("yyyy-MM-dd HH:mm:ss"),
                newest = newest?.ToString("yyyy-MM-dd HH:mm:ss")
            });
        }

        // POST /Embedding/RebuildAll
        /// <summary>Enqueues all documents for re-embedding via the background ingestion queue.</summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RebuildAll()
        {
            var docs = await _documentService.GetAllDocumentsAsync();
            int queued = 0;
            foreach (var doc in docs)
            {
                var req = new IngestionJobRequest(
                    new IngestionRequest
                    {
                        FileName      = doc.Name,
                        ContentType   = "text/plain",
                        UploadedBy    = "EmbeddingRebuildAll",
                        EmbeddingModel = "text-embedding-ada-002",
                        LegacySqlDocumentId = doc.Id
                    },
                    doc.Name,
                    DateTime.UtcNow);

                await _ingestionQueue.EnqueueAsync(req);
                queued++;
            }

            return Json(new { queued });
        }

        // POST /Embedding/DeleteByModel
        /// <summary>Deletes all embeddings for the specified model name.</summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteByModel(string model)
        {
            if (string.IsNullOrWhiteSpace(model))
                return Json(new { deleted = 0 });

            await using var pgCtx = await _pgFactory.CreateDbContextAsync();
            var toDelete = pgCtx.Embeddings.Where(e => e.Model == model);
            int count = await toDelete.CountAsync();
            pgCtx.Embeddings.RemoveRange(toDelete);
            await pgCtx.SaveChangesAsync();

            return Json(new { deleted = count });
        }
    }
}
