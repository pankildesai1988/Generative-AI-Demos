using System.ComponentModel.DataAnnotations;

namespace _2_OpenAIChatDemo.Models
{
    public class ChatSession
    {
        [Key]
        public int Id { get; set; }

        public string UserId { get; set; } = "default"; // later can support multi-user

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public string? Title { get; set; }  // ✅ new property

        public List<ChatMessage> Messages { get; set; } = new();
    }
}
