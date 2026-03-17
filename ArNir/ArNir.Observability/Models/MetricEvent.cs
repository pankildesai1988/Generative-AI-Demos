namespace ArNir.Observability.Models;

/// <summary>
/// Represents a single observable metric event emitted by an AI provider interaction.
/// <para>
/// <see cref="MetricEvent"/> is the core data unit flowing through the observability pipeline.
/// It is produced at the call site (e.g. after each LLM response), persisted via
/// <c>IMetricCollector.RecordAsync</c>, and later queried for trend analysis and SLA alerting.
/// </para>
/// </summary>
public sealed class MetricEvent
{
    /// <summary>
    /// Gets or sets the category of event that occurred.
    /// Common values: <c>LlmCall</c>, <c>EmbeddingCall</c>, <c>RagQuery</c>, <c>AgentStep</c>.
    /// </summary>
    public string EventType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the AI provider that handled the request.
    /// Expected values align with <c>ArNir.Platform.Enums.ProviderEnum</c>: <c>OpenAI</c>, <c>Gemini</c>, <c>Claude</c>.
    /// </summary>
    public string Provider { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the specific model identifier used for the request
    /// (e.g. <c>gpt-4o</c>, <c>gemini-1.5-pro</c>, <c>claude-3-5-sonnet</c>).
    /// </summary>
    public string Model { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the end-to-end latency of the AI call in milliseconds.
    /// Used by <c>SlaAlertRule</c> to determine SLA compliance.
    /// </summary>
    public long LatencyMs { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this event's latency fell within the
    /// configured SLA threshold. Set by the recording layer after applying <c>SlaAlertRule</c>.
    /// </summary>
    public bool IsWithinSla { get; set; }

    /// <summary>
    /// Gets or sets the total number of tokens consumed by the request (prompt + completion).
    /// <c>0</c> when token counts are not available (e.g. streaming or embedding calls).
    /// </summary>
    public int TokensUsed { get; set; }

    /// <summary>Gets or sets the UTC timestamp at which this event occurred.</summary>
    public DateTime OccurredAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets arbitrary key/value tags for filtering and grouping during query.
    /// Examples: <c>{"session": "abc123"}</c>, <c>{"feature": "rag-query"}</c>.
    /// </summary>
    public Dictionary<string, string> Tags { get; set; } = new();
}
