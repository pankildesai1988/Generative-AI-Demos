namespace ArNir.Admin.Models;

public class PlatformHealthViewModel
{
    public int TotalDocuments { get; set; }
    public int TotalChunks { get; set; }
    public long TotalEmbeddings { get; set; }
    public bool PostgresConnected { get; set; }
    public bool OpenAiKeyConfigured { get; set; }
    public bool EmbedderIsReal { get; set; }
    public int MetricEventsLast24h { get; set; }
    public double AvgLatencyLast24hMs { get; set; }
    public double SlaComplianceLast24hPct { get; set; }
    public List<RecentRunSummary> RecentAgentRuns { get; set; } = new();
}

public class RecentRunSummary
{
    public Guid Id { get; set; }
    public string Query { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime StartedAt { get; set; }
}
