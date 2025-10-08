using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArNir.Core.DTOs.Embeddings
{
    public class EmbeddingRequestDto
    {
        public int DocumentId { get; set; }
        public string Provider { get; set; } = "OpenAI"; // or HuggingFace
        public string Model { get; set; } = "text-embedding-ada-002";
    }
}
