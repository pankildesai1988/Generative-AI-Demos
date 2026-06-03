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
    /// Gets or sets the extracted text content for this page.
    /// </summary>
    public string Text { get; set; } = string.Empty;
}
