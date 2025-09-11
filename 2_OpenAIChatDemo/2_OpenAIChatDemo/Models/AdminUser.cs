using System.ComponentModel.DataAnnotations;

namespace _2_OpenAIChatDemo.Models
{
    public class AdminUser
    {
        [Key]
        public int Id { get; set; }
        public string Username { get; set; } = "";
        public string PasswordHash { get; set; } = "";
        public string NVARCHAR { get; set; } = "Admin";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
