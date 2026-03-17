using ArNir.Memory.Models;

namespace ArNir.Memory.Interfaces;

/// <summary>
/// Defines a contract for short-term, session-scoped conversational memory.
/// <para>
/// CRITICAL: This interface is named <c>IEpisodicMemory</c> — NOT <c>IContextMemoryService</c>.
/// <c>IContextMemoryService</c> already exists in <c>ArNir.Services</c> with different method signatures;
/// using the same name would cause interface conflicts.
/// </para>
/// </summary>
public interface IEpisodicMemory
{
    /// <summary>
    /// Appends a <see cref="MemoryEntry"/> to the history for its session.
    /// </summary>
    /// <param name="entry">The memory entry to store.</param>
    /// <param name="ct">A cancellation token.</param>
    Task AddAsync(MemoryEntry entry, CancellationToken ct = default);

    /// <summary>
    /// Retrieves the most recent entries for a given session, ordered oldest-first.
    /// </summary>
    /// <param name="sessionId">The session whose history to retrieve.</param>
    /// <param name="limit">Maximum number of entries to return. Defaults to <c>20</c>.</param>
    /// <param name="ct">A cancellation token.</param>
    /// <returns>An ordered, read-only list of <see cref="MemoryEntry"/> objects.</returns>
    Task<IReadOnlyList<MemoryEntry>> GetSessionHistoryAsync(string sessionId, int limit = 20, CancellationToken ct = default);

    /// <summary>
    /// Removes all stored entries for the given session.
    /// </summary>
    /// <param name="sessionId">The session whose history to clear.</param>
    /// <param name="ct">A cancellation token.</param>
    Task ClearSessionAsync(string sessionId, CancellationToken ct = default);
}
