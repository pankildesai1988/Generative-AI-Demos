namespace ArNir.RAG.Models;

/// <summary>
/// Represents a parsed document within the RAG ingestion pipeline.
/// Intentionally separate from <c>ArNir.Core.Entities.Document</c> — carries no EF dependency.
/// </summary>
public sealed class RagDocument
{
    /// <summary>Gets or sets the unique identifier for this document.</summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>Gets or sets the original file name of the document.</summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>Gets or sets the MIME content type (e.g. "application/pdf").</summary>
    public string ContentType { get; set; } = string.Empty;

    /// <summary>Gets or sets the full extracted text content of the document.</summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>Gets or sets the size of the original file in bytes.</summary>
    public long FileSizeBytes { get; set; }

    /// <summary>Gets or sets the UTC timestamp at which the document was parsed.</summary>
    public DateTime ParsedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Gets or sets arbitrary key/value metadata associated with the document.</summary>
    public Dictionary<string, string> Metadata { get; set; } = new();
}
