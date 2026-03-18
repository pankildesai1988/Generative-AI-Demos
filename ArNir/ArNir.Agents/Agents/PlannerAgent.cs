using ArNir.Agents.Interfaces;
using ArNir.Agents.Models;
using ArNir.Memory.Interfaces;
using ArNir.Memory.Models;
using ArNir.PromptEngine.Interfaces;
using Microsoft.Extensions.Logging;

namespace ArNir.Agents.Agents;

/// <summary>
/// A plan-and-execute orchestrator that implements <see cref="IPlannerAgent"/>.
/// <para>
/// <b>Architectural note (locked decision):</b> <see cref="ISemanticMemory"/> is injected directly
/// into this class to provide cross-session context recall during plan creation. This is intentional
/// and must not be changed — semantic recall is a first-class concern of the planning step, not an
/// optional enhancement.
/// </para>
/// <para>
/// Dependency summary:
/// <list type="bullet">
///   <item><see cref="IToolRegistry"/> — discovers available tools at plan-creation time and dispatches steps at execution time.</item>
///   <item><see cref="ISemanticMemory"/> — recalled cross-session context enriches the planner's tool-selection heuristic.</item>
///   <item><see cref="IEpisodicMemory"/> — each completed step is appended as a session memory entry for within-session history.</item>
///   <item><see cref="IPromptResolver"/> — reserved for future LLM-backed planning; injected now so the interface is stable.</item>
/// </list>
/// </para>
/// <para>
/// <b>Phase 6 planning heuristic:</b> tool selection uses a keyword <c>Contains</c> match between
/// the query (lower-cased) and <see cref="IAgentTool.Description"/> (lower-cased). Full LLM-backed
/// planning is deferred to a later phase.
/// </para>
/// </summary>
public sealed class PlannerAgent : IPlannerAgent
{
    private readonly IToolRegistry _toolRegistry;
    private readonly ISemanticMemory _semanticMemory;
    private readonly IEpisodicMemory _episodicMemory;
    private readonly IPromptResolver _promptResolver;
    private readonly ILogger<PlannerAgent> _logger;

    /// <summary>
    /// Initialises a new instance of <see cref="PlannerAgent"/>.
    /// </summary>
    /// <param name="toolRegistry">The registry supplying available tools.</param>
    /// <param name="semanticMemory">
    /// Long-term, vector-based memory used for cross-session context recall.
    /// This dependency is a <b>locked architectural decision</b>.
    /// </param>
    /// <param name="episodicMemory">Short-term, session-scoped conversational history.</param>
    /// <param name="promptResolver">
    /// 3-layer prompt resolver; reserved for future LLM-backed planning per step.
    /// </param>
    /// <param name="logger">Logger for diagnostic and audit output.</param>
    public PlannerAgent(
        IToolRegistry toolRegistry,
        ISemanticMemory semanticMemory,
        IEpisodicMemory episodicMemory,
        IPromptResolver promptResolver,
        ILogger<PlannerAgent> logger)
    {
        _toolRegistry   = toolRegistry;
        _semanticMemory = semanticMemory;
        _episodicMemory = episodicMemory;
        _promptResolver = promptResolver;
        _logger         = logger;
    }

