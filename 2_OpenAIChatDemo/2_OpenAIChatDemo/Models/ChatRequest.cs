using System.Text.Json.Serialization;

namespace _2_OpenAIChatDemo.Models
{
    public class ChatRequest
    {
        [JsonPropertyName("messages")]
        public List<ChatMessage> Messages { get; set; } = new();
    }
}
