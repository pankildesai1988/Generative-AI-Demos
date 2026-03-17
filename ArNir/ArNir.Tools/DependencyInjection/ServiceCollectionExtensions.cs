using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ArNir.Tools.DependencyInjection;

/// <summary>
/// Provides extension methods for registering ArNir.Tools services into an
/// <see cref="IServiceCollection"/>.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers all ArNir.Tools implementations into the dependency-injection container.
    /// <para>
    /// Registered tools:
    /// <list type="bullet">
    ///   <item>
    ///     <term><see cref="DocumentLookupTool"/></term>
    ///     <description>Registered as <b>Singleton</b>. Stateless placeholder; delegates to the RAG pipeline in a later phase.</description>
    ///   </item>
    ///   <item>
    ///     <term><see cref="WebFetchTool"/></term>
    ///     <description>Registered as <b>Singleton</b>. Uses a named <see cref="System.Net.Http.HttpClient"/> (<c>"WebFetch"</c>) with a 30-second timeout.</description>
    ///   </item>
    /// </list>
    /// </para>
    /// <para>
    /// To make these tools available to <c>IPlannerAgent</c>, resolve <c>IToolRegistry</c>
    /// from the container and call <c>Register</c> for each tool after the host is built:
    /// <code>
    /// var registry = app.Services.GetRequiredService&lt;IToolRegistry&gt;();
    /// registry.Register(app.Services.GetRequiredService&lt;DocumentLookupTool&gt;());
    /// registry.Register(app.Services.GetRequiredService&lt;WebFetchTool&gt;());
    /// </code>
    /// </para>
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <returns>The same <see cref="IServiceCollection"/> instance for method chaining.</returns>
    public static IServiceCollection AddArNirTools(this IServiceCollection services)
    {
        // Named HttpClient for WebFetchTool — 30-second timeout, descriptive User-Agent
        services.AddHttpClient("WebFetch", client =>
        {
            client.Timeout = TimeSpan.FromSeconds(30);
            client.DefaultRequestHeaders.Add("User-Agent", "ArNir-WebFetchTool/1.0");
        });

        // Register tool implementations as Singletons
        services.AddSingleton<DocumentLookupTool>();

        services.AddSingleton<WebFetchTool>(sp =>
        {
            var factory = sp.GetRequiredService<IHttpClientFactory>();
            var logger  = sp.GetRequiredService<ILogger<WebFetchTool>>();
            return new WebFetchTool(factory.CreateClient("WebFetch"), logger);
        });

        return services;
    }
}
