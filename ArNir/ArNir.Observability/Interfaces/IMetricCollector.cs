using ArNir.Observability.Models;

namespace ArNir.Observability.Interfaces;

/// <summary>
/// Defines the contract for recording and querying <see cref="MetricEvent"/> observations.
/// <para>
/// Implementations are responsible for persisting events to a backing store (e.g. in-memory
/// list, time-series database, or distributed telemetry sink) and exposing them for
/// time-range and provider-scoped queries.
/// </para>
/// <para>
/// <b>NOT registered by default</b> in the DI container — the backing store is an
/// infrastructure concern supplied by the consuming application.
/// </para>
/// </summary>
public interface IMetricCollector
{
    /// <summary>
    /// Records a <see cref="MetricEvent"/> to the backing store.
    /// </summary>
    /// <param name="metricEvent">The event to record. Must not be <c>null</c>.</param>
    /// <param name="ct">A cancellation token.</param>
    Task RecordAsync(MetricEvent metricEvent, CancellationToken ct = default);

    /// <summary>
    /// Queries recorded events, optionally filtered by provider and/or time window.
    /// </summary>
    /// <param name="provider">
    /// Optional provider filter (e.g. <c>OpenAI</c>, <c>Gemini</c>, <c>Claude</c>).
    /// When <c>null</c>, events from all providers are returned.
    /// </param>
    /// <param name="start">
    /// Optional inclusive start of the time window (UTC).
    /// When <c>null</c>, no lower time bound is applied.
    /// </param>
    /// <param name="end">
    /// Optional inclusive end of the time window (UTC).
    /// When <c>null</c>, no upper time bound is applied.
    /// </param>
    /// <param name="ct">A cancellation token.</param>
    /// <returns>
    /// A read-only list of <see cref="MetricEvent"/> objects matching the supplied filters,
    /// ordered by <see cref="MetricEvent.OccurredAt"/> ascending.
    /// </returns>
    Task<IReadOnlyList<MetricEvent>> QueryAsync(
        string? provider = null,
        DateTime? start  = null,
        DateTime? end    = null,
        CancellationToken ct = default);
}
