using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ArNir.Core.Entities
{
    [Table("ChatMemories")]
    public class ChatMemory
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required, MaxLength(64)]
        public string SessionId { get; set; } = string.Empty;

        [Required]
        public string UserMessage { get; set; } = string.Empty;

        public string? AssistantMessage { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // ✅ Link to PostgreSQL ChatEmbedding
        public Guid? EmbeddingRefId { get; set; }
    }
}
