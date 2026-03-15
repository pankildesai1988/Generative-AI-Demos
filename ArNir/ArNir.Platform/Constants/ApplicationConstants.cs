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
}
