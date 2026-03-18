using System.Collections.Concurrent;
using ArNir.Memory.Interfaces;
using ArNir.Memory.Models;
using Microsoft.Extensions.Logging;

namespace ArNir.Memory.InProcess;

/// <summary>
/// An in-process, <see cref="ConcurrentDictionary{TKey,TValue}"/>-backed implementation of
/// <see cref="IEpisodicMemory"/> intended for development and testing scenarios.
/// <para>
/// All data is stored in memory and is lost when the process exits.
/// For production use, replace with a persistent implementation (e.g., database-backed).
/// </para>
/// </summary>
public sealed class InProcessEpisodicMemory : IEpisodicMemory
{
    private readonly ConcurrentDictionary<string, List<MemoryEntry>> _sessions = new();
    private readonly ILogger<InProcessEpisodicMemory> _logger;

    /// <summary>
    /// Initialises a new instance of <see cref="InProcessEpisodicMemory"/>.
    /// </summary>
    /// <param name="logger">Logger for diagnostic output.</param>
    public InProcessEpisodicMemory(ILogger<InProcessEpisodicMemory> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc />
    public Task AddAsync(MemoryEntry entry, CancellationToken ct = default)
    {
        var list = _sessions.GetOrAdd(entry.SessionId, _ => new List<MemoryEntry>());

        lock (list)
        {
            list.Add(entry);
        }

        _logger.LogDebug(
            "Added memory entry {EntryId} to session {SessionId} (role={Role}).",
            entry.Id, entry.SessionId, entry.Role);

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<MemoryEntry>> GetSessionHistoryAsync(
        string sessionId,
        int limit = 20,
        CancellationToken ct = default)
    {
        if (!_sessions.TryGetValue(sessionId, out var list))
        {
            return Task.FromResult<IReadOnlyList<MemoryEntry>>(Array.Empty<MemoryEntry>());
        }

        IReadOnlyList<MemoryEntry> result;

        lock (list)
        {
            result = list
                .OrderBy(e => e.CreatedAt)
                .TakeLast(limit)
                .ToList();
        }

        return Task.FromResult(result);
    }

    /// <inheritdoc />
    public Task ClearSessionAsync(string sessionId, CancellationToken ct = default)
    {
        _sessions.TryRemove(sessionId, out _);

        _logger.LogDebug("Cleared episodic memory for session {SessionId}.", sessionId);

        return Task.CompletedTask;
    }
}
