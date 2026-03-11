using ArNir.Core.Entities;
using ArNir.Data;
using ArNir.Services.Interfaces;
using ArNir.Services.Provider;
using Microsoft.EntityFrameworkCore;
using Pgvector;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ArNir.Services
{
    public class ChatEmbeddingService : IChatEmbeddingService
    {
        private readonly ArNirDbContext _sqlContext;
        private readonly VectorDbContext _pgContext;
        private readonly IEmbeddingProvider _embeddingProvider;

        public ChatEmbeddingService(ArNirDbContext sqlContext, VectorDbContext pgContext, IEmbeddingProvider embeddingProvider)
        {
            _sqlContext = sqlContext;
            _pgContext = pgContext;
            _embeddingProvider = embeddingProvider;
        }

        public async Task<Guid?> GenerateEmbeddingForMessageAsync(int chatMemoryId, string text, string model = "text-embedding-3-small")
        {
            var vectorArray = await _embeddingProvider.GenerateEmbeddingAsync(text, model);
            var vector = new Vector(vectorArray);

            var embedding = new ChatEmbedding
            {
                ChatMemoryId = chatMemoryId,
                Model = model,
                Vector = vector
            };

            _pgContext.ChatEmbeddings.Add(embedding);
            await _pgContext.SaveChangesAsync();

            var memory = await _sqlContext.ChatMemories.FindAsync(chatMemoryId);
            if (memory != null)
            {
                memory.EmbeddingRefId = embedding.EmbeddingId;
                _sqlContext.ChatMemories.Update(memory);
                await _sqlContext.SaveChangesAsync();
            }

            return embedding.EmbeddingId;
        }

        public async Task<ChatEmbedding?> FindSimilarAsync(float[] queryVector, int limit = 1)
        {
            var vector = new Vector(queryVector);
            var sql = @"SELECT * FROM ""ChatEmbeddings"" 
                        ORDER BY ""Vector"" <-> @p0 
                        LIMIT @p1";

            var results = await _pgContext.ChatEmbeddings
                .FromSqlRaw(sql, vector, limit)
                .ToListAsync();

            return results.FirstOrDefault();
        }
    }
}
