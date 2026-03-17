namespace ArNir.Agents.Models;

/// <summary>
/// Represents a single executable step within an <see cref="AgentPlan"/>.
/// Each step is dispatched to a named <c>IAgentTool</c> via the <c>IToolRegistry</c>.
/// Steps are executed in ascending <see cref="Order"/> sequence.
/// </summary>
public sealed class AgentStep
{
    /// <summary>
    /// Gets or sets the zero-based execution order of this step within its parent <see cref="AgentPlan"/>.
    /// Steps with lower values are executed first.
    /// </summary>
    public int Order { get; set; } = 0;

    /// <summary>
    /// Gets or sets the unique name of the <c>IAgentTool</c> that should handle this step.
    /// Must match <c>IAgentTool.Name</c> exactly (case-insensitive lookup via <c>IToolRegistry.Get</c>).
    /// Use <c>"no-op"</c> as a sentinel value when no matching tool was found during planning.
    /// </summary>
    public string ToolName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the key/value parameters forwarded to the tool's
    /// <c>IAgentTool.ExecuteAsync</c> call.
    /// Parameter semantics are tool-specific and defined by each <c>IAgentTool</c> implementation.
    /// </summary>
    public Dictionary<string, string> Parameters { get; set; } = new();

    /// <summary>
    /// Gets or sets the string result produced by the tool after execution.
    /// <para>
    /// <c>null</c> when the step has not yet been executed.
    /// Contains the exception message if the step faulted.
    /// </para>
    /// </summary>
    public string? Result { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this step has finished executing successfully.
    /// Remains <c>false</c> if the step faulted or has not yet run.
    /// </summary>
    public bool IsCompleted { get; set; } = false;
}
