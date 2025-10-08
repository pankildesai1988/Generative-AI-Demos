using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArNir.Core.DTOs.RAG
{
    public class RagHistoryListDto
    {
        public int Id { get; set; }
        public string Query { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public bool IsWithinSla { get; set; }
        public long RetrievalLatencyMs { get; set; }
        public long LlmLatencyMs { get; set; }
        public long TotalLatencyMs { get; set; }
        public string PromptStyle { get; set; } = string.Empty;
        public string Provider { get; set; } = "OpenAI";
        public string Model { get; set; } = "gpt-4";
        public int QueryTokens { get; set; }
        public int ContextTokens { get; set; }
        public int TotalTokens { get; set; }
    }
}
