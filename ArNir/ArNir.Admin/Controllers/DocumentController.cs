using ArNir.Core.DTOs.Documents;
using ArNir.RAG.Interfaces;
using ArNir.RAG.Models;
using ArNir.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ArNir.Admin.Controllers;

/// <summary>
/// Admin CRUD controller for documents.
/// <para>
/// The Upload action uses a <b>bridge pattern</b> to run both the legacy and new ingestion paths:
/// <list type="number">
///   <item>
///     <term>Path 1 — Legacy (SQL Server entity)</term>
///     <description>
///       <see cref="IDocumentService.UploadDocumentAsync"/> saves the <c>Documents</c> and
///       <c>DocumentChunks</c> rows in SQL Server via <c>ArNirDbContext</c>. Existing list/detail
///       views continue to read from this path unchanged.
///     </description>
///   </item>
///   <item>
///     <term>Path 2 — Modular RAG pipeline (<see cref="IIngestionPipeline"/>)</term>
///     <description>
///       The new Parse → Chunk → Embed → Store pipeline from <c>ArNir.RAG</c> is invoked in
///       parallel. In the development configuration the embedder and vector-store are no-op stubs
///       (<c>NullDocumentEmbedder</c>, <c>NullDocumentVectorStore</c>), so this is a safe dry-run
///       that logs chunk counts without touching external infrastructure. Swap the stubs for real
///       implementations (OpenAI embedder + pgvector store) when ready.
///     </description>
///   </item>
/// </list>
/// The ingestion result (chunk count) is surfaced in <c>TempData["IngestionResult"]</c> and
/// displayed as a banner on the redirect target (<c>Index</c>).
/// </para>
/// </summary>
public class DocumentController : Controller
{
    private readonly IDocumentService  _documentService;
    private readonly IIngestionPipeline _pipeline;
    private readonly ILogger<DocumentController> _logger;

    /// <summary>Initialises the controller with both the legacy service and the new pipeline.</summary>
    public DocumentController(
        IDocumentService documentService,
        IIngestionPipeline pipeline,
        ILogger<DocumentController> logger)
    {
        _documentService = documentService;
        _pipeline        = pipeline;
        _logger          = logger;
    }

    // GET /Document
    public async Task<IActionResult> Index()
    {
        var docs = await _documentService.GetAllDocumentsAsync();
        return View(docs);
    }

    // GET /Document/Upload
    public IActionResult Upload() => View();

    // POST /Document/Upload
    [HttpPost]
    public async Task<IActionResult> Upload(DocumentUploadDto dto)
    {
        if (!ModelState.IsValid) return View(dto);

        try
        {
            // ── Copy stream to memory so both paths can read the same bytes ──────
            // IFormFile.OpenReadStream() returns a forward-only stream that can be
            // read only once. We buffer it here before the first consumer touches it.
            using var ms = new MemoryStream();
            await dto.File.CopyToAsync(ms);
            ms.Position = 0;

            // ── Path 1: Legacy SQL Server entity path ─────────────────────────────
            // UploadDocumentAsync reads dto.File; it will get its own seek from the
            // IFormFile internal buffer (separate from our MemoryStream copy).
            await _documentService.UploadDocumentAsync(dto);
            _logger.LogInformation("Document '{Name}' saved via legacy IDocumentService.", dto.File.FileName);

            // ── Path 2: New modular RAG pipeline ──────────────────────────────────
            ms.Position = 0;
            var ingestionRequest = new IngestionRequest
            {
                FileStream     = ms,
                FileName       = dto.File.FileName,
                ContentType    = dto.File.ContentType,
                UploadedBy     = dto.UploadedBy,
                EmbeddingModel = "text-embedding-ada-002"
            };

            var result = await _pipeline.IngestAsync(ingestionRequest);

            if (result.Success)
            {
                TempData["IngestionResult"] =
                    $"✅ Document uploaded. RAG pipeline processed {result.ChunksCreated} chunks " +
                    $"and generated {result.EmbeddingsCreated} embeddings.";
                _logger.LogInformation(
                    "IIngestionPipeline succeeded: DocumentId={Id}, Chunks={Chunks}, Embeddings={Emb}.",
                    result.DocumentId, result.ChunksCreated, result.EmbeddingsCreated);
            }
            else
            {
                TempData["IngestionResult"] =
                    $"⚠️ Document saved, but RAG pipeline reported: {result.ErrorMessage}";
                _logger.LogWarning(
                    "IIngestionPipeline returned failure for '{Name}': {Error}.",
                    dto.File.FileName, result.ErrorMessage);
            }

            return RedirectToAction(nameof(Index));
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError("File", ex.Message);
            return View(dto);
        }
    }

    // GET /Document/Edit/{id}
    public async Task<IActionResult> Edit(int id)
    {
        var doc = await _documentService.GetDocumentByIdAsync(id);
        if (doc == null) return NotFound();

        var updateDto = new DocumentUpdateDto
        {
            Name       = doc.Name,
            UploadedBy = doc.UploadedBy,
            Type       = doc.Type
        };
        return View(updateDto);
    }

    // POST /Document/Edit/{id}
    [HttpPost]
    public async Task<IActionResult> Edit(int id, DocumentUpdateDto dto)
    {
        if (!ModelState.IsValid) return View(dto);

        try
        {
            await _documentService.UpdateDocumentAsync(id, dto);
            return RedirectToAction(nameof(Index));
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError("NewFile", ex.Message);
            return View(dto);
        }
    }

    // GET /Document/Details/{id}
    public async Task<IActionResult> Details(int id)
    {
        var doc = await _documentService.GetDocumentByIdAsync(id);
        if (doc == null) return NotFound();
        return View(doc);
    }

    // POST /Document/Delete/{id}
    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        await _documentService.DeleteDocumentAsync(id);
        return RedirectToAction(nameof(Index));
    }

    // POST /Document/RebuildEmbeddings/{id}
    [HttpPost]
    public async Task<IActionResult> RebuildEmbeddings(int id)
    {
        await _documentService.RebuildDocumentEmbeddingsAsync(id);
        TempData["Success"] = "Embeddings rebuilt successfully.";
        return RedirectToAction(nameof(Index));
    }
}
