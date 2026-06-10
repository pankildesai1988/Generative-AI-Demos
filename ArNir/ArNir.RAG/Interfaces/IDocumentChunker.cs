using ArNir.RAG.Models;

namespace ArNir.RAG.Interfaces;

/// <summary>
/// Defines a strategy for splitting a <see cref="RagDocument"/> into a collection of <see cref="RagChunk"/> objects.
/// </summary>
public interface IDocumentChunker
{
    /// <summary>
    /// Splits the given document into a read-only list of text chunks.
    /// </summary>
    /// <param name="document">The parsed document to chunk.</param>
    /// <param name="chunkSize">The target characters per chunk. When <see langword="null"/> the
    /// implementation's configured value (appsettings <c>Rag:ChunkSize</c> → const) is used.</param>
    /// <param name="overlap">The characters adjacent chunks share. When <see langword="null"/> the
    /// implementation's configured value (appsettings <c>Rag:ChunkOverlap</c> → const) is used.</param>
    /// <returns>An ordered, read-only list of <see cref="RagChunk"/> objects.</returns>
    IReadOnlyList<RagChunk> Chunk(RagDocument document, int? chunkSize = null, int? overlap = null);
}
