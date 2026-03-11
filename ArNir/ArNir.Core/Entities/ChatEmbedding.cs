using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Pgvector;

namespace ArNir.Core.Entities
{
    [Table("ChatEmbeddings")]
    public class ChatEmbedding
    {
        [Key]
        public Guid EmbeddingId { get; set; } = Guid.NewGuid();

        public int ChatMemoryId { get; set; }

        public string Model { get; set; } = "text-embedding-3-small";

        [Column(TypeName = "vector(1536)")]
        public Vector Vector { get; set; } = null!;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
