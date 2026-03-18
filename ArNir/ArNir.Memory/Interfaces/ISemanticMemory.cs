using ArNir.Memory.Models;

namespace ArNir.Memory.Interfaces;

/// <summary>
/// Defines a contract for long-term, vector-based semantic memory.
/// Implementations are responsible for storing and retrieving <see cref="MemoryEntry"/> objects
/// by similarity to a query embedding or query text.
/// <para>
/// <b>NOT registered by default</b> in the DI container — semantic memory requires an
/// embedding provider and a vector store, which are infrastructure concerns supplied by
/// the consuming application.
/// </para>
/// </summary>
public interface ISemanticMemory
{
    /// <summary>
    /// Persists a <see cref="MemoryEntry"/> (with its embedding) into the semantic store.
    /// </summary>
    /// <param name="entry">
    /// The memory entry to store. <see cref="MemoryEntry.Embedding"/> should be populated
    /// before calling this method.
    /// </param>
    /// <param name="ct">A cancellation token.</param>
    Task StoreAsync(MemoryEntry entry, CancellationToken ct = default);

    /// <summary>
    /// Retrieves the top-<paramref name="topK"/> entries whose embeddings are most similar
    /// to the provided <paramref name="queryEmbedding"/>.
    /// </summary>
    /// <param name="queryEmbedding">The dense vector to search against.</param>
    /// <param name="topK">Maximum number of results to return. Defaults to <c>5</c>.</param>
    /// <param name="ct">A cancellation token.</param>
    /// <returns>A read-only list of <see cref="MemoryEntry"/> objects ranked by similarity.</returns>
    Task<IReadOnlyList<MemoryEntry>> RecallAsync(float[] queryEmbedding, int topK = 5, CancellationToken ct = default);

    /// <summary>
    /// Retrieves the top-<paramref name="topK"/> entries most semantically relevant to the
    /// given <paramref name="query"/> text. Implementations are responsible for embedding
    /// the query text before performing the vector search.
    /// </summary>
    /// <param name="query">The natural-language query string.</param>
    /// <param name="topK">Maximum number of results to return. Defaults to <c>5</c>.</param>
    /// <param name="ct">A cancellation token.</param>
    /// <returns>A read-only list of <see cref="MemoryEntry"/> objects ranked by relevance.</returns>
    Task<IReadOnlyList<MemoryEntry>> RecallByTextAsync(string query, int topK = 5, CancellationToken ct = default);
}
