using _2_OpenAIChatDemo.Models;
using _2_OpenAIChatDemo.Models.OpenAIChatDemo.Models;
using Microsoft.EntityFrameworkCore;

namespace _2_OpenAIChatDemo.Data
{
    public class ChatDbContext : DbContext
    {
        public ChatDbContext(DbContextOptions<ChatDbContext> options) : base(options) { }

        public DbSet<ChatSession> ChatSessions { get; set; }
        public DbSet<ChatMessage> ChatMessages { get; set; }
        public DbSet<AdminUser> AdminUsers { get; set; }
        public DbSet<PromptTemplate> PromptTemplates { get; set; }
        public DbSet<PromptTemplateParameter> PromptTemplateParameters { get; set; }
        public DbSet<PromptTemplateVersion> PromptTemplateVersions { get; set; }
        // New for Phase 2.4
        public DbSet<SessionComparison> SessionComparisons { get; set; }
        public DbSet<ComparisonResult> ComparisonResults { get; set; }

        // NEW: Document ingestion tables
        public DbSet<Document> Documents { get; set; }
        public DbSet<DocumentChunk> DocumentChunks { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<SessionComparison>()
                .HasMany(c => c.Results)
                .WithOne(r => r.SessionComparison)
                .HasForeignKey(r => r.SessionComparisonId);

            modelBuilder.Entity<ComparisonResult>()
                .Property(r => r.Provider)
                .HasMaxLength(100);

            modelBuilder.Entity<ComparisonResult>()
                .Property(r => r.ModelName)
                .HasMaxLength(100);

            modelBuilder.Entity<ComparisonResult>()
                .Property(r => r.ErrorCode)
                .HasMaxLength(50);

            modelBuilder.Entity<ComparisonResult>()
                .Property(r => r.ErrorMessage)
                .HasMaxLength(500);

            // Document → Chunks relationship
            modelBuilder.Entity<Document>()
                .HasMany(d => d.Chunks)
                .WithOne(c => c.Document)
                .HasForeignKey(c => c.DocumentId)
                .OnDelete(DeleteBehavior.Cascade);

            // Document unique constraint (optional)
            modelBuilder.Entity<Document>()
                .HasIndex(d => new { d.Name, d.Version });
        }

    }
}
