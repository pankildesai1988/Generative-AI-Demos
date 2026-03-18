namespace ArNir.Agents.Interfaces;

/// <summary>
/// Defines the contract for a runtime catalogue of available <see cref="IAgentTool"/> instances.
/// <para>
/// The registry is the single source of truth for tools that the planner may select during
/// <c>IPlannerAgent.CreatePlanAsync</c> and that the executor dispatches during
/// <c>IPlannerAgent.ExecutePlanAsync</c>.
/// </para>
/// <para>
/// Implementations must be thread-safe; the registry is registered as Singleton in the DI container.
/// </para>
/// </summary>
public interface IToolRegistry
{
    /// <summary>
    /// Adds or replaces a tool in the registry.
    /// If a tool with the same <see cref="IAgentTool.Name"/> already exists it is overwritten.
    /// </summary>
    /// <param name="tool">The tool to register. Must not be <c>null</c>.</param>
    void Register(IAgentTool tool);

    /// <summary>
    /// Retrieves a registered tool by its <see cref="IAgentTool.Name"/> (case-insensitive).
    /// </summary>
    /// <param name="name">The tool name to look up.</param>
    /// <returns>
    /// The matching <see cref="IAgentTool"/>, or <c>null</c> when no tool with that name is registered.
    /// </returns>
    IAgentTool? Get(string name);

    /// <summary>
    /// Returns a read-only snapshot of all currently registered tools.
    /// </summary>
    /// <returns>An immutable list of every <see cref="IAgentTool"/> in the registry.</returns>
    IReadOnlyList<IAgentTool> GetAll();
}
