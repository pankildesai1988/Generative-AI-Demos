using ArNir.Core.Entities;
using ArNir.Data;
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
    /// </summary>
    public class ContextMemoryService : IContextMemoryService
    {
        private readonly ArNirDbContext _sqlContext;

        public ContextMemoryService(ArNirDbContext sqlContext)
        {
            _sqlContext = sqlContext;
        }

        /// <summary>
        /// Save a chat message in the SQL Server ChatMemories table.
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
