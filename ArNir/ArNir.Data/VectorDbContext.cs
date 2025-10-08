using ArNir.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace ArNir.Data
{
    public class VectorDbContext : DbContext
    {
        public DbSet<Embedding> Embeddings { get; set; }

        public VectorDbContext(DbContextOptions<VectorDbContext> options)
            : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // ✅ Ensure Postgres knows about pgvector extension
            modelBuilder.HasPostgresExtension("vector");

            modelBuilder.Entity<Embedding>(entity =>
            {
                entity.HasKey(e => e.EmbeddingId);

                entity.Property(e => e.Vector)
                      .HasColumnType("vector(1536)");
            });
        }
    }
}
