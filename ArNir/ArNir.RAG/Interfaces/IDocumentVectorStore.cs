using ArNir.RAG.Models;

namespace ArNir.RAG.Interfaces;

/// <summary>
/// Defines a contract for persisting and querying document chunk embeddings in a vector store.
/// <para>
/// CRITICAL: This interface is named <c>IDocumentVectorStore</c> — NOT <c>IRetrievalService</c>.
/// <c>IRetrievalService</c> already exists in <c>ArNir.Services</c> with different method signatures;
/// renaming or merging these would cause interface conflicts.
/// </para>
/// </summary>
public interface IDocumentVectorStore
{
    /// <summary>
    /// Persists a single chunk embedding with optional metadata.
    /// </summary>
    /// <param name="chunkId">The unique string identifier of the chunk.</param>
    /// <param name="vector">The dense float embedding vector.</param>
    /// <param name="metadata">Optional key/value metadata to associate with the vector entry.</param>
    Task StoreAsync(string chunkId, float[] vector, Dictionary<string, string>? metadata = null);

    /// <summary>
    /// Persists a batch of chunk embeddings in a single operation.
    /// </summary>
    /// <param name="items">
    /// A sequence of <c>(chunkId, vector)</c> tuples to store.
    /// </param>
    Task StoreBatchAsync(IEnumerable<(string chunkId, float[] vector)> items);

    /// <summary>
    /// Searches the vector store for the nearest neighbours to the given query vector.
    /// </summary>
    /// <param name="queryVector">The query embedding vector.</param>
    /// <param name="topK">The maximum number of results to return. Defaults to <c>5</c>.</param>
    /// <returns>A read-only list of <see cref="RetrievalResult"/> ordered by descending similarity score.</returns>
    Task<IReadOnlyList<RetrievalResult>> SearchAsync(float[] queryVector, int topK = 5);

    /// <summary>
    /// Removes all vector entries associated with the specified document.
    /// </summary>
    /// <param name="documentId">The string identifier of the document whose vectors should be deleted.</param>
    Task DeleteByDocumentAsync(string documentId);
}
