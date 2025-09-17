using Pgvector;

namespace ArNir.Core.Entities
{
    public class Embedding
    {
        public Guid EmbeddingId { get; set; }
        public int ChunkId { get; set; }
        public string Model { get; set; } = string.Empty;
        // ✅ Now use Pgvector.Vector type
        public Vector Vector { get; set; } = null!;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
