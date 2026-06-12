namespace ArNir.Platform.Constants;

/// <summary>
/// Application-wide constants used across the ArNir platform.
/// All values are intentionally immutable compile-time constants.
/// </summary>
public static class ApplicationConstants
{
    /// <summary>
    /// Default number of document chunks (top-K) to retrieve during a RAG similarity search.
    /// 10 rather than 3–5: multi-row comparison questions ("which models support X?") silently
    /// fail when top-K is smaller than the expected answer set.
    /// </summary>
    public const int DefaultTopK = 10;

    /// <summary>
    /// Default size (in characters) of each text chunk when splitting documents for embedding.
    /// </summary>
    public const int DefaultChunkSize = 600;

    /// <summary>
    /// Default SLA threshold in milliseconds. Responses that exceed this value are flagged
    /// as <see cref="ArNir.Platform.Enums.SlaStatusEnum.Critical"/>.
    /// </summary>
    public const int DefaultSlaThresholdMs = 5000;

    /// <summary>
    /// Default number of characters that consecutive chunks overlap. Overlap preserves context
    /// that would otherwise be split across a chunk boundary.
    /// </summary>
    public const int DefaultChunkOverlap = 100;

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
    /// Default minimum chunk size in characters. A page's trailing chunk shorter than
    /// <c>min(DefaultMinChunkSize, chunkSize / 3)</c> is merged into the previous chunk of the
    /// same page so fragment chunks don't pollute the embedding space.
    /// </summary>
    public const int DefaultMinChunkSize = 200;

    /// <summary>
    /// Default document chunking strategy. <c>sliding</c> = fixed-size sliding window;
    /// <c>sentence</c> = sentence-boundary-aware packing (see <c>SentenceAwareChunker</c>).
    /// </summary>
    public const string DefaultChunkingStrategy = "sentence";

    /// <summary>
    /// Chunking strategy name for the fixed-size sliding window chunker.
    /// </summary>
    public const string ChunkingStrategySliding = "sliding";

    /// <summary>
    /// Chunking strategy name for the sentence-boundary-aware chunker.
    /// </summary>
    public const string ChunkingStrategySentence = "sentence";

    /// <summary>
    /// Default fixed relevance score assigned to keyword (full-text) matches during hybrid
    /// retrieval — a keyword hit is good but not a perfect semantic match.
    /// </summary>
    public const double DefaultKeywordMatchScore = 0.75;

    /// <summary>
    /// Default weight applied to the best per-chunk score when merging semantic and keyword
    /// results during hybrid retrieval.
    /// </summary>
    public const double DefaultHybridVectorWeight = 0.8;

    /// <summary>
    /// Default bonus added to a chunk's merged hybrid score when it also appears in the
    /// keyword result set.
    /// </summary>
    public const double DefaultHybridKeywordBonus = 0.2;
}
