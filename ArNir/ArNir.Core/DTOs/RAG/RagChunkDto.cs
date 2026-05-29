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
        public string DocumentTitle { get; set; }
        public string ChunkText { get; set; }
        public int Rank { get; set; }
        public string RetrievalType { get; set; }
        public int? PageNumber { get; set; }
        public float? BboxX1 { get; set; }
        public float? BboxY1 { get; set; }
        public float? BboxX2 { get; set; }
        public float? BboxY2 { get; set; }
        public string? ChunkType { get; set; }
    }

}
