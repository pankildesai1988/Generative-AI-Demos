namespace ArNir.RAG.Models;

/// <summary>
/// Represents a single text chunk derived from a <see cref="RagDocument"/> during the chunking phase.
/// Intentionally separate from <c>ArNir.Core.Entities.DocumentChunk</c> — pure in-memory model with no EF dependency.
/// </summary>
public sealed class RagChunk
{
    /// <summary>Gets or sets the unique identifier for this chunk.</summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>Gets or sets the identifier of the parent <see cref="RagDocument"/>.</summary>
    public Guid DocumentId { get; set; }

    /// <summary>Gets or sets the zero-based index of this chunk within the document.</summary>
    public int ChunkIndex { get; set; }

    /// <summary>Gets or sets the raw text content of this chunk.</summary>
    public string Text { get; set; } = string.Empty;

    /// <summary>Gets or sets the approximate token count for this chunk.</summary>
    public int TokenCount { get; set; }

    /// <summary>Gets or sets arbitrary key/value metadata associated with this chunk.</summary>
    public Dictionary<string, string> Metadata { get; set; } = new();
}
