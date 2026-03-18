namespace ArNir.Admin.Models;

public class VectorStoreViewModel
{
    public long TotalEmbeddings { get; set; }
    public List<ModelEmbeddingCount> EmbeddingsByModel { get; set; } = new();
    public DateTime? LastIndexedAt { get; set; }
    public List<OrphanedDocument> OrphanedDocuments { get; set; } = new();
}

public class ModelEmbeddingCount
{
    public string Model { get; set; } = "";
    public long Count { get; set; }
}

public class OrphanedDocument
{
    public int DocumentId { get; set; }
    public string DocumentName { get; set; } = "";
    public int MissingChunks { get; set; }
}
