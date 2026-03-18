namespace ArNir.Agents.Models;

/// <summary>
/// Represents a structured execution plan produced by <c>IPlannerAgent.CreatePlanAsync</c>
/// and consumed by <c>IPlannerAgent.ExecutePlanAsync</c>.
/// <para>
/// Lifecycle:
/// <list type="number">
///   <item><term>Create</term><description>The planner decomposes a user query into an ordered list of <see cref="AgentStep"/> objects. Status is <see cref="AgentPlanStatus.Pending"/>.</description></item>
///   <item><term>Execute</term><description>Steps are dispatched in <see cref="AgentStep.Order"/> sequence to registered tools. Status transitions to <see cref="AgentPlanStatus.Running"/>.</description></item>
///   <item><term>Complete / Fail</term><description>Status becomes <see cref="AgentPlanStatus.Completed"/> when all steps succeed, or <see cref="AgentPlanStatus.Failed"/> when a step throws.</description></item>
/// </list>
/// </para>
/// </summary>
public sealed class AgentPlan
{
    /// <summary>Gets or sets the unique identifier for this plan.</summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Gets or sets the session identifier that associates this plan with a conversational context.
    /// Used to store step results in <c>IEpisodicMemory</c> and to look up cross-session context
    /// from <c>ISemanticMemory</c>.
    /// </summary>
    public string SessionId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the original user query that triggered plan creation.
    /// Preserved verbatim for traceability and replay.
    /// </summary>
    public string OriginalQuery { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the ordered list of steps that make up this plan.
    /// Steps are executed in ascending <see cref="AgentStep.Order"/> sequence.
    /// </summary>
    public List<AgentStep> Steps { get; set; } = new();

    /// <summary>
    /// Gets or sets the current lifecycle status of this plan.
    /// Starts at <see cref="AgentPlanStatus.Pending"/> and transitions through
    /// <see cref="AgentPlanStatus.Running"/> to either
    /// <see cref="AgentPlanStatus.Completed"/> or <see cref="AgentPlanStatus.Failed"/>.
    /// </summary>
    public AgentPlanStatus Status { get; set; } = AgentPlanStatus.Pending;

    /// <summary>Gets or sets the UTC timestamp at which this plan was created.</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
