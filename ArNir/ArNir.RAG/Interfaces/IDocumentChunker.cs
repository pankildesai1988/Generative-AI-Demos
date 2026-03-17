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
    /// <param name="chunkSize">The target number of characters per chunk. Defaults to <c>500</c>.</param>
    /// <param name="overlap">The number of characters that adjacent chunks share. Defaults to <c>50</c>.</param>
    /// <returns>An ordered, read-only list of <see cref="RagChunk"/> objects.</returns>
    IReadOnlyList<RagChunk> Chunk(RagDocument document, int chunkSize = 500, int overlap = 50);
}
