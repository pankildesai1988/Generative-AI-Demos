using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArNir.Core.DTOs.Documents
{
    public class ChunkResultDto   // <-- was internal, must be public
    {
        public int ChunkId { get; set; }
        public int DocumentId { get; set; }
        public string Text { get; set; } = string.Empty;
        public double Score { get; set; }
        public Dictionary<string, string>? Metadata { get; set; }
        // ✅ New field
        public string Source { get; set; } = "Semantic";
        public int? PageNumber { get; set; }
        public float? BboxX1 { get; set; }
        public float? BboxY1 { get; set; }
        public float? BboxX2 { get; set; }
        public float? BboxY2 { get; set; }
        public string? ChunkType { get; set; }
    }
}
