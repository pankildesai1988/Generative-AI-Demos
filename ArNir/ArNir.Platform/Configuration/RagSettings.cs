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
    /// Defaults to <see cref="ApplicationConstants.DefaultChunkOverlap"/>.
    /// </summary>
    public int ChunkOverlap { get; set; } = ApplicationConstants.DefaultChunkOverlap;

    /// <summary>
    /// Gets or sets the OpenAI model used to generate text embeddings (e.g. <c>text-embedding-3-small</c>).
    /// </summary>
    public string EmbeddingModel { get; set; } = "text-embedding-3-small";

    /// <summary>
    /// Gets or sets the minimum cosine-similarity score (0–1) a chunk must achieve to be included
    /// in the retrieved context. Chunks below this threshold are discarded.
    /// Defaults to <see cref="ApplicationConstants.DefaultScoreThreshold"/>.
    /// </summary>
    public double SimilarityThreshold { get; set; } = ApplicationConstants.DefaultScoreThreshold;

    /// <summary>
    /// Gets or sets the fixed relevance score assigned to keyword (full-text) matches during
    /// hybrid retrieval. Defaults to <see cref="ApplicationConstants.DefaultKeywordMatchScore"/>.
    /// </summary>
    public double KeywordMatchScore { get; set; } = ApplicationConstants.DefaultKeywordMatchScore;

    /// <summary>
    /// Gets or sets the weight applied to the best per-chunk score when merging semantic and
    /// keyword results. Defaults to <see cref="ApplicationConstants.DefaultHybridVectorWeight"/>.
    /// </summary>
    public double HybridVectorWeight { get; set; } = ApplicationConstants.DefaultHybridVectorWeight;

    /// <summary>
    /// Gets or sets the bonus added to a chunk's merged hybrid score when it also appears in the
    /// keyword result set. Defaults to <see cref="ApplicationConstants.DefaultHybridKeywordBonus"/>.
    /// </summary>
    public double HybridKeywordBonus { get; set; } = ApplicationConstants.DefaultHybridKeywordBonus;

    /// <summary>
    /// Gets or sets the character count below which a parsed document is flagged <c>LowText</c>
    /// (likely an image-only / scanned PDF). Defaults to <see cref="ApplicationConstants.DefaultLowTextThreshold"/>.
    /// </summary>
    public int LowTextThreshold { get; set; } = ApplicationConstants.DefaultLowTextThreshold;

    /// <summary>
    /// Gets or sets the maximum number of context tokens allowed before the RAG context block
    /// is trimmed. Defaults to <see cref="ApplicationConstants.DefaultMaxContextTokens"/>.
    /// </summary>
    public int MaxContextTokens { get; set; } = ApplicationConstants.DefaultMaxContextTokens;

    /// <summary>
    /// Gets or sets the top-score cutoff at or above which retrieval confidence is <c>high</c>.
    /// Defaults to <see cref="ApplicationConstants.DefaultConfidenceHigh"/>.
    /// </summary>
    public double ConfidenceHigh { get; set; } = ApplicationConstants.DefaultConfidenceHigh;

    /// <summary>
    /// Gets or sets the top-score cutoff at or above which retrieval confidence is <c>medium</c>
    /// (below it is <c>low</c>). Defaults to <see cref="ApplicationConstants.DefaultConfidenceMedium"/>.
    /// </summary>
    public double ConfidenceMedium { get; set; } = ApplicationConstants.DefaultConfidenceMedium;

    /// <summary>
    /// Gets or sets whether vision-based OCR fallback runs for pages that yield no extractable
    /// text. Off by default — OCR adds latency and cost. (Wired in a later phase.)
    /// </summary>
    public bool EnableVisionOcr { get; set; } = false;

    /// <summary>
    /// Gets or sets the minimum chunk size in characters. A page's trailing chunk shorter than
    /// <c>min(MinChunkSize, ChunkSize / 3)</c> is merged into the preceding chunk of the same page.
    /// Defaults to <see cref="ApplicationConstants.DefaultMinChunkSize"/>.
    /// </summary>
    public int MinChunkSize { get; set; } = ApplicationConstants.DefaultMinChunkSize;

    /// <summary>
    /// Gets or sets the document chunking strategy used during ingestion.
    /// Supported values: <c>sliding</c> (fixed-size sliding window) and <c>sentence</c>
    /// (sentence-boundary-aware packing — chunks never split mid-sentence).
    /// Defaults to <see cref="ApplicationConstants.DefaultChunkingStrategy"/>.
    /// </summary>
    public string ChunkingStrategy { get; set; } = ApplicationConstants.DefaultChunkingStrategy;
}
