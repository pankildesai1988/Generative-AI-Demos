using ArNir.Agents.Interfaces;
using ArNir.Agents.Models;
using Microsoft.AspNetCore.Mvc;

namespace ArNir.Api.Controllers;

/// <summary>
/// Exposes the ArNir.Agents plan-and-execute pipeline over HTTP.
/// Clients submit a natural-language query; the controller creates a plan and executes it,
/// returning the completed <see cref="AgentPlan"/> with per-step results.
/// </summary>
[ApiController]
[Route("api/agent")]
public sealed class AgentController : ControllerBase
{
    private readonly IPlannerAgent _planner;
    private readonly ILogger<AgentController> _logger;

    /// <summary>Initialises a new <see cref="AgentController"/>.</summary>
    public AgentController(IPlannerAgent planner, ILogger<AgentController> logger)
    {
        _planner = planner;
        _logger = logger;
    }

    /// <summary>
    /// Creates a plan from the provided query and immediately executes all steps.
    /// </summary>
    /// <param name="request">
    /// A <see cref="AgentRunRequest"/> containing <c>SessionId</c> and <c>Query</c>.
    /// </param>
    /// <returns>
    /// The completed <see cref="AgentPlan"/> with all steps, tool results, and final status.
    /// </returns>
    /// <remarks>
    /// <b>Demo story:</b> A consulting client types a natural-language instruction
    /// (e.g. "Find weather docs and summarise them"). The PlannerAgent decomposes
    /// this into ordered steps, dispatches each to the registered tools
    /// (e.g. <c>DocumentLookup</c>, <c>WebFetch</c>), and returns the structured result.
    /// </remarks>
    [HttpPost("run")]
    public async Task<IActionResult> Run([FromBody] AgentRunRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Query))
            return BadRequest(new { error = "Query cannot be empty." });

        var sessionId = string.IsNullOrWhiteSpace(request.SessionId)
            ? Guid.NewGuid().ToString()
            : request.SessionId;

        _logger.LogInformation("Agent run requested — session '{SessionId}', query: '{Query}'", sessionId, request.Query);

        var plan = await _planner.CreatePlanAsync(sessionId, request.Query);
        var result = await _planner.ExecutePlanAsync(plan);

        return Ok(result);
    }
}

/// <summary>Request body for <c>POST /api/agent/run</c>.</summary>
public sealed class AgentRunRequest
{
    /// <summary>
    /// Optional session identifier. A new GUID is generated automatically if not supplied.
    /// </summary>
    public string? SessionId { get; set; }

    /// <summary>The natural-language query or instruction for the agent to process.</summary>
    public string Query { get; set; } = string.Empty;
}
