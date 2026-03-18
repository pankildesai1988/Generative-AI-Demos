using ArNir.Platform.Constants;

namespace ArNir.Platform.Configuration;

/// <summary>
/// Strongly-typed configuration settings for the Retrieval-Augmented Generation (RAG) pipeline.
/// Bind this class to the <c>Rag</c> section of <c>appsettings.json</c>.
/// </summary>
public sealed class RagSettings
{
    /// <summary>
    /// Configuration section key used when binding from <see cref="Microsoft.Extensions.Configuration.IConfiguration"/>.
    /// </summary>
    public const string SectionName = "Rag";

    /// <summary>
    /// Gets or sets the number of nearest-neighbour chunks to retrieve for each query.
    /// Defaults to <see cref="ApplicationConstants.DefaultTopK"/>.
    /// </summary>
    public int TopK { get; set; } = ApplicationConstants.DefaultTopK;

    /// <summary>
    /// Gets or sets the character length of each document chunk produced during ingestion.
    /// Defaults to <see cref="ApplicationConstants.DefaultChunkSize"/>.
    /// </summary>
    public int ChunkSize { get; set; } = ApplicationConstants.DefaultChunkSize;

    /// <summary>
    /// Gets or sets the number of characters that consecutive chunks may overlap.
    /// Overlap helps preserve context that would otherwise be split across chunk boundaries.
    /// </summary>
    public int ChunkOverlap { get; set; } = 50;

    /// <summary>
    /// Gets or sets the OpenAI model used to generate text embeddings (e.g. <c>text-embedding-3-small</c>).
    /// </summary>
    public string EmbeddingModel { get; set; } = "text-embedding-3-small";

    /// <summary>
    /// Gets or sets the minimum cosine-similarity score (0–1) a chunk must achieve to be included
    /// in the retrieved context. Chunks below this threshold are discarded.
    /// </summary>
    public double SimilarityThreshold { get; set; } = 0.75;

    /// <summary>
    /// Gets or sets whether hybrid retrieval (vector + keyword BM25) is enabled.
    /// When <see langword="false"/> only vector similarity search is performed.
    /// </summary>
    public bool EnableHybridRetrieval { get; set; } = false;
}
