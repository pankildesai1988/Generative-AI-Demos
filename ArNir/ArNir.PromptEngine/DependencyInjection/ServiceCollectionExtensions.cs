using ArNir.PromptEngine.Interfaces;
using ArNir.PromptEngine.Resolution;
using Microsoft.Extensions.DependencyInjection;

namespace ArNir.PromptEngine.DependencyInjection;

/// <summary>
/// Provides extension methods for registering ArNir.PromptEngine services into an
/// <see cref="IServiceCollection"/>.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers all ArNir.PromptEngine services into the dependency-injection container.
    /// <para>
    /// Registered as <b>Singleton</b>: <see cref="CodePromptResolver"/> as <see cref="IPromptResolver"/>.
    /// <see cref="CodePromptResolver"/> is thread-safe (read-only dictionary) and carries no
    /// per-request state, making Singleton the correct lifetime.
    /// </para>
    /// <para>
    /// <b>NOT registered</b>: <see cref="IPromptVersionStore"/> — this interface represents the
    /// Database layer (Layer 1) of the 3-layer resolution chain and is an infrastructure concern.
    /// The consuming application must supply a concrete implementation (e.g. EF Core, Dapper, Redis)
    /// and register it separately.
    /// </para>
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <returns>The same <see cref="IServiceCollection"/> instance for method chaining.</returns>
    public static IServiceCollection AddArNirPromptEngine(this IServiceCollection services)
    {
        // Layer 3 (Code) resolver — Singleton, no infrastructure required
        services.AddSingleton<IPromptResolver, CodePromptResolver>();

        return services;
    }
}
