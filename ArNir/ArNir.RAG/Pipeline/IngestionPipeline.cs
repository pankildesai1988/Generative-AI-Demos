using ArNir.RAG.Interfaces;
using ArNir.RAG.Models;
using Microsoft.Extensions.Logging;

namespace ArNir.RAG.Pipeline;

/// <summary>
/// Orchestrates the full document ingestion flow: Parse → Chunk → Embed → Store.
/// </summary>
public sealed class IngestionPipeline : IIngestionPipeline
{
    private readonly IEnumerable<IDocumentParser> _parsers;
    private readonly IDocumentChunker             _chunker;
    private readonly IDocumentEmbedder            _embedder;
    private readonly IDocumentVectorStore         _vectorStore;
    private readonly ILogger<IngestionPipeline>   _logger;

    /// <summary>
    /// Initialises a new instance of <see cref="IngestionPipeline"/>.
    /// </summary>
    /// <param name="parsers">All registered <see cref="IDocumentParser"/> implementations.</param>
    /// <param name="chunker">The chunking strategy to apply after parsing.</param>
    /// <param name="embedder">The embedder used to vectorise each chunk.</param>
    /// <param name="vectorStore">The vector store where embeddings are persisted.</param>
    /// <param name="logger">Logger for diagnostic output.</param>
    public IngestionPipeline(
        IEnumerable<IDocumentParser>  parsers,
        IDocumentChunker              chunker,
        IDocumentEmbedder             embedder,
        IDocumentVectorStore          vectorStore,
        ILogger<IngestionPipeline>    logger)
    {
        _parsers     = parsers;
        _chunker     = chunker;
        _embedder    = embedder;
        _vectorStore = vectorStore;
        _logger      = logger;
    }

    /// <inheritdoc />
    public async Task<IngestionResult> IngestAsync(IngestionRequest request)
    {
        try
        {
            // ── 1. Parse ────────────────────────────────────────────────────────────
            var parser = _parsers.FirstOrDefault(p => p.CanParse(request.ContentType));
            if (parser is null)
            {
                return new IngestionResult
                {
                    Success      = false,
                    ErrorMessage = $"No parser registered for content type '{request.ContentType}'."
                };
            }

            _logger.LogInformation(
                "Parsing document '{FileName}' (content-type: {ContentType})",
                request.FileName, request.ContentType);

            var document = await parser.ParseAsync(
                request.FileStream, request.FileName, request.ContentType);

            // ── 2. Chunk ────────────────────────────────────────────────────────────
            _logger.LogInformation(
                "Chunking document {DocumentId} ('{FileName}')",
                document.Id, document.FileName);

            var chunks = _chunker.Chunk(document);

            // ── 3. Embed (batch) ────────────────────────────────────────────────────
            _logger.LogInformation(
                "Generating embeddings for {ChunkCount} chunks using model '{Model}'",
                chunks.Count, request.EmbeddingModel);

            var texts   = chunks.Select(c => c.Text);
            var vectors = await _embedder.GenerateBatchAsync(texts, request.EmbeddingModel);

            // ── 4. Store (batch) ────────────────────────────────────────────────────
            var items = chunks.Zip(vectors, (chunk, vector) =>
                (chunkId: chunk.Id.ToString(), vector));

            _logger.LogInformation(
                "Storing {VectorCount} vectors for document {DocumentId}",
                vectors.Count, document.Id);

            await _vectorStore.StoreBatchAsync(items);

            return new IngestionResult
            {
                Success           = true,
                DocumentId        = document.Id,
                ChunksCreated     = chunks.Count,
                EmbeddingsCreated = vectors.Count
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Ingestion failed for '{FileName}': {Message}",
                request.FileName, ex.Message);

            return new IngestionResult
            {
                Success      = false,
                ErrorMessage = ex.Message
            };
        }
    }
}
