using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArNir.Core.DTOs.RAG
{
    public class RagResultDto
    {
        public string UserQuery { get; set; }
        public string BaselineAnswer { get; set; }
        public string RagAnswer { get; set; }
        // Retrieved context
        public List<RagChunkDto> RetrievedChunks { get; set; } = new();

        // Latency metrics
        public long RetrievalLatencyMs { get; set; }
        public long LlmLatencyMs { get; set; }
        public long TotalLatencyMs => RetrievalLatencyMs + LlmLatencyMs;

        // SLA check
        public bool IsWithinSla => TotalLatencyMs <= 1000; // < 1s target
        public string Provider { get; set; }   // NEW
        public string Model { get; set; }      // NEW
    }

}
