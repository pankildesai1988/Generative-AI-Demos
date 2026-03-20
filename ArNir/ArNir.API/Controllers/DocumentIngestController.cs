using ArNir.Core.DTOs.Documents;
using ArNir.RAG.Hosting;
using ArNir.RAG.Models;
using ArNir.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ArNir.Api.Controllers;

/// <summary>
/// Exposes document ingestion via the ArNir.RAG pipeline.
/// Clients upload a document (PDF, DOCX, or plain text); the controller saves
/// it to SQL Server via <see cref="IDocumentService"/> (Path 1), then enqueues
/// a background RAG ingestion job via <see cref="IngestionQueue"/> (Path 2):
/// Parse, Chunk, Embed, Store.
/// </summary>
[ApiController]
[Route("api/documents")]
public sealed class DocumentIngestController : ControllerBase
{
    private readonly IDocumentService _documentService;
    private readonly IngestionQueue _ingestionQueue;
    private readonly ILogger<DocumentIngestController> _logger;

    /// <summary>Initialises a new <see cref="DocumentIngestController"/>.</summary>
    public DocumentIngestController(
        IDocumentService documentService,
        IngestionQueue ingestionQueue,
        ILogger<DocumentIngestController> logger)
    {
        _documentService = documentService;
        _ingestionQueue  = ingestionQueue;
        _logger          = logger;
    }

    /// <summary>
    /// Ingests an uploaded document through the dual-path pipeline:
    /// Path 1 — save to SQL Server via <see cref="IDocumentService"/>;
    /// Path 2 — enqueue background RAG pipeline (Parse, Chunk, Embed, Store).
    /// </summary>
    /// <param name="file">The multipart file to ingest.</param>
    /// <param name="uploadedBy">
    /// Optional uploader identifier from multipart form data.
    /// Defaults to <c>demo-user</c> when not provided.
    /// </param>
    /// <param name="embeddingModel">
    /// Optional embedding model override. Defaults to <c>text-embedding-ada-002</c>.
    /// </param>
    /// <returns>
    /// <c>202 Accepted</c> with <c>documentId</c> and a status message on success,
    /// or <c>400 Bad Request</c> on validation failure.
    /// </returns>
    [HttpPost("ingest")]
    public async Task<IActionResult> Ingest(
        IFormFile file,
        [FromForm] string uploadedBy = "demo-user",
        [FromQuery] string embeddingModel = "text-embedding-ada-002")
    {
        if (file is null || file.Length == 0)
            return BadRequest("No file uploaded.");

        _logger.LogInformation("Ingesting document '{FileName}' ({Size} bytes)", file.FileName, file.Length);

        // ── Read file bytes into MemoryStream (IFormFile stream is only readable during the request) ──
        var ms = new MemoryStream();
        await file.CopyToAsync(ms);
        ms.Position = 0;

        // ── Path 1: Save to SQL Server via IDocumentService ──
        var uploadDto = new DocumentUploadDto
        {
            File       = file,
            UploadedBy = uploadedBy
        };

        var docResult = await _documentService.UploadDocumentAsync(uploadDto);
        _logger.LogInformation(
            "Document '{Name}' saved via IDocumentService (SqlId={Id}).",
            file.FileName, docResult.Id);

        // ── Path 2: Enqueue background RAG pipeline ──
        ms.Position = 0;
        var ingestionRequest = new IngestionRequest
        {
            FileStream          = ms,
            FileName            = file.FileName,
            ContentType         = file.ContentType,
            UploadedBy          = uploadedBy,
            EmbeddingModel      = embeddingModel,
            LegacySqlDocumentId = docResult.Id
        };

        var jobRequest = new IngestionJobRequest(
            ingestionRequest,
            file.FileName,
            DateTime.UtcNow);

        await _ingestionQueue.EnqueueAsync(jobRequest);

        _logger.LogInformation(
            "Ingestion job enqueued for '{Name}' (SqlId={Id}).",
            file.FileName, docResult.Id);

        return Accepted(new
        {
            message    = "Document saved. RAG embedding queued for background processing.",
            documentId = docResult.Id
        });
    }
}
