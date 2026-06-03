using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArNir.Core.DTOs.Documents
{
    public class DocumentChunkDto
    {
        public int Id { get; set; }
        public int ChunkOrder { get; set; }
        public string Text { get; set; } = "";

        /// <summary>
        /// 1-based page number of the source document for this chunk. Null for non-paginated formats.
        /// </summary>
        public int? PageNumber { get; set; }
    }
}
