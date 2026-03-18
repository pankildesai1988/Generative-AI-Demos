using ArNir.Core.Config;
using ArNir.Core.DTOs.Documents;
using ArNir.RAG.Interfaces;
using ArNir.RAG.Models;
using ArNir.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace ArNir.Admin.Controllers;

/// <summary>
/// Admin CRUD controller for documents.
/// <para>
/// The Upload action uses a <b>bridge pattern</b>:
/// <list type="number">
///   <item>
///     <term>Path 1 — Legacy (SQL Server entity)</term>
///     <description>
///       <see cref="IDocumentService.UploadDocumentAsync"/> saves Documents + DocumentChunks
///       rows in SQL Server. Returns the SQL <c>Document.Id</c> (int) via DocumentResponseDto.
///     </description>
///   </item>
///   <item>
///     <term>Path 2 — Modular RAG pipeline (<see cref="IIngestionPipeline"/>)</term>
///     <description>
///       Parse → Chunk → Embed → Store. The SQL doc ID is threaded into
///       <see cref="IngestionRequest.LegacySqlDocumentId"/> so the
///       <c>PgvectorDocumentVectorStore</c> can resolve the correct FK when writing
///       Embedding rows to PostgreSQL.
///     </description>
///   </item>
/// </list>
/// </para>
/// </summary>
[Authorize]
public class DocumentController : Controller
{
    private readonly IDocumentService              _documentService;
    private readonly IIngestionPipeline            _pipeline;
    private readonly ILogger<DocumentController>   _logger;
    private readonly IOptions<FileUploadSettings>  _uploadSettings;

    /// <summary>Initialises the controller with the legacy service, new pipeline, and settings.</summary>
    public DocumentController(
        IDocumentService             documentService,
        IIngestionPipeline           pipeline,
        ILogger<DocumentController>  logger,
        IOptions<FileUploadSettings> uploadSettings)
    {
        _documentService = documentService;
        _pipeline        = pipeline;
        _logger          = logger;
        _uploadSettings  = uploadSettings;
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
        // ── Server-side file validation (Sprint 1) ────────────────────────────
        if (dto.File is null || dto.File.Length == 0)
        {
            ModelState.AddModelError("File", "Please select a file.");
        }
        else if (dto.File.Length > _uploadSettings.Value.MaxFileSize)
        {
            var maxMb = _uploadSettings.Value.MaxFileSize / 1_048_576;
            ModelState.AddModelError("File", $"File size exceeds the {maxMb} MB limit.");
        }
        else if (_uploadSettings.Value.AllowedTypes.Length > 0 &&
                 !_uploadSettings.Value.AllowedTypes.Contains(dto.File.ContentType))
        {
            ModelState.AddModelError("File", "Only PDF, DOCX, and TXT files are accepted.");
        }

        if (!ModelState.IsValid) return View(dto);

        try
        {
            // ── Copy stream to memory so both paths can read the same bytes ──
            using var ms = new MemoryStream();
            await dto.File.CopyToAsync(ms);
            ms.Position = 0;

            // ── Path 1: Legacy SQL Server entity path ─────────────────────────
            // Capture the returned DocumentResponseDto to get the SQL Document.Id (int PK).
            var docResult = await _documentService.UploadDocumentAsync(dto);
            _logger.LogInformation(
                "Document '{Name}' saved via legacy IDocumentService (SqlId={Id}).",
                dto.File.FileName, docResult.Id);

            // ── Path 2: New modular RAG pipeline ──────────────────────────────
            // LegacySqlDocumentId threads the SQL doc ID through the pipeline so
            // PgvectorDocumentVectorStore can resolve the DocumentChunk FK.
            ms.Position = 0;
            var ingestionRequest = new IngestionRequest
            {
                FileStream          = ms,
                FileName            = dto.File.FileName,
                ContentType         = dto.File.ContentType,
                UploadedBy          = dto.UploadedBy,
                EmbeddingModel      = "text-embedding-ada-002",
                LegacySqlDocumentId = docResult.Id
            };

            var result = await _pipeline.IngestAsync(ingestionRequest);

            TempData["IngestionResult"] = result.Success
                ? $"✅ Document uploaded. RAG pipeline processed {result.ChunksCreated} chunks " +
                  $"and generated {result.EmbeddingsCreated} embeddings."
                : $"⚠️ Document saved, but RAG pipeline reported: {result.ErrorMessage}";

            if (result.Success)
                _logger.LogInformation(
                    "IIngestionPipeline succeeded: SqlDocId={Id}, Chunks={C}, Embeddings={E}.",
                    docResult.Id, result.ChunksCreated, result.EmbeddingsCreated);
            else
                _logger.LogWarning(
                    "IIngestionPipeline failed for '{Name}': {Error}.",
                    dto.File.FileName, result.ErrorMessage);

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
