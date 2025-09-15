using System;
using System.Collections.Generic;

namespace _2_OpenAIChatDemo.Models
{
    public class Document
    {
        public int Id { get; set; }                        // Primary Key
        public string Name { get; set; } = string.Empty;   // File name
        public string Type { get; set; } = string.Empty;   // pdf, docx, md, sql
        public string? Metadata { get; set; }              // Extra info (JSON)
        public int Version { get; set; } = 1;              // Document versioning
        public string UploadedBy { get; set; } = "admin";  // User who uploaded
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public ICollection<DocumentChunk> Chunks { get; set; } = new List<DocumentChunk>();
    }
}
