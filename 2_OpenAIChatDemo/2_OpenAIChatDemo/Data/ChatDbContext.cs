using _2_OpenAIChatDemo.Models;
using Microsoft.EntityFrameworkCore;

namespace _2_OpenAIChatDemo.Data
{
    public class ChatDbContext : DbContext
    {
        public ChatDbContext(DbContextOptions<ChatDbContext> options) : base(options) { }

        public DbSet<ChatSession> ChatSessions { get; set; }
        public DbSet<ChatMessage> ChatMessages { get; set; }
    }
}
