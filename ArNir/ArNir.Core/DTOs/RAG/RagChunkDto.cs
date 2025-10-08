using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArNir.Core.DTOs.RAG
{
    public class RagChunkDto
    {
        public int DocumentId { get; set; }
        public string DocumentTitle { get; set; }  // extracted from Metadata["DocumentName"]
        public string ChunkText { get; set; }
        public int Rank { get; set; }
        public string RetrievalType { get; set; } // Semantic | Keyword | Hybrid
    }

}
