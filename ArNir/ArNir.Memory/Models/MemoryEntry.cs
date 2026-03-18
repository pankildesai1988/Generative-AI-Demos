namespace ArNir.Memory.Models;

/// <summary>
/// Represents a single entry in conversational or semantic memory.
/// Intentionally separate from any EF-mapped entity — pure in-memory model with no database dependency.
/// </summary>
public sealed class MemoryEntry
{
    /// <summary>Gets or sets the unique identifier for this memory entry.</summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Gets or sets the session identifier that groups related entries into a conversation thread.
    /// </summary>
    public string SessionId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the role of the message author.
    /// Accepted values: <c>user</c>, <c>assistant</c>, <c>system</c>.
    /// </summary>
    public string Role { get; set; } = string.Empty;

    /// <summary>Gets or sets the text content of this memory entry.</summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the dense vector embedding for this entry, used for semantic recall.
    /// May be <c>null</c> when the entry has not been embedded yet.
    /// </summary>
    public float[]? Embedding { get; set; }

    /// <summary>Gets or sets the UTC timestamp at which this entry was created.</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Gets or sets arbitrary key/value metadata associated with this entry.</summary>
    public Dictionary<string, string> Metadata { get; set; } = new();
}
