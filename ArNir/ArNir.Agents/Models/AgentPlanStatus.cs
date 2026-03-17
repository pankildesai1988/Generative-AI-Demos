namespace ArNir.Agents.Models;

/// <summary>
/// Represents the lifecycle state of an <see cref="AgentPlan"/> as it moves through
/// the plan-and-execute pipeline.
/// </summary>
public enum AgentPlanStatus
{
    /// <summary>
    /// The plan has been created but execution has not yet started.
    /// All steps are queued and awaiting a call to <c>IPlannerAgent.ExecutePlanAsync</c>.
    /// </summary>
    Pending,

    /// <summary>
    /// The plan is actively executing. Steps are being dispatched to registered tools
    /// in <c>Order</c> sequence.
    /// </summary>
    Running,

    /// <summary>
    /// All steps completed successfully. Every <see cref="AgentStep.IsCompleted"/> flag is <c>true</c>
    /// and each step carries a non-null <see cref="AgentStep.Result"/>.
    /// </summary>
    Completed,

    /// <summary>
    /// Execution was halted because a step threw an unhandled exception.
    /// The faulted step's <see cref="AgentStep.Result"/> contains the exception message;
    /// subsequent steps were not executed.
    /// </summary>
    Failed
}
