namespace ArNir.PromptEngine.Models;

/// <summary>
/// Represents a versioned, style-specific prompt template used by the 3-layer resolution chain.
/// <para>
/// Resolution priority (highest → lowest):
/// <list type="number">
///   <item><term>Database</term><description>Retrieved at runtime via <c>IPromptVersionStore</c>; supports hot-update without redeployment.</description></item>
///   <item><term>Config</term><description>Loaded from <c>appsettings.json</c> or environment variables at startup.</description></item>
///   <item><term>Code</term><description>Hardcoded fallback in <c>CodePromptResolver</c>; always available, zero infrastructure required.</description></item>
/// </list>
/// </para>
/// <para>
/// <see cref="TemplateText"/> may contain the placeholders <c>{{QUERY}}</c> and <c>{{CONTEXT}}</c>,
/// which are substituted at build time by <c>IPromptResolver.BuildPromptAsync</c>.
/// </para>
/// </summary>
public sealed class PromptTemplate
{
    /// <summary>Gets or sets the unique identifier for this prompt template.</summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Gets or sets a human-readable name for this template (e.g. <c>"RAG Default"</c>, <c>"Few-Shot v2"</c>).
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the prompt style key this template satisfies.
    /// <para>
    /// Accepted values: <c>zero-shot</c>, <c>few-shot</c>, <c>role</c>, <c>hybrid</c>, <c>rag</c>.
    /// </para>
    /// </summary>
    public string Style { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the raw template text.
    /// Use <c>{{QUERY}}</c> and <c>{{CONTEXT}}</c> as substitution placeholders.
    /// </summary>
    public string TemplateText { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the monotonically increasing version number.
    /// Higher values indicate more recent templates; the version store uses this to maintain history.
    /// </summary>
    public int Version { get; set; } = 1;

    /// <summary>
    /// Gets or sets a value indicating whether this template is currently active.
    /// Resolvers should skip inactive templates during lookup.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Gets or sets the resolution layer that produced this template.
    /// Defaults to <see cref="PromptSource.Code"/> for hardcoded fallbacks.
    /// </summary>
    public PromptSource Source { get; set; } = PromptSource.Code;

    /// <summary>Gets or sets the UTC timestamp at which this template was created or last persisted.</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
