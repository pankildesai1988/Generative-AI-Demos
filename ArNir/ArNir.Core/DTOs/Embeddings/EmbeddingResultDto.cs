using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArNir.Core.DTOs.Embeddings
{
    public class EmbeddingResultDto
    {
        public Guid EmbeddingId { get; set; }
        public int ChunkId { get; set; }
        public string Model { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
