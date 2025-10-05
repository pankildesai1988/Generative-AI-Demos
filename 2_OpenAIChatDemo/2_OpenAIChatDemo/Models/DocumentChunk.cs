using System;
using System.Text.Json.Serialization;

namespace _2_OpenAIChatDemo.Models
{
    public class DocumentChunk
    {
        public int Id { get; set; }                         // Primary Key
        public int DocumentId { get; set; }                 // FK to Document
        public int ChunkOrder { get; set; }                 // Order of chunk
        public string Text { get; set; } = string.Empty;    // Chunk text

        // Navigation
        [JsonIgnore]
        public Document Document { get; set; } = null!;
    }
}
