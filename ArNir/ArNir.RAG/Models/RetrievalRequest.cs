namespace ArNir.RAG.Models;

/// <summary>
/// Encapsulates the parameters for a vector similarity retrieval query.
/// </summary>
public sealed class RetrievalRequest
{
    /// <summary>Gets or sets the natural-language query to embed and search against the vector store.</summary>
    public string Query { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the maximum number of results to return.
    /// Defaults to <c>5</c>.
    /// </summary>
    public int TopK { get; set; } = 5;

    /// <summary>
    /// Gets or sets a value indicating whether hybrid (semantic + keyword) retrieval should be used.
    /// Defaults to <c>false</c> (semantic only).
    /// </summary>
    public bool UseHybrid { get; set; } = false;

    /// <summary>Gets or sets the embedding model used to vectorise the query.</summary>
    public string EmbeddingModel { get; set; } = "text-embedding-ada-002";
}

/// <summary>
/// Represents a single result item returned from a retrieval operation.
/// </summary>
public sealed class RetrievalResult
{
    /// <summary>Gets or sets the unique identifier of the source chunk.</summary>
    public Guid ChunkId { get; set; }

    /// <summary>Gets or sets the identifier of the document that contains this chunk.</summary>
    public Guid DocumentId { get; set; }

    /// <summary>Gets or sets the raw text of the retrieved chunk.</summary>
    public string Text { get; set; } = string.Empty;

    /// <summary>Gets or sets the similarity score (higher is more relevant).</summary>
    public double Score { get; set; }

    /// <summary>
    /// Gets or sets the retrieval strategy that produced this result.
    /// Defaults to <c>"Semantic"</c>.
    /// </summary>
    public string Source { get; set; } = "Semantic";

    /// <summary>Gets or sets arbitrary key/value metadata associated with this result.</summary>
    public Dictionary<string, string> Metadata { get; set; } = new();
}
