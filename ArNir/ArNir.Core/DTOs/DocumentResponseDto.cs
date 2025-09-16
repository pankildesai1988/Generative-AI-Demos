using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArNir.Core.DTOs
{
    public class DocumentResponseDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Type { get; set; } = "";
        public string UploadedBy { get; set; } = "";
        public DateTime UploadedAt { get; set; }
        public List<DocumentChunkDto> Chunks { get; set; } = new();
    }
}
