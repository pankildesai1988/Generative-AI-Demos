using ArNir.Admin.Models;
using ArNir.Data;
using ArNir.RAG.Interfaces;
using ArNir.RAG.Models;
using ArNir.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ArNir.Admin.Controllers;

[Authorize]
public class VectorStoreController : Controller
{
    private readonly IDbContextFactory<ArNirDbContext> _sqlFactory;
    private readonly IDbContextFactory<VectorDbContext> _pgFactory;
    private readonly IIngestionPipeline _pipeline;
    private readonly IDocumentService _documentService;
    private readonly ILogger<VectorStoreController> _logger;

    public VectorStoreController(
        IDbContextFactory<ArNirDbContext> sqlFactory,
        IDbContextFactory<VectorDbContext> pgFactory,
        IIngestionPipeline pipeline,
        IDocumentService documentService,
        ILogger<VectorStoreController> logger)
    {
        _sqlFactory = sqlFactory;
        _pgFactory = pgFactory;
        _pipeline = pipeline;
        _documentService = documentService;
        _logger = logger;
    }

    // GET /VectorStore
    public async Task<IActionResult> Index()
    {
        var vm = new VectorStoreViewModel();

        try
        {
            // PostgreSQL stats
            await using var pgCtx = await _pgFactory.CreateDbContextAsync();

            vm.TotalEmbeddings = await pgCtx.Embeddings.LongCountAsync();

            vm.EmbeddingsByModel = await pgCtx.Embeddings
                .GroupBy(e => e.Model)
                .Select(g => new ModelEmbeddingCount { Model = g.Key, Count = g.LongCount() })
                .ToListAsync();

            vm.LastIndexedAt = await pgCtx.Embeddings
                .OrderByDescending(e => e.CreatedAt)
                .Select(e => (DateTime?)e.CreatedAt)
                .FirstOrDefaultAsync();

            // Find orphaned documents (chunks with no embeddings)
            await using var sqlCtx = await _sqlFactory.CreateDbContextAsync();

            // Get all chunk IDs from SQL Server
            var allChunkIds = await sqlCtx.DocumentChunks
                .Select(c => c.Id)
                .ToListAsync();

            // Get all embedded chunk IDs from PostgreSQL
            var embeddedChunkIds = await pgCtx.Embeddings
                .Select(e => e.ChunkId)
                .Distinct()
                .ToListAsync();

            var embeddedSet = embeddedChunkIds.ToHashSet();

            // Find orphan chunk IDs (in SQL but not in pgvector)
            var orphanChunkIds = allChunkIds.Where(id => !embeddedSet.Contains(id)).ToList();

            if (orphanChunkIds.Any())
            {
                // Group orphan chunks by document
                var orphanChunks = await sqlCtx.DocumentChunks
                    .Where(c => orphanChunkIds.Contains(c.Id))
                    .Include(c => c.Document)
                    .ToListAsync();

                vm.OrphanedDocuments = orphanChunks
                    .GroupBy(c => c.DocumentId)
                    .Select(g => new OrphanedDocument
                    {
                        DocumentId = g.Key,
                        DocumentName = g.First().Document?.Name ?? $"Document #{g.Key}",
                        MissingChunks = g.Count()
                    })
                    .ToList();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading VectorStore health data.");
            TempData["Error"] = "Could not load vector store data: " + ex.Message;
        }

        return View(vm);
    }

    // POST /VectorStore/RebuildForDocument
    [HttpPost]
    public async Task<IActionResult> RebuildForDocument(int documentId)
    {
        try
        {
            await using var sqlCtx = await _sqlFactory.CreateDbContextAsync();

            var document = await sqlCtx.Documents
                .Include(d => d.Chunks)
                .FirstOrDefaultAsync(d => d.Id == documentId);

            if (document == null)
            {
                TempData["Error"] = $"Document #{documentId} not found.";
                return RedirectToAction(nameof(Index));
            }

            // Delete existing embeddings for the document's chunks
            await using var pgCtx = await _pgFactory.CreateDbContextAsync();
            var chunkIds = document.Chunks.Select(c => c.Id).ToList();
            var existingEmbeddings = pgCtx.Embeddings.Where(e => chunkIds.Contains(e.ChunkId));
            pgCtx.Embeddings.RemoveRange(existingEmbeddings);
            await pgCtx.SaveChangesAsync();

            // Re-run ingestion via pipeline
            // Build a plain-text representation of the document chunks
            var text = string.Join("\n\n", document.Chunks.OrderBy(c => c.ChunkOrder).Select(c => c.Text));
            var ms = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(text));

            var ingestionRequest = new IngestionRequest
            {
                FileStream = ms,
                FileName = document.Name,
                ContentType = "text/plain",
                UploadedBy = "VectorStoreRebuild",
                EmbeddingModel = "text-embedding-ada-002",
                LegacySqlDocumentId = document.Id
            };

            var result = await _pipeline.IngestAsync(ingestionRequest);

            if (result.Success)
            {
                TempData["Success"] = $"Embeddings rebuilt for '{document.Name}' — {result.ChunksCreated} chunks, {result.EmbeddingsCreated} embeddings.";
                _logger.LogInformation("Rebuilt embeddings for document '{Name}' (Id={Id}).", document.Name, documentId);
            }
            else
            {
                TempData["Error"] = $"Rebuild failed for '{document.Name}': {result.ErrorMessage}";
                _logger.LogWarning("Embedding rebuild failed for document '{Name}': {Error}", document.Name, result.ErrorMessage);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rebuilding embeddings for document {Id}.", documentId);
            TempData["Error"] = "Error rebuilding embeddings: " + ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }
}