    /// <inheritdoc />
    /// <remarks>
    /// <b>Planning algorithm (Phase 6 — keyword heuristic):</b>
    /// <list type="number">
    ///   <item>Recall up to 5 cross-session entries from <see cref="ISemanticMemory"/> using the raw query text.</item>
    ///   <item>For each registered tool, check whether any word in the query appears in the tool's <see cref="IAgentTool.Description"/> (case-insensitive). Matching tools become plan steps ordered by their position in the registry snapshot.</item>
    ///   <item>If no tools match, a single <c>no-op</c> fallback step is created.</item>
    /// </list>
    /// Full LLM-backed decomposition is deferred to a later phase and will use
    /// <c>IPromptResolver.BuildPromptAsync</c> to format the planning prompt.
    /// </remarks>
    public async Task<AgentPlan> CreatePlanAsync(
        string sessionId,
        string query,
        CancellationToken ct = default)
    {
        _logger.LogInformation(
            "PlannerAgent.CreatePlanAsync: session={SessionId}, query='{Query}'.", sessionId, query);

        // Layer 1 — cross-session context recall via semantic memory (locked architectural decision)
        var recalled = await _semanticMemory.RecallByTextAsync(query, topK: 5, ct).ConfigureAwait(false);

        _logger.LogDebug(
            "PlannerAgent: recalled {Count} cross-session entries for planning context.", recalled.Count);

        // Layer 2 — keyword heuristic tool selection
        var allTools  = _toolRegistry.GetAll();
        var queryLower = query.ToLowerInvariant();

        var matchedSteps = allTools
            .Where(t => t.Description.ToLowerInvariant()
                .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Any(word => queryLower.Contains(word, StringComparison.OrdinalIgnoreCase)))
            .Select((t, index) => new AgentStep
            {
                Order      = index,
                ToolName   = t.Name,
                Parameters = new Dictionary<string, string>
                {
                    ["query"]   = query,
                    ["context"] = string.Join("\n", recalled.Select(r => r.Content))
                }
            })
            .ToList();

        // Fallback — no tools matched
        if (matchedSteps.Count == 0)
        {
            _logger.LogWarning(
                "PlannerAgent: no tools matched query '{Query}'. Inserting no-op fallback step.", query);

            matchedSteps.Add(new AgentStep
            {
                Order    = 0,
                ToolName = "no-op",
                Parameters = new Dictionary<string, string> { ["query"] = query }
            });
        }

        var plan = new AgentPlan
        {
            SessionId     = sessionId,
            OriginalQuery = query,
            Steps         = matchedSteps,
            Status        = AgentPlanStatus.Pending
        };

        _logger.LogInformation(
            "PlannerAgent: created plan {PlanId} with {StepCount} step(s).", plan.Id, plan.Steps.Count);

        return plan;
    }

    /// <inheritdoc />
    /// <remarks>
    /// Steps are executed strictly in ascending <see cref="AgentStep.Order"/> sequence.
    /// After each successful step a <see cref="MemoryEntry"/> is appended to
    /// <see cref="IEpisodicMemory"/> with <c>Role = "agent"</c>.
    /// On the first exception the plan is marked <see cref="AgentPlanStatus.Failed"/> and
    /// no further steps are run.
    /// </remarks>
    public async Task<AgentPlan> ExecutePlanAsync(
        AgentPlan plan,
        CancellationToken ct = default)
    {
        _logger.LogInformation(
            "PlannerAgent.ExecutePlanAsync: plan={PlanId}, steps={StepCount}.",
            plan.Id, plan.Steps.Count);

        plan.Status = AgentPlanStatus.Running;

        foreach (var step in plan.Steps.OrderBy(s => s.Order))
        {
            ct.ThrowIfCancellationRequested();

            var tool = _toolRegistry.Get(step.ToolName);

            if (tool is null)
            {
                _logger.LogWarning(
                    "PlannerAgent: step {Order} references unknown tool '{ToolName}' — skipping.",
                    step.Order, step.ToolName);

                step.Result      = $"Tool '{step.ToolName}' is not registered.";
                step.IsCompleted = false;
                continue;
            }

            try
            {
                _logger.LogDebug(
                    "PlannerAgent: executing step {Order} via tool '{ToolName}'.",
                    step.Order, step.ToolName);

                step.Result      = await tool.ExecuteAsync(step.Parameters, ct).ConfigureAwait(false);
                step.IsCompleted = true;

                // Record the step outcome in episodic memory for within-session history
                await _episodicMemory.AddAsync(new MemoryEntry
                {
                    SessionId = plan.SessionId,
                    Role      = "agent",
                    Content   = $"Step {step.Order}: {step.ToolName} → {step.Result}"
                }, ct).ConfigureAwait(false);

                _logger.LogDebug(
                    "PlannerAgent: step {Order} completed successfully.", step.Order);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "PlannerAgent: step {Order} (tool='{ToolName}') failed — aborting plan {PlanId}.",
                    step.Order, step.ToolName, plan.Id);

                step.Result      = ex.Message;
                step.IsCompleted = false;
                plan.Status      = AgentPlanStatus.Failed;

                return plan;
            }
        }

        plan.Status = AgentPlanStatus.Completed;

        _logger.LogInformation(
            "PlannerAgent: plan {PlanId} completed successfully.", plan.Id);

        return plan;
    }
}
