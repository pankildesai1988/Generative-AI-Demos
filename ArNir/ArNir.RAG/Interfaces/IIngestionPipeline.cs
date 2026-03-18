using ArNir.RAG.Models;

namespace ArNir.RAG.Interfaces;

/// <summary>
/// Defines the single entry point for the full document ingestion pipeline:
/// Parse → Chunk → Embed → Store.
/// </summary>
public interface IIngestionPipeline
{
    /// <summary>
    /// Executes the full ingestion pipeline for the given request.
    /// </summary>
    /// <param name="request">
    /// An <see cref="IngestionRequest"/> containing the document stream, file metadata,
    /// and embedding model configuration.
    /// </param>
    /// <returns>
    /// An <see cref="IngestionResult"/> indicating success or failure, with counts of
    /// chunks and embeddings created.
    /// </returns>
    Task<IngestionResult> IngestAsync(IngestionRequest request);
}
