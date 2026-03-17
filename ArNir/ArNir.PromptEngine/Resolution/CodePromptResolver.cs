using ArNir.PromptEngine.Interfaces;
using ArNir.PromptEngine.Models;
using Microsoft.Extensions.Logging;

namespace ArNir.PromptEngine.Resolution;

/// <summary>
/// A hardcoded, infrastructure-free implementation of <see cref="IPromptResolver"/>.
/// <para>
/// This class represents <b>Layer 3 (Code)</b> — the lowest-priority fallback in the
/// 3-layer prompt resolution chain (Database → Config → Code).
/// It is always available with zero external dependencies, making it safe for
/// local development, unit testing, and CI environments.
/// </para>
/// <para>
/// Supported styles and their template strategies:
/// <list type="table">
///   <listheader><term>Style</term><description>Strategy</description></listheader>
///   <item><term>zero-shot</term><description>Direct question with no examples or extra context.</description></item>
///   <item><term>few-shot</term><description>Examples-first format; <c>{{CONTEXT}}</c> holds the examples, <c>{{QUERY}}</c> the question.</description></item>
///   <item><term>role</term><description>System-persona preamble followed by the user query.</description></item>
///   <item><term>hybrid</term><description>Combines role preamble, examples (<c>{{CONTEXT}}</c>), and the user query.</description></item>
///   <item><term>rag</term><description>Grounded retrieval format; retrieved documents go in <c>{{CONTEXT}}</c>. Default fallback for unknown styles.</description></item>
/// </list>
/// </para>
/// </summary>
public sealed class CodePromptResolver : IPromptResolver
{
    private readonly ILogger<CodePromptResolver> _logger;

    /// <summary>
    /// Read-only dictionary of hardcoded <see cref="PromptTemplate"/> objects keyed by style (lower-case).
    /// </summary>
    private static readonly IReadOnlyDictionary<string, PromptTemplate> _templates =
        new Dictionary<string, PromptTemplate>(StringComparer.OrdinalIgnoreCase)
        {
            ["zero-shot"] = new PromptTemplate
            {
                Name        = "Zero-Shot Default",
                Style       = "zero-shot",
                Source      = PromptSource.Code,
                TemplateText =
                    "Answer the following question as accurately and concisely as possible.\n\n" +
                    "Question: {{QUERY}}"
            },

            ["few-shot"] = new PromptTemplate
            {
                Name        = "Few-Shot Default",
                Style       = "few-shot",
                Source      = PromptSource.Code,
                TemplateText =
                    "Below are some examples that illustrate the expected answer format.\n\n" +
                    "Examples:\n{{CONTEXT}}\n\n" +
                    "Now answer the following question using the same format.\n\n" +
                    "Question: {{QUERY}}"
            },

            ["role"] = new PromptTemplate
            {
                Name        = "Role Default",
                Style       = "role",
                Source      = PromptSource.Code,
                TemplateText =
                    "You are a knowledgeable and helpful AI assistant. " +
                    "Respond clearly, professionally, and with appropriate depth.\n\n" +
                    "User: {{QUERY}}"
            },

            ["hybrid"] = new PromptTemplate
            {
                Name        = "Hybrid Default",
                Style       = "hybrid",
                Source      = PromptSource.Code,
                TemplateText =
                    "You are a knowledgeable and helpful AI assistant. " +
                    "Use the provided examples and context to formulate your answer.\n\n" +
                    "Context / Examples:\n{{CONTEXT}}\n\n" +
                    "User: {{QUERY}}"
            },

            ["rag"] = new PromptTemplate
            {
                Name        = "RAG Default",
                Style       = "rag",
                Source      = PromptSource.Code,
                TemplateText =
                    "You are a helpful assistant. Use ONLY the information in the provided context " +
                    "to answer the question. If the context does not contain the answer, say so.\n\n" +
                    "Context:\n{{CONTEXT}}\n\n" +
                    "Question: {{QUERY}}"
            }
        };

    /// <summary>
    /// Initialises a new instance of <see cref="CodePromptResolver"/>.
    /// </summary>
    /// <param name="logger">Logger used to trace resolution and fallback events.</param>
    public CodePromptResolver(ILogger<CodePromptResolver> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc />
    /// <remarks>
    /// Performs a case-insensitive lookup in the hardcoded template dictionary.
    /// If <paramref name="style"/> is not found, falls back to the <c>rag</c> template and
    /// logs a warning. The <paramref name="provider"/> parameter is accepted for interface
    /// compatibility but is not used by this implementation.
    /// </remarks>
    public Task<PromptTemplate?> ResolveAsync(string style, string? provider = null, CancellationToken ct = default)
    {
        if (_templates.TryGetValue(style, out var template))
        {
            _logger.LogDebug("CodePromptResolver: resolved style '{Style}' (source=Code).", style);
            return Task.FromResult<PromptTemplate?>(template);
        }

        _logger.LogWarning(
            "CodePromptResolver: unknown style '{Style}' — falling back to 'rag'.", style);

        return Task.FromResult<PromptTemplate?>(_templates["rag"]);
    }

    /// <inheritdoc />
    /// <remarks>
    /// Calls <see cref="ResolveAsync"/> to obtain the template, then performs string substitution:
    /// <list type="bullet">
    ///   <item><c>{{QUERY}}</c> is replaced with <paramref name="query"/>.</item>
    ///   <item><c>{{CONTEXT}}</c> is replaced with <paramref name="context"/> when provided,
    ///   or removed (replaced with an empty string) when <c>null</c>.</item>
    /// </list>
    /// </remarks>
    public async Task<string> BuildPromptAsync(
        string style,
        string query,
        string? context     = null,
        string? provider    = null,
        CancellationToken ct = default)
    {
        var template = await ResolveAsync(style, provider, ct).ConfigureAwait(false);

        var text = template!.TemplateText
            .Replace("{{QUERY}}", query, StringComparison.Ordinal)
            .Replace("{{CONTEXT}}", context ?? string.Empty, StringComparison.Ordinal);

        _logger.LogDebug(
            "CodePromptResolver: built prompt for style '{Style}', length={Length}.", style, text.Length);

        return text;
    }
}
