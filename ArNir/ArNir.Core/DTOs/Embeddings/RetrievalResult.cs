using ArNir.Core.DTOs.Documents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArNir.Core.DTOs.Embeddings
{
    public class RetrievalResult
    {
        public List<ChunkResultDto> Results { get; set; } = new();
        public RetrievalTimingDto Timing { get; set; } = new();
    }
}
