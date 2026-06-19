using ArNir.Core.Interfaces;
using ArNir.Core.Models.Chunking;
using ArNir.RAG.Interfaces;
using ArNir.RAG.Models;
using Microsoft.Extensions.Logging;

namespace ArNir.RAG.Pipeline;

/// <summary>
/// Orchestrates the full document ingestion flow: Extract (parse + chunk) → Embed → Store.
/// Chunking is delegated to <see cref="IUnifiedChunkExtractor"/> — the same component the SQL
/// path (<c>DocumentService</c>) uses — so the embedding key <c>"sql:{docId}:{index}"</c> always
/// refers to the <c>DocumentChunk</c> row with identical text.
/// </summary>
public sealed class IngestionPipeline : IIngestionPipeline
{
    private readonly IUnifiedChunkExtractor     _chunkExtractor;
    private readonly IDocumentEmbedder          _embedder;
    private readonly IDocumentVectorStore       _vectorStore;
    private readonly ILogger<IngestionPipeline> _logger;

    /// <summary>
    /// Initialises a new instance of <see cref="IngestionPipeline"/>.
    /// </summary>
    /// <param name="chunkExtractor">The unified extractor producing the authoritative chunk sequence.</param>
    /// <param name="embedder">The embedder used to vectorise each chunk.</param>
    /// <param name="vectorStore">The vector store where embeddings are persisted.</param>
    /// <param name="logger">Logger for diagnostic output.</param>
    public IngestionPipeline(
        IUnifiedChunkExtractor      chunkExtractor,
        IDocumentEmbedder           embedder,
        IDocumentVectorStore        vectorStore,
        ILogger<IngestionPipeline>  logger)
    {
        _chunkExtractor = chunkExtractor;
        _embedder       = embedder;
        _vectorStore    = vectorStore;
        _logger         = logger;
    }

    /// <inheritdoc />
    public async Task<IngestionResult> IngestAsync(IngestionRequest request)
    {
        try
        {
            // ── 1. Extract (parse + chunk via the shared extractor) ─────────────────
            _logger.LogInformation(
                "Extracting chunks for '{FileName}' (content-type: {ContentType})",
                request.FileName, request.ContentType);

            var extraction = await _chunkExtractor.ExtractAsync(
                request.FileStream, request.FileName, request.ContentType);

            // Image stubs ("[Image: page N, image i]") are persisted as DocumentChunk rows by the
            // SQL path for page provenance/citation, but they carry no real text — embedding them
            // floods the vector store with near-identical placeholder vectors that crowd out real
            // content in top-K retrieval. Skip them here; their ChunkOrder simply has no embedding
            // row (a harmless orphan). Index alignment is preserved because the chunkId is built
            // from each embedded chunk's own Index, not its position in this filtered list.
            // TODO: embed these once vision captioning replaces the placeholder text.
            var chunks = extraction.Chunks
                .Where(c => !string.Equals(c.ChunkType, ChunkTypes.Image, StringComparison.OrdinalIgnoreCase))
                .ToList();

            var skipped = extraction.Chunks.Count - chunks.Count;

            // ── 2. Embed (batch) ────────────────────────────────────────────────────
            _logger.LogInformation(
                "Generating embeddings for {ChunkCount} chunks using model '{Model}' ({Skipped} image stub(s) skipped)",
                chunks.Count, request.EmbeddingModel, skipped);

            var texts   = chunks.Select(c => c.Text);
            var vectors = await _embedder.GenerateBatchAsync(texts, request.EmbeddingModel);

            // ── 3. Store (batch) ────────────────────────────────────────────────────
            // When LegacySqlDocumentId is provided, encode it plus the chunk index as
            // "sql:{docId}:{chunkIndex}" so PgvectorDocumentVectorStore can resolve the
            // DocumentChunk.Id (int FK) required by the Embeddings table in PostgreSQL.
            // The index matches DocumentChunk.ChunkOrder because both paths run the same
            // IUnifiedChunkExtractor over the same bytes. Without a SQL id a fresh Guid is
            // used (safe no-op with null-stub in dev).
            var items = chunks.Zip(vectors, (chunk, vector) =>
            {
                var chunkId = request.LegacySqlDocumentId.HasValue
                    ? $"sql:{request.LegacySqlDocumentId}:{chunk.Index}"
                    : Guid.NewGuid().ToString();
                return (chunkId, vector);
            });

            _logger.LogInformation(
                "Storing {VectorCount} vectors for document {DocumentId}",
                vectors.Count, extraction.DocumentId);

            await _vectorStore.StoreBatchAsync(items);

            return new IngestionResult
            {
                Success           = true,
                DocumentId        = extraction.DocumentId,
                ChunksCreated     = extraction.Chunks.Count,
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
