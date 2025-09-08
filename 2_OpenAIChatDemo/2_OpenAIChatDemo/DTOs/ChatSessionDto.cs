namespace _2_OpenAIChatDemo.DTOs
{
    public class ChatSessionDto
    {
        public int SessionId { get; set; }
        public string Title { get; set; }
        public string Model { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime LastMessageAt { get; set; }
    }
}
