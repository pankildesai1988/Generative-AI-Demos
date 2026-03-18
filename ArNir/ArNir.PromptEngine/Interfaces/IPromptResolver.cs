using ArNir.PromptEngine.Models;

namespace ArNir.PromptEngine.Interfaces;

/// <summary>
/// Defines the contract for resolving and building prompts using the 3-layer resolution chain.
/// <para>
/// Resolution order (highest → lowest priority):
/// <list type="number">
///   <item><term>Database (Layer 1)</term><description>Checked first via <c>IPromptVersionStore</c>; supports runtime updates.</description></item>
///   <item><term>Config (Layer 2)</term><description>Falls back to configuration-bound templates when no DB entry exists.</description></item>
///   <item><term>Code (Layer 3)</term><description>Final fallback to hardcoded templates in <c>CodePromptResolver</c>.</description></item>
/// </list>
/// </para>
/// <para>
/// <see cref="BuildPromptAsync"/> handles placeholder substitution:
/// <c>{{QUERY}}</c> is replaced with the user query and <c>{{CONTEXT}}</c> with any retrieved context.
/// </para>
/// </summary>
public interface IPromptResolver
{
    /// <summary>
    /// Resolves the best available <see cref="PromptTemplate"/> for the given <paramref name="style"/>,
    /// walking the DB → Config → Code chain until a match is found.
    /// </summary>
    /// <param name="style">
    /// The prompt style key to resolve (e.g. <c>zero-shot</c>, <c>few-shot</c>, <c>role</c>, <c>hybrid</c>, <c>rag</c>).
    /// </param>
    /// <param name="provider">
    /// Optional AI provider hint (e.g. <c>openai</c>, <c>gemini</c>, <c>claude</c>).
    /// Implementations may use this to return provider-optimised variants.
    /// </param>
    /// <param name="ct">A cancellation token.</param>
    /// <returns>
    /// The resolved <see cref="PromptTemplate"/>, or <c>null</c> if no template could be found at any layer.
    /// </returns>
    Task<PromptTemplate?> ResolveAsync(string style, string? provider = null, CancellationToken ct = default);

    /// <summary>
    /// Resolves the template for <paramref name="style"/> and returns the final prompt string
    /// with all placeholders substituted.
    /// <para>
    /// Substitutions applied:
    /// <list type="bullet">
    ///   <item><c>{{QUERY}}</c> → <paramref name="query"/></item>
    ///   <item><c>{{CONTEXT}}</c> → <paramref name="context"/> (omitted if <c>null</c>)</item>
    /// </list>
    /// </para>
    /// </summary>
    /// <param name="style">The prompt style key (e.g. <c>rag</c>, <c>few-shot</c>).</param>
    /// <param name="query">The user query that replaces the <c>{{QUERY}}</c> placeholder.</param>
    /// <param name="context">
    /// Optional retrieved context that replaces the <c>{{CONTEXT}}</c> placeholder.
    /// Pass <c>null</c> when no context is available; the placeholder is removed from the output.
    /// </param>
    /// <param name="provider">Optional provider hint forwarded to <see cref="ResolveAsync"/>.</param>
    /// <param name="ct">A cancellation token.</param>
    /// <returns>The fully resolved, placeholder-free prompt string ready to send to an LLM.</returns>
    Task<string> BuildPromptAsync(string style, string query, string? context = null, string? provider = null, CancellationToken ct = default);
}
