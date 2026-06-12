namespace ArNir.Core.Models.Chunking;

/// <summary>
/// Well-known values for <see cref="ExtractedChunk.ChunkType"/>. Stored verbatim in
/// <c>DocumentChunk.ChunkType</c> so the retrieval DTOs and the PDF viewer can colour-code chunks.
/// </summary>
public static class ChunkTypes
{
    /// <summary>A plain prose chunk produced by the text chunking strategy.</summary>
    public const string Text = "text";

    /// <summary>A chunk produced from detected table rows converted to natural-language sentences.</summary>
    public const string Table = "table";

    /// <summary>A placeholder chunk standing in for an embedded image (no vision captioning yet).</summary>
    public const string Image = "image";
}

/// <summary>
/// A single retrieval-unit chunk produced by the unified chunk extractor. Plain POCO shared by
/// both ingestion paths: <c>DocumentService</c> persists it as a <c>DocumentChunk</c> row
/// (<see cref="Index"/> ⇒ <c>ChunkOrder</c>) and <c>IngestionPipeline</c> embeds it under the
/// same index (<c>"sql:{docId}:{Index}"</c>), guaranteeing the embedding↔chunk FK alignment.
/// </summary>
public sealed class ExtractedChunk
{
    /// <summary>Gets or sets the zero-based position of this chunk within the document's chunk sequence.</summary>
    public int Index { get; set; }

    /// <summary>Gets or sets the chunk text (prose, row-sentence, or image placeholder).</summary>
    public string Text { get; set; } = string.Empty;

    /// <summary>Gets or sets the 1-based source page number, or <c>null</c> when unknown.</summary>
    public int? PageNumber { get; set; }

    /// <summary>Gets or sets the chunk modality — one of <see cref="ChunkTypes"/>.</summary>
    public string ChunkType { get; set; } = ChunkTypes.Text;

    /// <summary>Gets or sets the left X of the source region's bounding box (PDF points), if known.</summary>
    public float? BboxX1 { get; set; }

    /// <summary>Gets or sets the bottom Y of the source region's bounding box (PDF points), if known.</summary>
    public float? BboxY1 { get; set; }

    /// <summary>Gets or sets the right X of the source region's bounding box (PDF points), if known.</summary>
    public float? BboxX2 { get; set; }

    /// <summary>Gets or sets the top Y of the source region's bounding box (PDF points), if known.</summary>
    public float? BboxY2 { get; set; }

    /// <summary>Gets or sets the approximate token count (length / 4 heuristic).</summary>
    public int TokenCount { get; set; }
}

/// <summary>
/// The result of running the unified chunk extractor over one uploaded file: the ordered chunk
/// sequence plus document-level parse signals.
/// </summary>
public sealed class ChunkExtractionResult
{
    /// <summary>Gets or sets the pipeline-side document identifier assigned during parsing.</summary>
    public Guid DocumentId { get; set; }

    /// <summary>Gets or sets the number of pages the parser produced (1 for non-paginated formats).</summary>
    public int PageCount { get; set; }

    /// <summary>
    /// Gets or sets whether the parser flagged the document as low-text
    /// (likely an image-only / scanned PDF).
    /// </summary>
    public bool LowText { get; set; }

    /// <summary>Gets or sets the ordered chunk sequence; <see cref="ExtractedChunk.Index"/> is sequential from 0.</summary>
    public IReadOnlyList<ExtractedChunk> Chunks { get; set; } = Array.Empty<ExtractedChunk>();
}
