using System;
using System.ComponentModel.DataAnnotations;

namespace ArNir.Core.Entities
{
    public class RagComparisonHistory
    {
        [Key]
        public int Id { get; set; }

        public string UserQuery { get; set; }
        public string BaselineAnswer { get; set; }
        public string RagAnswer { get; set; }
        public string RetrievedChunksJson { get; set; } // store chunks as JSON

        public long RetrievalLatencyMs { get; set; }
        public long LlmLatencyMs { get; set; }
        public long TotalLatencyMs { get; set; }
        public bool IsWithinSla { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
