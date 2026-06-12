using ArNir.Core.Models.Chunking;

namespace ArNir.Core.Interfaces;

/// <summary>
/// Produces the single authoritative chunk sequence for an uploaded file. Both ingestion paths
/// must consume this one sequence: the SQL path (<c>DocumentService</c>) persists the chunks as
/// <c>DocumentChunk</c> rows ordered by <see cref="ExtractedChunk.Index"/>, and the vector path
/// (<c>IngestionPipeline</c>) embeds the same chunks keyed by the same index — so PostgreSQL
/// embeddings always FK to the SQL chunk row holding the identical text.
/// <para>
/// Lives in <c>ArNir.Core.Interfaces</c> (like <c>IEmbeddingProvider</c>) because
/// <c>ArNir.Services</c> and <c>ArNir.RAG</c> must never reference each other; the implementation
/// is in <c>ArNir.RAG</c> and is wired by the composition roots (Admin/API).
/// </para>
/// </summary>
public interface IUnifiedChunkExtractor
{
    /// <summary>
    /// Parses <paramref name="content"/> and returns the ordered chunk sequence
    /// (text, table, and image chunks) for the document.
    /// </summary>
    /// <param name="content">The raw file stream.</param>
    /// <param name="fileName">The original file name (used for metadata only).</param>
    /// <param name="contentType">The MIME type used to select the parser.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The extraction result with sequentially indexed chunks.</returns>
    /// <exception cref="NotSupportedException">No parser is registered for <paramref name="contentType"/>.</exception>
    Task<ChunkExtractionResult> ExtractAsync(Stream content, string fileName, string contentType, CancellationToken ct = default);
}
