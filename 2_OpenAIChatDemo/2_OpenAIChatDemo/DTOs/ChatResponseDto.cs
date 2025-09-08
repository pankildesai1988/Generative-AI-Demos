namespace _2_OpenAIChatDemo.DTOs
{
    public class ChatResponseDto
    {
        public bool Success { get; set; }
        public string Error { get; set; }
        public int SessionId { get; set; }  // ✅ new
        public string Data { get; set; }
    }

}
