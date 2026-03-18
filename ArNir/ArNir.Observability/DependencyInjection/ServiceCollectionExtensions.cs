using ArNir.Observability.Rules;
using Microsoft.Extensions.DependencyInjection;

namespace ArNir.Observability.DependencyInjection;

/// <summary>
/// Provides extension methods for registering ArNir.Observability services into an
/// <see cref="IServiceCollection"/>.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers all ArNir.Observability services into the dependency-injection container.
    /// <para>
    /// Registered as <b>Singleton</b>: <see cref="SlaAlertRule"/> with the default
    /// 5 000 ms threshold. Override by registering your own instance before calling this method.
    /// </para>
    /// <para>
    /// <b>NOT registered here</b> (infrastructure concerns — supply concrete implementations):
    /// <list type="bullet">
    ///   <item><c>IMetricCollector</c> — requires a backing store (in-memory, time-series DB, etc.).</item>
    ///   <item><c>IAIInsightGenerator</c> — requires access to <c>IMetricCollector</c>; register alongside it.</item>
    ///   <item><c>IEvaluationService</c> — may require an LLM client (LLM-as-judge) or heuristic implementation.</item>
    /// </list>
    /// </para>
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <returns>The same <see cref="IServiceCollection"/> instance for method chaining.</returns>
    public static IServiceCollection AddArNirObservability(this IServiceCollection services)
    {
        // Singleton — stateless rule; default 5 000 ms SLA threshold
        services.AddSingleton<SlaAlertRule>();

        return services;
    }

    /// <summary>
    /// Registers ArNir.Observability services with a custom SLA latency threshold.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <param name="slaThresholdMs">
    /// The SLA latency threshold in milliseconds. Must be greater than zero.
    /// </param>
    /// <returns>The same <see cref="IServiceCollection"/> instance for method chaining.</returns>
    public static IServiceCollection AddArNirObservability(
        this IServiceCollection services,
        long slaThresholdMs)
    {
        services.AddSingleton(new SlaAlertRule(slaThresholdMs));

        return services;
    }
}
