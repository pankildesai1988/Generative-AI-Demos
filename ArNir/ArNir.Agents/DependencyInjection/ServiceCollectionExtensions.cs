using ArNir.Agents.Agents;
using ArNir.Agents.Interfaces;
using ArNir.Agents.Registry;
using Microsoft.Extensions.DependencyInjection;

namespace ArNir.Agents.DependencyInjection;

/// <summary>
/// Provides extension methods for registering ArNir.Agents services into an
/// <see cref="IServiceCollection"/>.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers all ArNir.Agents services into the dependency-injection container.
    /// <para>
    /// Registered as <b>Singleton</b>: <see cref="ToolRegistry"/> as <see cref="IToolRegistry"/>.
    /// The registry is thread-safe (ConcurrentDictionary-backed) and holds no per-request state,
    /// making Singleton the correct lifetime.
    /// </para>
    /// <para>
    /// Registered as <b>Transient</b>: <see cref="PlannerAgent"/> as <see cref="IPlannerAgent"/>.
    /// The planner itself carries no mutable state — all plan state lives in the <c>AgentPlan</c>
    /// value object, so a fresh instance per resolution is safe and avoids inadvertent state leakage.
    /// </para>
    /// <para>
    /// <b>NOT registered here</b>:
    /// <list type="bullet">
    ///   <item><c>ISemanticMemory</c> — infrastructure concern; register via <c>AddArNirMemory()</c> or supply a custom implementation.</item>
    ///   <item><c>IEpisodicMemory</c> — infrastructure concern; register via <c>AddArNirMemory()</c>.</item>
    ///   <item><c>IPromptResolver</c> — register via <c>AddArNirPromptEngine()</c>.</item>
    /// </list>
    /// </para>
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <returns>The same <see cref="IServiceCollection"/> instance for method chaining.</returns>
    public static IServiceCollection AddArNirAgents(this IServiceCollection services)
    {
        // Singleton — shared, thread-safe tool catalogue
        services.AddSingleton<IToolRegistry, ToolRegistry>();

        // Transient — stateless orchestrator; plan state lives in AgentPlan
        services.AddTransient<IPlannerAgent, PlannerAgent>();

        return services;
    }
}
