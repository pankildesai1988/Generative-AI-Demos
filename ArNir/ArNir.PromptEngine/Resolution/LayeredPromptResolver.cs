using ArNir.PromptEngine.Interfaces;
using ArNir.PromptEngine.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ArNir.PromptEngine.Resolution;

/// <summary>
/// Full 3-layer implementation of <see cref="IPromptResolver"/>:
/// <list type="number">
///   <item><term>Layer 1 — Database</term><description>Queries <see cref="IPromptVersionStore"/> for the latest active template.</description></item>
///   <item><term>Layer 2 — Config</term><description>Falls back to <c>appsettings.json</c> key <c>Prompts:{style}</c> when no DB record exists.</description></item>
///   <item><term>Layer 3 — Code</term><description>Delegates to <see cref="CodePromptResolver"/> as the final, always-available fallback.</description></item>
/// </list>
/// <para>
/// Register this class as <see cref="IPromptResolver"/> in the DI container of any host that
/// provides a concrete <see cref="IPromptVersionStore"/> (e.g. <c>DbPromptVersionStore</c>).
/// Hosts without a DB layer should keep the default <see cref="CodePromptResolver"/> registration.
/// </para>
/// </summary>
public sealed class LayeredPromptResolver : IPromptResolver
{
    private readonly IPromptVersionStore            _store;
    private readonly IConfiguration                 _config;
    private readonly CodePromptResolver             _code;
    private readonly ILogger<LayeredPromptResolver> _logger;

    /// <summary>
    /// Initialises the resolver with all three resolution layers.
    /// </summary>
    public LayeredPromptResolver(
        IPromptVersionStore store,
        IConfiguration config,
        CodePromptResolver code,
        ILogger<LayeredPromptResolver> logger)
    {
        _store  = store;
        _config = config;
        _code   = code;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<PromptTemplate?> ResolveAsync(string style, string? provider = null, CancellationToken ct = default)
    {
        // Layer 1 — Database
        var dbTemplate = await _store.GetByStyleAsync(style, ct);
        if (dbTemplate != null)
        {
            _logger.LogDebug("LayeredPromptResolver: resolved style '{Style}' from DB (v{Version}).", style, dbTemplate.Version);
            return dbTemplate;
        }

        // Layer 2 — Config  (key: Prompts:{style})
        var configText = _config[$"Prompts:{style}"];
        if (!string.IsNullOrWhiteSpace(configText))
        {
            _logger.LogDebug("LayeredPromptResolver: resolved style '{Style}' from Config.", style);
            return new PromptTemplate
            {
                Name         = $"{style} (config)",
                Style        = style,
                TemplateText = configText,
                Source       = PromptSource.Config,
                IsActive     = true
            };
        }

        // Layer 3 — Code
        _logger.LogDebug("LayeredPromptResolver: falling back to Code resolver for style '{Style}'.", style);
        return await _code.ResolveAsync(style, provider, ct);
    }

    /// <inheritdoc />
    public async Task<string> BuildPromptAsync(
        string style,
        string query,
        string? context  = null,
        string? provider = null,
        CancellationToken ct = default)
    {
        var template = await ResolveAsync(style, provider, ct);

        // If no template resolved at all, delegate entirely to the code resolver
        if (template == null)
            return await _code.BuildPromptAsync(style, query, context, provider, ct);

        var prompt = template.TemplateText
            .Replace("{{QUERY}}",   query,           StringComparison.OrdinalIgnoreCase)
            .Replace("{{CONTEXT}}", context ?? string.Empty, StringComparison.OrdinalIgnoreCase);

        return prompt;
    }
}
