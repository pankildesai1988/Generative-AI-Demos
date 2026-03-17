using ArNir.Core.Entities;
using ArNir.Data;
using ArNir.Memory.Interfaces;
using ArNir.Memory.Models;
using ArNir.Services.AI.Interfaces;
using ArNir.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace ArNir.Services.AI
{
    /// <summary>
    /// Handles saving and retrieving chat messages from SQL Server.
    /// Embeddings are managed by ChatEmbeddingService (PostgreSQL).
    /// <para>
    /// Migration bridge: also writes to <see cref="IEpisodicMemory"/> in parallel so the new
    /// memory module stays in sync. <see cref="IContextMemoryService"/> will be deprecated
    /// once all consumers have migrated to <see cref="IEpisodicMemory"/> directly.
    /// </para>
    /// </summary>
    public class ContextMemoryService : IContextMemoryService
    {
        private readonly ArNirDbContext _sqlContext;
        private readonly IEpisodicMemory _episodicMemory;

        public ContextMemoryService(ArNirDbContext sqlContext, IEpisodicMemory episodicMemory)
        {
            _sqlContext = sqlContext;
            _episodicMemory = episodicMemory;
        }

        /// <summary>
        /// Save a chat message in the SQL Server ChatMemories table and mirrors the entry
        /// into <see cref="IEpisodicMemory"/> for the new memory module.
        /// </summary>
        public async Task SaveContextAsync(string sessionId, string message)
        {
            if (string.IsNullOrWhiteSpace(sessionId) || string.IsNullOrWhiteSpace(message))
                return;

            var memory = new ChatMemory
            {
                SessionId = sessionId,
                UserMessage = message
            };

            _sqlContext.ChatMemories.Add(memory);
            await _sqlContext.SaveChangesAsync();

            // Mirror into IEpisodicMemory (new memory module migration bridge)
            await _episodicMemory.AddAsync(new MemoryEntry
            {
                SessionId = sessionId,
                Role = "user",
                Content = message,
                CreatedAt = DateTime.UtcNow
            });
        }

        /// <summary>
        /// Retrieve the last chat message in the specified session.
        /// </summary>
        public async Task<string?> RetrieveContextAsync(string sessionId)
        {
            if (string.IsNullOrWhiteSpace(sessionId))
                return null;

            var lastMessage = await _sqlContext.ChatMemories
                .Where(m => m.SessionId == sessionId)
                .OrderByDescending(m => m.CreatedAt)
                .Select(m => m.UserMessage)
                .FirstOrDefaultAsync();

            return lastMessage;
        }

        /// <summary>
        /// This method is a placeholder — embeddings are handled in PostgreSQL by ChatEmbeddingService.
        /// </summary>
        public Task SaveEmbeddingAsync(string sessionId, float[] embedding)
        {
            // Intentionally no-op in SQL layer.
            // Chat embeddings are managed via ChatEmbeddingService in PostgreSQL.
            return Task.CompletedTask;
        }

        /// <summary>
        /// Semantic similarity is handled in PostgreSQL (ChatEmbeddingService).
        /// </summary>
        public Task<ChatMemory?> FindSimilarAsync(float[] embedding, int limit = 1)
        {
            // Not supported in SQL Server context.
            return Task.FromResult<ChatMemory?>(null);
        }
    }
}
