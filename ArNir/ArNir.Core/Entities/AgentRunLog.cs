using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ArNir.Core.Entities;

/// <summary>Persisted record of a PlannerAgent execution for admin audit and debugging (Phase 10).</summary>
[Table("AgentRunLogs")]
public class AgentRunLog
{
    /// <summary>Primary key.</summary>
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>Client-supplied or generated session identifier.</summary>
    [Required, MaxLength(64)]
    public string SessionId { get; set; } = string.Empty;

    /// <summary>The original natural-language query that triggered the agent run.</summary>
    [Required]
    public string OriginalQuery { get; set; } = string.Empty;

    /// <summary>Full serialised AgentPlan JSON (steps + results).</summary>
    [Required]
    public string PlanJson { get; set; } = string.Empty;

    /// <summary>Run status: "Pending" | "Running" | "Completed" | "Failed".</summary>
    [MaxLength(20)]
    public string Status { get; set; } = "Pending";

    /// <summary>UTC timestamp when the run was created.</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>UTC timestamp when the run finished (null while still running).</summary>
    public DateTime? CompletedAt { get; set; }
}
