using ArNir.Memory.Interfaces;
using ArNir.Memory.Models;
using Microsoft.Extensions.Logging;

namespace ArNir.Memory.InProcess;

/// <summary>
/// No-op dev stub for <see cref="ISemanticMemory"/>.
/// All operations succeed silently and return empty results.
/// <para>
/// <b>Purpose:</b> Allows the DI container (and <c>PlannerAgent</c>) to resolve
/// <see cref="ISemanticMemory"/> in local/dev environments without a real vector store.
/// </para>
/// <para>
/// <b>Warning:</b> Replace with a real vector-store-backed implementation (e.g., pgvector,
/// Azure AI Search, Qdrant) before deploying to production.
/// </para>
/// </summary>
public sealed class NullSemanticMemory : ISemanticMemory
{
    private readonly ILogger<NullSemanticMemory> _logger;

    /// <summary>
    /// Initialises a new <see cref="NullSemanticMemory"/> instance.
    /// </summary>
    /// <param name="logger">Logger used to emit debug-level no-op notices.</param>
    public NullSemanticMemory(ILogger<NullSemanticMemory> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc />
    /// <remarks>No-op — entry is silently discarded.</remarks>
    public Task StoreAsync(MemoryEntry entry, CancellationToken ct = default)
    {
        _logger.LogDebug("[NullSemanticMemory] StoreAsync called for session '{SessionId}' — no-op stub, entry discarded.", entry.SessionId);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    /// <remarks>No-op — always returns an empty list.</remarks>
    public Task<IReadOnlyList<MemoryEntry>> RecallAsync(float[] queryEmbedding, int topK = 5, CancellationToken ct = default)
    {
        _logger.LogDebug("[NullSemanticMemory] RecallAsync called — no-op stub, returning empty list.");
        return Task.FromResult<IReadOnlyList<MemoryEntry>>(Array.Empty<MemoryEntry>());
    }

    /// <inheritdoc />
    /// <remarks>No-op — always returns an empty list.</remarks>
    public Task<IReadOnlyList<MemoryEntry>> RecallByTextAsync(string query, int topK = 5, CancellationToken ct = default)
    {
        _logger.LogDebug("[NullSemanticMemory] RecallByTextAsync called for query '{Query}' — no-op stub, returning empty list.", query);
        return Task.FromResult<IReadOnlyList<MemoryEntry>>(Array.Empty<MemoryEntry>());
    }
}
