namespace ArNir.PromptEngine.Models;

/// <summary>
/// Identifies which layer of the 3-layer prompt resolution chain produced a <see cref="PromptTemplate"/>.
/// <para>
/// Resolution order (highest to lowest priority):
/// <list type="number">
///   <item><term>Database</term><description>Fetched at runtime from persistent storage via <c>IPromptVersionStore</c>.</description></item>
///   <item><term>Config</term><description>Loaded from <c>appsettings.json</c> or environment variables at startup.</description></item>
///   <item><term>Code</term><description>Hardcoded fallback defined in <c>CodePromptResolver</c> — always available, no infrastructure required.</description></item>
/// </list>
/// </para>
/// </summary>
public enum PromptSource
{
    /// <summary>
    /// The template is hardcoded in <c>CodePromptResolver</c>.
    /// This is <b>Layer 3</b> — the lowest-priority fallback, safe for dev and testing with zero infrastructure.
    /// </summary>
    Code,

    /// <summary>
    /// The template was loaded from application configuration (e.g. <c>appsettings.json</c> or environment variables).
    /// This is <b>Layer 2</b> — overrides code defaults but can be overridden by database values.
    /// </summary>
    Config,

    /// <summary>
    /// The template was retrieved from persistent storage (e.g. a database) via <c>IPromptVersionStore</c>.
    /// This is <b>Layer 1</b> — the highest-priority source; runtime updates take effect without redeployment.
    /// </summary>
    Database
}
