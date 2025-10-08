using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArNir.Core.DTOs.RAG
{
    public class RagHistoryDetailDto
    {
        public int Id { get; set; }
        public string Query { get; set; } = string.Empty;
        public string BaselineAnswer { get; set; } = string.Empty;
        public string RagAnswer { get; set; } = string.Empty;
        public string RetrievedChunksJson { get; set; } = string.Empty;
        public long RetrievalLatencyMs { get; set; }
        public long LlmLatencyMs { get; set; }
        public long TotalLatencyMs { get; set; }
        public bool IsWithinSla { get; set; }
        public string PromptStyle { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
