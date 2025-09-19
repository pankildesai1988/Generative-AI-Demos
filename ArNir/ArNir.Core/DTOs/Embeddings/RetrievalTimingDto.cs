using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArNir.Core.DTOs.Embeddings
{
    public class RetrievalTimingDto
    {
        public long EmbeddingMs { get; set; }
        public long SemanticMs { get; set; }
        public long ChunkFetchMs { get; set; }
        public long KeywordMs { get; set; }
        public long TotalMs { get; set; }
    }
}
