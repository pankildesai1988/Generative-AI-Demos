namespace ArNir.RAG.Models;

/// <summary>
/// Encapsulates all parameters required to ingest a document through the RAG pipeline.
/// </summary>
public sealed class IngestionRequest
{
    /// <summary>Gets or sets the readable stream containing the raw document bytes.</summary>
    public Stream FileStream { get; set; } = Stream.Null;

    /// <summary>Gets or sets the original file name, including extension.</summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>Gets or sets the MIME content type of the document (e.g. "application/pdf").</summary>
    public string ContentType { get; set; } = string.Empty;

    /// <summary>Gets or sets the optional identifier of the user who uploaded the document.</summary>
    public string? UploadedBy { get; set; }

    /// <summary>
    /// Gets or sets the embedding model to use when generating vectors for chunks.
    /// Defaults to <c>"text-embedding-ada-002"</c>.
    /// </summary>
    public string EmbeddingModel { get; set; } = "text-embedding-ada-002";
}

/// <summary>
/// Represents the outcome of a document ingestion operation.
/// </summary>
public sealed class IngestionResult
{
    /// <summary>Gets or sets a value indicating whether the ingestion completed successfully.</summary>
    public bool Success { get; set; }

    /// <summary>Gets or sets the identifier of the ingested document, if successful.</summary>
    public Guid DocumentId { get; set; }

    /// <summary>Gets or sets the number of text chunks created from the document.</summary>
    public int ChunksCreated { get; set; }

    /// <summary>Gets or sets the number of embedding vectors generated and stored.</summary>
    public int EmbeddingsCreated { get; set; }

    /// <summary>Gets or sets a human-readable error message when <see cref="Success"/> is <c>false</c>.</summary>
    public string? ErrorMessage { get; set; }
}
