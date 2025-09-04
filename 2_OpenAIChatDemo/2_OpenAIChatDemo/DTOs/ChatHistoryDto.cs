namespace _2_OpenAIChatDemo.DTOs
{
    public class ChatHistoryDto
    {
        public int SessionId { get; set; }
        public List<ChatMessageDto> Messages { get; set; } = new();
    }
}
