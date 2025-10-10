using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArNir.Core.DTOs.RAG
{
    public class RagRequestDto
    {
        public string Query { get; set; } = string.Empty;
        public int TopK { get; set; } = 3;
        public bool UseHybrid { get; set; } = true;
        public string PromptStyle { get; set; } = "rag";
        public bool SaveAsNew { get; set; } = true;
        public string Provider { get; set; } = "OpenAI";
        public string Model { get; set; } = "gpt-4o-mini";
    }
}
