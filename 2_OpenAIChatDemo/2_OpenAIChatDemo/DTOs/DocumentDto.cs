namespace _2_OpenAIChatDemo.DTOs
{
    public class DocumentDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Type { get; set; } = "";
        public string UploadedBy { get; set; } = "";
        public DateTime UploadedAt { get; set; }
    }
}
