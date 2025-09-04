using System.Text.Json.Serialization;

namespace _2_OpenAIChatDemo.DTOs
{
    public class ChatMessageDto
    {
        [JsonPropertyName("role")]
        public string Role { get; set; } = "";

        [JsonPropertyName("content")]
        public string Content { get; set; } = "";
    }
}
