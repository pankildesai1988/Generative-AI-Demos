namespace ArNir.Platform.Enums;

/// <summary>
/// Represents the SLA (Service Level Agreement) health status of an AI provider response.
/// </summary>
public enum SlaStatusEnum
{
    /// <summary>
    /// Response latency is within the acceptable threshold — the provider is performing normally.
    /// </summary>
    Ok,

    /// <summary>
    /// Response latency is elevated but has not yet breached the critical threshold.
    /// Monitoring should be increased.
    /// </summary>
    Slow,

    /// <summary>
    /// Response latency has exceeded the critical SLA threshold.
    /// Alerts should be raised and fallback behaviour considered.
    /// </summary>
    Critical
}
