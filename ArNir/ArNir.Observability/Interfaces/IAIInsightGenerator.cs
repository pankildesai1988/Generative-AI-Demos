namespace ArNir.Observability.Interfaces;

/// <summary>
/// Defines the contract for generating reactive, metrics-driven AI insights from
/// recorded observability data.
/// <para>
/// <b>CRITICAL — naming and coexistence:</b><br/>
/// This interface is named <c>IAIInsightGenerator</c> — NOT <c>IAIInsightService</c>.
/// <c>IAIInsightService</c> already exists in <c>ArNir.Services</c> with a different contract
/// and must remain unchanged during migration. Both interfaces coexist intentionally:
/// <list type="bullet">
///   <item><c>IAIInsightService</c> (ArNir.Services) — <b>agentic</b>: drives active AI-powered analysis workflows.</item>
///   <item><c>IAIInsightGenerator</c> (ArNir.Observability) — <b>reactive</b>: derives insights from collected <c>MetricEvent</c> data.</item>
/// </list>
/// </para>
/// <para>
/// Implementations analyse the metric history returned by <c>IMetricCollector.QueryAsync</c>
/// and produce plain-language insight strings (e.g. latency trends, SLA breach rates,
/// token usage anomalies, provider comparisons).
/// </para>
/// </summary>
public interface IAIInsightGenerator
{
    /// <summary>
    /// Generates a list of human-readable insight strings derived from the recorded
    /// <c>MetricEvent</c> history, optionally scoped to a provider and/or time window.
    /// </summary>
    /// <param name="provider">
    /// Optional provider filter (e.g. <c>OpenAI</c>, <c>Gemini</c>, <c>Claude</c>).
    /// When <c>null</c>, insights are generated across all providers.
    /// </param>
    /// <param name="start">
    /// Optional inclusive start of the analysis window (UTC).
    /// When <c>null</c>, the full recorded history is analysed.
    /// </param>
    /// <param name="end">
    /// Optional inclusive end of the analysis window (UTC).
    /// When <c>null</c>, the analysis extends to the most recent event.
    /// </param>
    /// <param name="ct">A cancellation token.</param>
    /// <returns>
    /// A read-only list of insight strings. Each string is a self-contained, actionable
    /// observation (e.g. <c>"OpenAI p95 latency exceeded 5 000 ms in 12 % of calls this hour."</c>).
    /// Returns an empty list when insufficient data is available to generate insights.
    /// </returns>
    Task<IReadOnlyList<string>> GenerateInsightsAsync(
        string? provider     = null,
        DateTime? start      = null,
        DateTime? end        = null,
        CancellationToken ct = default);
}
