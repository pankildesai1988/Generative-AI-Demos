namespace ArNir.Admin.Models;

/// <summary>View model for the Embedding management stats panel.</summary>
public class EmbeddingStatsViewModel
{
    /// <summary>Total number of embedding vectors stored.</summary>
    public long TotalEmbeddings { get; set; }

    /// <summary>Per-model breakdown of embedding counts.</summary>
    public List<ModelStatEntry> ByModel { get; set; } = new();

    /// <summary>UTC timestamp of the oldest embedding.</summary>
    public DateTime? OldestCreatedAt { get; set; }

    /// <summary>UTC timestamp of the newest embedding.</summary>
    public DateTime? NewestCreatedAt { get; set; }

    /// <summary>Total number of documents in the SQL store.</summary>
    public int TotalDocuments { get; set; }
}

/// <summary>A single row in the per-model breakdown table.</summary>
public class ModelStatEntry
{
    /// <summary>Embedding model name (e.g. "text-embedding-ada-002").</summary>
    public string Model { get; set; } = "";

    /// <summary>Number of embedding vectors for this model.</summary>
    public long Count { get; set; }
}
