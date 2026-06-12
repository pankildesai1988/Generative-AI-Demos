namespace ArNir.RAG.Models;

/// <summary>
/// A table detected on a parsed page: header cells, data rows, and the table's bounding-box
/// region in PDF points (nullable — extraction may not always yield a region).
/// In-memory model only; table content reaches persistence as row-sentence chunks
/// (<c>ChunkType = "table"</c>) produced by the unified chunk extractor.
/// </summary>
public sealed class RagTable
{
    /// <summary>Gets or sets the header cell texts (the table's first detected row).</summary>
    public List<string> Headers { get; set; } = new();

    /// <summary>Gets or sets the data rows; each row is an ordered list of cell texts.</summary>
    public List<List<string>> Rows { get; set; } = new();

    /// <summary>Gets or sets the left X of the table region, if known.</summary>
    public float? X1 { get; set; }

    /// <summary>Gets or sets the bottom Y of the table region, if known.</summary>
    public float? Y1 { get; set; }

    /// <summary>Gets or sets the right X of the table region, if known.</summary>
    public float? X2 { get; set; }

    /// <summary>Gets or sets the top Y of the table region, if known.</summary>
    public float? Y2 { get; set; }
}

/// <summary>
/// A reference to an embedded image on a parsed page. Only the per-page index is recorded —
/// the unified chunk extractor emits a placeholder chunk per image
/// (<c>ChunkType = "image"</c>, bbox null) until vision captioning lands.
/// </summary>
public sealed class RagImageRef
{
    /// <summary>Gets or sets the 1-based index of the image within its page.</summary>
    public int Index { get; set; }
}
