using ArNir.Agents.Models;

namespace ArNir.Agents.Interfaces;

/// <summary>
/// Defines the contract for a plan-and-execute agent that decomposes a natural-language query
/// into an ordered sequence of tool invocations and then executes them.
/// <para>
/// Workflow:
/// <list type="number">
///   <item>
///     <term><see cref="CreatePlanAsync"/></term>
///     <description>
///       Consults <c>ISemanticMemory</c> for cross-session context, then matches the query
///       against available tools in <c>IToolRegistry</c> to produce an <see cref="AgentPlan"/>
///       with status <see cref="AgentPlanStatus.Pending"/>.
///     </description>
///   </item>
///   <item>
///     <term><see cref="ExecutePlanAsync"/></term>
///     <description>
///       Iterates the plan's steps in <see cref="AgentStep.Order"/> sequence, dispatching each
///       to the named tool. Results are written back into the steps and recorded in
///       <c>IEpisodicMemory</c> for within-session history.
///     </description>
///   </item>
/// </list>
/// </para>
/// </summary>
public interface IPlannerAgent
{
    /// <summary>
    /// Decomposes <paramref name="query"/> into an ordered <see cref="AgentPlan"/> by consulting
    /// cross-session semantic memory and matching against registered tools.
    /// </summary>
    /// <param name="sessionId">
    /// The session identifier used to scope episodic memory writes and to provide context
    /// when recalling from semantic memory.
    /// </param>
    /// <param name="query">The natural-language user query to plan for.</param>
    /// <param name="ct">A cancellation token.</param>
    /// <returns>
    /// A new <see cref="AgentPlan"/> with <see cref="AgentPlanStatus.Pending"/> status and
    /// one <see cref="AgentStep"/> per matched tool. Returns a single <c>no-op</c> step
    /// when no tools match.
    /// </returns>
    Task<AgentPlan> CreatePlanAsync(string sessionId, string query, CancellationToken ct = default);

    /// <summary>
    /// Executes all steps in the <paramref name="plan"/> in ascending <see cref="AgentStep.Order"/>
    /// sequence, writing results back into the plan and into episodic memory after each step.
    /// </summary>
    /// <param name="plan">
    /// The plan to execute. Must have been produced by <see cref="CreatePlanAsync"/>.
    /// The plan is mutated in place: <see cref="AgentPlan.Status"/> and each
    /// <see cref="AgentStep.Result"/> / <see cref="AgentStep.IsCompleted"/> are updated.
    /// </param>
    /// <param name="ct">A cancellation token.</param>
    /// <returns>
    /// The mutated <see cref="AgentPlan"/> with <see cref="AgentPlanStatus.Completed"/>
    /// on full success, or <see cref="AgentPlanStatus.Failed"/> if any step threw.
    /// </returns>
    Task<AgentPlan> ExecutePlanAsync(AgentPlan plan, CancellationToken ct = default);
}
