using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArNir.Core.Entities
{
    public class DocumentChunk
    {
        public int Id { get; set; }
        public int DocumentId { get; set; }
        public int ChunkOrder { get; set; }
        public string Text { get; set; } = "";

        public Document Document { get; set; } = null!;
    }
}
