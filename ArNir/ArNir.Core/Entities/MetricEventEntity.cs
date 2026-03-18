using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ArNir.Core.Entities;

/// <summary>Persisted observability metric event for SLA dashboards and trend analysis (Phase 10).</summary>
[Table("MetricEvents")]
public class MetricEventEntity
{
    /// <summary>Auto-incremented surrogate key.</summary>
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    /// <summary>Category of the event (e.g. "Inference", "Embedding", "RAG").</summary>
    [MaxLength(50)]
    public string EventType { get; set; } = string.Empty;

    /// <summary>AI provider name (e.g. "OpenAI", "Anthropic", "Gemini").</summary>
    [MaxLength(50)]
    public string Provider { get; set; } = string.Empty;

    /// <summary>Model identifier (e.g. "gpt-4o-mini", "claude-3-haiku").</summary>
    [MaxLength(100)]
    public string Model { get; set; } = string.Empty;

    /// <summary>End-to-end latency in milliseconds.</summary>
    public long LatencyMs { get; set; }

    /// <summary>True when LatencyMs is below the configured SLA threshold.</summary>
    public bool IsWithinSla { get; set; }

    /// <summary>Total tokens consumed (prompt + completion).</summary>
    public int TokensUsed { get; set; }

    /// <summary>UTC timestamp when the event occurred.</summary>
    public DateTime OccurredAt { get; set; } = DateTime.UtcNow;

    /// <summary>Optional JSON blob of additional tags / metadata.</summary>
    public string? TagsJson { get; set; }
}
