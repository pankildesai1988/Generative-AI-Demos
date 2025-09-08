using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace _2_OpenAIChatDemo.Models
{
    public class ChatMessage
    {
        [Key]
        public int Id { get; set; }

        public int ChatSessionId { get; set; }
        public ChatSession ChatSession { get; set; } = null!;

        public string Role { get; set; } = "";   // "user" or "assistant"
        public string Content { get; set; } = "";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
