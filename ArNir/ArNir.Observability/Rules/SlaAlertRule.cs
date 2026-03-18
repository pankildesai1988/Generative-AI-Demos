using ArNir.Observability.Models;

namespace ArNir.Observability.Rules;

/// <summary>
/// Evaluates whether a <see cref="MetricEvent"/> violates the configured SLA latency threshold
/// and produces a human-readable alert message when it does.
/// <para>
/// The default threshold is <b>5 000 ms</b>. Inject or construct with a custom
/// <see cref="ThresholdMs"/> to override this value for specific providers or environments.
/// </para>
/// <para>
/// Typical usage — stamp <see cref="MetricEvent.IsWithinSla"/> at recording time:
/// <code>
/// var rule = new SlaAlertRule();          // or inject via DI
/// evt.IsWithinSla = !rule.IsViolated(evt);
/// if (rule.IsViolated(evt))
///     logger.LogWarning(rule.GetAlertMessage(evt));
/// await collector.RecordAsync(evt);
/// </code>
/// </para>
/// </summary>
public sealed class SlaAlertRule
{
    /// <summary>Default SLA latency threshold in milliseconds.</summary>
    public const long DefaultThresholdMs = 5_000;

    /// <summary>
    /// Gets the latency threshold in milliseconds above which a <see cref="MetricEvent"/>
    /// is considered an SLA violation.
    /// </summary>
    public long ThresholdMs { get; }

    /// <summary>
    /// Initialises a new instance of <see cref="SlaAlertRule"/> with the default
    /// threshold of <see cref="DefaultThresholdMs"/> (5 000 ms).
    /// </summary>
    public SlaAlertRule() : this(DefaultThresholdMs) { }

    /// <summary>
    /// Initialises a new instance of <see cref="SlaAlertRule"/> with a custom threshold.
    /// </summary>
    /// <param name="thresholdMs">
    /// The latency threshold in milliseconds. Must be greater than zero.
    /// </param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="thresholdMs"/> is less than or equal to zero.
    /// </exception>
    public SlaAlertRule(long thresholdMs)
    {
        if (thresholdMs <= 0)
            throw new ArgumentOutOfRangeException(
                nameof(thresholdMs),
                thresholdMs,
                "SLA threshold must be greater than zero milliseconds.");

        ThresholdMs = thresholdMs;
    }

    /// <summary>
    /// Determines whether the <paramref name="metricEvent"/> violates the SLA threshold.
    /// </summary>
    /// <param name="metricEvent">The event to evaluate. Must not be <c>null</c>.</param>
    /// <returns>
    /// <c>true</c> when <see cref="MetricEvent.LatencyMs"/> exceeds <see cref="ThresholdMs"/>;
    /// <c>false</c> otherwise.
    /// </returns>
    public bool IsViolated(MetricEvent metricEvent) =>
        metricEvent.LatencyMs > ThresholdMs;

    /// <summary>
    /// Produces a descriptive alert message for an SLA-violating <paramref name="metricEvent"/>.
    /// </summary>
    /// <param name="metricEvent">The event that triggered the violation.</param>
    /// <returns>
    /// A human-readable alert string including the provider, model, actual latency,
    /// and configured threshold. Example:
    /// <c>"SLA VIOLATION — Provider: OpenAI | Model: gpt-4o | Latency: 6 120 ms (threshold: 5 000 ms) | OccurredAt: 2026-03-18T10:00:00Z"</c>
    /// </returns>
    public string GetAlertMessage(MetricEvent metricEvent) =>
        $"SLA VIOLATION — " +
        $"Provider: {metricEvent.Provider} | " +
        $"Model: {metricEvent.Model} | " +
        $"EventType: {metricEvent.EventType} | " +
        $"Latency: {metricEvent.LatencyMs:N0} ms (threshold: {ThresholdMs:N0} ms) | " +
        $"OccurredAt: {metricEvent.OccurredAt:O}";
}
