using ArNir.RAG.Interfaces;
using ArNir.RAG.Models;
using Microsoft.Extensions.Logging;

namespace ArNir.RAG.InProcess;

/// <summary>
/// No-op dev stub for <see cref="IDocumentVectorStore"/>.
/// All store operations are silent no-ops; search always returns an empty list.
/// <para>
/// <b>Warning:</b> Replace with a real vector store (e.g., pgvector, Qdrant, Azure AI Search)
/// before deploying to production.
/// </para>
/// </summary>
public sealed class NullDocumentVectorStore : IDocumentVectorStore
{
    private readonly ILogger<NullDocumentVectorStore> _logger;

    /// <summary>Initialises a new <see cref="NullDocumentVectorStore"/>.</summary>
    public NullDocumentVectorStore(ILogger<NullDocumentVectorStore> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc />
    public Task StoreAsync(string chunkId, float[] vector, Dictionary<string, string>? metadata = null)
    {
        _logger.LogDebug("[NullDocumentVectorStore] StoreAsync called for chunk '{ChunkId}' — no-op stub.", chunkId);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task StoreBatchAsync(IEnumerable<(string chunkId, float[] vector)> items)
    {
        _logger.LogDebug("[NullDocumentVectorStore] StoreBatchAsync called — no-op stub.");
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<RetrievalResult>> SearchAsync(float[] queryVector, int topK = 5)
    {
        _logger.LogDebug("[NullDocumentVectorStore] SearchAsync called — no-op stub, returning empty results.");
        return Task.FromResult<IReadOnlyList<RetrievalResult>>(Array.Empty<RetrievalResult>());
    }

    /// <inheritdoc />
    public Task DeleteByDocumentAsync(string documentId)
    {
        _logger.LogDebug("[NullDocumentVectorStore] DeleteByDocumentAsync called for document '{DocumentId}' — no-op stub.", documentId);
        return Task.CompletedTask;
    }
}
