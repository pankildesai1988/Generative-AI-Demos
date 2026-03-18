namespace ArNir.Admin.Models;

/// <summary>View model for the Prompt A/B Statistics panel.</summary>
public class PromptStatsViewModel
{
    /// <summary>Per-style performance statistics rows.</summary>
    public List<PromptStyleStats> Stats { get; set; } = new();
}

/// <summary>Aggregated statistics for a single prompt style.</summary>
public class PromptStyleStats
{
    /// <summary>Prompt style key (e.g. "rag", "zero-shot", "few-shot").</summary>
    public string Style { get; set; } = "";

    /// <summary>Total number of RAG comparison runs for this style.</summary>
    public int TotalRuns { get; set; }

    /// <summary>Average total latency in milliseconds across all runs.</summary>
    public double AvgLatencyMs { get; set; }

    /// <summary>Percentage of runs that were within SLA (IsWithinSla == true).</summary>
    public double SlaCompliancePct { get; set; }

    /// <summary>Average feedback rating for this style (0.0 if no feedback).</summary>
    public double AvgRating { get; set; }

    /// <summary>UTC timestamp of the most recent run for this style.</summary>
    public DateTime? LastUsed { get; set; }
}
