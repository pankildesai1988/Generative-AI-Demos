namespace ArNir.Platform.Constants;

/// <summary>
/// Application-wide constants used across the ArNir platform.
/// All values are intentionally immutable compile-time constants.
/// </summary>
public static class ApplicationConstants
{
    /// <summary>
    /// Default number of document chunks (top-K) to retrieve during a RAG similarity search.
    /// </summary>
    public const int DefaultTopK = 5;

    /// <summary>
    /// Default size (in characters) of each text chunk when splitting documents for embedding.
    /// </summary>
    public const int DefaultChunkSize = 500;

    /// <summary>
    /// Default SLA threshold in milliseconds. Responses that exceed this value are flagged
    /// as <see cref="ArNir.Platform.Enums.SlaStatusEnum.Critical"/>.
    /// </summary>
    public const int DefaultSlaThresholdMs = 5000;

    /// <summary>
    /// Default number of characters that consecutive chunks overlap. Overlap preserves context
    /// that would otherwise be split across a chunk boundary.
    /// </summary>
    public const int DefaultChunkOverlap = 50;

    /// <summary>
    /// Default minimum cosine-similarity score (0–1) a retrieved chunk must reach to survive
    /// the relevance filter. Chunks below this are discarded before prompt construction.
    /// </summary>
    public const double DefaultScoreThreshold = 0.45;

    /// <summary>
    /// Default character count below which a parsed document is flagged <c>LowText</c>
    /// (likely an image-only / scanned PDF that yielded no extractable text).
    /// </summary>
    public const int DefaultLowTextThreshold = 100;

    /// <summary>
    /// Default maximum number of context tokens allowed before the RAG context block is trimmed.
    /// </summary>
    public const int DefaultMaxContextTokens = 6000;

    /// <summary>
    /// Default top-score cutoff at or above which retrieval confidence is reported as <c>high</c>.
    /// </summary>
    public const double DefaultConfidenceHigh = 0.85;

    /// <summary>
    /// Default top-score cutoff at or above which retrieval confidence is reported as <c>medium</c>
    /// (below it is <c>low</c>).
    /// </summary>
    public const double DefaultConfidenceMedium = 0.65;

    /// <summary>
    /// Default document chunking strategy. <c>sliding</c> = fixed-size sliding window;
    /// <c>sentence</c> = sentence-boundary-aware packing (see <c>SentenceAwareChunker</c>).
    /// </summary>
    public const string DefaultChunkingStrategy = "sliding";

    /// <summary>
    /// Chunking strategy name for the fixed-size sliding window chunker.
    /// </summary>
    public const string ChunkingStrategySliding = "sliding";

    /// <summary>
    /// Chunking strategy name for the sentence-boundary-aware chunker.
    /// </summary>
    public const string ChunkingStrategySentence = "sentence";
}
