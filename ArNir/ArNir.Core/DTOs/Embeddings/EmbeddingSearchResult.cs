using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArNir.Core.DTOs.Embeddings
{
    public class EmbeddingSearchResult
    {
        public int ChunkId { get; set; }
        public double Score { get; set; }
    }
}
