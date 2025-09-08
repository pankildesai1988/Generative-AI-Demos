using System.Text.Json.Serialization;

namespace _2_OpenAIChatDemo.DTOs
{
    public class ChatRequestDto
    {
        [JsonPropertyName("sessionId")]
        public int SessionId { get; set; }

        [JsonPropertyName("messages")]
        public List<ChatMessageDto> Messages { get; set; } = new();
        public string? Model { get; set; } // ✅ allow client to pick model
    }
}
