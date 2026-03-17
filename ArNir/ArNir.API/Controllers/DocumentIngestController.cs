using ArNir.RAG.Interfaces;
using ArNir.RAG.Models;
using Microsoft.AspNetCore.Mvc;

namespace ArNir.Api.Controllers;

/// <summary>
/// Exposes document ingestion via the ArNir.RAG pipeline.
/// Clients upload a document (PDF, DOCX, or plain text) and the pipeline
/// parses → chunks → embeds → stores it, returning the resulting counts.
/// </summary>
[ApiController]
[Route("api/documents")]
public sealed class DocumentIngestController : ControllerBase
{
    private readonly IIngestionPipeline _pipeline;
    private readonly ILogger<DocumentIngestController> _logger;

    /// <summary>Initialises a new <see cref="DocumentIngestController"/>.</summary>
    public DocumentIngestController(IIngestionPipeline pipeline, ILogger<DocumentIngestController> logger)
    {
        _pipeline = pipeline;
        _logger = logger;
    }

    /// <summary>
    /// Ingests an uploaded document through the full RAG pipeline: Parse → Chunk → Embed → Store.
    /// </summary>
    /// <param name="file">The multipart file to ingest.</param>
    /// <param name="embeddingModel">
    /// Optional embedding model override. Defaults to <c>text-embedding-ada-002</c>.
    /// </param>
    /// <returns>
    /// An <see cref="IngestionResult"/> containing <c>DocumentId</c>, <c>ChunksCreated</c>,
    /// and <c>EmbeddingsCreated</c> on success, or an error message on failure.
    /// </returns>
    /// <remarks>
    /// <b>Demo note:</b> In dev mode the pipeline uses <c>NullDocumentEmbedder</c> and
    /// <c>NullDocumentVectorStore</c> — parsing and chunking work fully, but vectors are
    /// not persisted to a real store. Replace the stubs with production implementations
    /// before using this endpoint in a live environment.
    /// </remarks>
    [HttpPost("ingest")]
    public async Task<IActionResult> Ingest(
        IFormFile file,
        [FromQuery] string embeddingModel = "text-embedding-ada-002")
    {
        if (file is null || file.Length == 0)
            return BadRequest("No file uploaded.");

        _logger.LogInformation("Ingesting document '{FileName}' ({Size} bytes)", file.FileName, file.Length);

        var request = new IngestionRequest
        {
            FileStream     = file.OpenReadStream(),
            FileName       = file.FileName,
            ContentType    = file.ContentType,
            UploadedBy     = User.Identity?.Name,
            EmbeddingModel = embeddingModel
        };

        var result = await _pipeline.IngestAsync(request);

        if (!result.Success)
            return BadRequest(new { error = result.ErrorMessage });

        return Ok(result);
    }
}
