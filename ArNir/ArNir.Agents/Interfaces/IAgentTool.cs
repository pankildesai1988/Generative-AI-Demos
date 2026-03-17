namespace ArNir.Agents.Interfaces;

/// <summary>
/// Defines the contract for a discrete, executable capability that an agent can invoke
/// as part of a plan step.
/// <para>
/// Each tool is uniquely identified by its <see cref="Name"/> and registered in the
/// <c>IToolRegistry</c>. The planner selects tools by matching <see cref="Description"/>
/// keywords against the user query during <c>IPlannerAgent.CreatePlanAsync</c>.
/// </para>
/// <para>
/// Implementations should be stateless and thread-safe so they can be safely shared
/// as Singleton-lifetime registrations.
/// </para>
/// </summary>
public interface IAgentTool
{
    /// <summary>
    /// Gets the unique name used to identify and look up this tool in the <c>IToolRegistry</c>.
    /// <para>
    /// Must be non-null, non-empty, and unique across all registered tools.
    /// The registry performs case-insensitive matching on this value.
    /// </para>
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets a human-readable description of what this tool does.
    /// <para>
    /// The planner uses this text to determine whether the tool is relevant to a given query.
    /// Descriptions should be concise (1–2 sentences) and include domain-specific keywords
    /// that the planner's heuristic can match.
    /// </para>
    /// </summary>
    string Description { get; }

    /// <summary>
    /// Executes the tool's action using the provided <paramref name="parameters"/>.
    /// </summary>
    /// <param name="parameters">
    /// A key/value map of input parameters. Parameter names and expected values are
    /// tool-specific and documented by each implementation.
    /// </param>
    /// <param name="ct">A cancellation token.</param>
    /// <returns>
    /// A string result representing the tool's output. May be a plain-text summary,
    /// JSON-serialised data, or an error description if the tool cannot complete.
    /// </returns>
    Task<string> ExecuteAsync(Dictionary<string, string> parameters, CancellationToken ct = default);
}
