using ArNir.Memory.InProcess;
using ArNir.Memory.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace ArNir.Memory.DependencyInjection;

/// <summary>
/// Provides extension methods for registering ArNir.Memory services into an
/// <see cref="IServiceCollection"/>.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers all ArNir.Memory services into the dependency-injection container.
    /// <para>
    /// Registered as <b>Singleton</b>: <see cref="InProcessEpisodicMemory"/> as <see cref="IEpisodicMemory"/>.
    /// </para>
    /// <para>
    /// Registered as <b>Singleton</b> (dev stub): <see cref="NullSemanticMemory"/> as <see cref="ISemanticMemory"/>.
    /// Replace with a real vector-store-backed implementation before production.
    /// </para>
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <returns>The same <see cref="IServiceCollection"/> instance for chaining.</returns>
    public static IServiceCollection AddArNirMemory(this IServiceCollection services)
    {
        // Episodic memory — Singleton (thread-safe ConcurrentDictionary backing store)
        services.AddSingleton<IEpisodicMemory, InProcessEpisodicMemory>();

        // Semantic memory — NullSemanticMemory dev stub (Singleton).
        // Replace with a real vector-store-backed implementation before production.
        services.AddSingleton<ISemanticMemory, NullSemanticMemory>();

        return services;
    }
}
