using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArNir.Core.DTOs
{
    public class DocumentChunkDto
    {
        public int Id { get; set; }
        public int ChunkOrder { get; set; }
        public string Text { get; set; } = "";
    }
}
