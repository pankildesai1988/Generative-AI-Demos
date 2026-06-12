namespace ArNir.RAG.Models;

/// <summary>
/// Represents the extracted text content of a single page within a parsed <see cref="RagDocument"/>.
/// Used by paginated parsers (e.g. PDF) so that each downstream <see cref="RagChunk"/> can be
/// attributed to its source page number.
/// </summary>
public sealed class RagPageContent
{
    /// <summary>
    /// Gets or sets the 1-based page number within the source document.
    /// </summary>
    public int PageNumber { get; set; }

    /// <summary>
    /// Gets or sets the extracted text content for this page. When tables were detected,
    /// their words are excluded here (they become dedicated table chunks instead).
    /// </summary>
    public string Text { get; set; } = string.Empty;

    /// <summary>Gets or sets the tables detected on this page (empty for non-PDF formats).</summary>
    public List<RagTable> Tables { get; set; } = new();

    /// <summary>Gets or sets references to the images embedded on this page.</summary>
    public List<RagImageRef> Images { get; set; } = new();
}
