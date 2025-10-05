namespace _2_OpenAIChatDemo.DTOs
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
