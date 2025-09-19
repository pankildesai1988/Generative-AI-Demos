using ArNir.Core.DTOs.Embeddings;
using ArNir.Services.Interfaces;
using ArNir.Data;
using ArNir.Services.Provider;
using Microsoft.EntityFrameworkCore;
using Pgvector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArNir.Services
{
    public class EmbeddingService : IEmbeddingService
    {
        private readonly ArNirDbContext _sqlContext;       // MS SQL (Docs + Chunks)
        private readonly VectorDbContext _vectorContext;   // Postgres (Embeddings)
        private readonly IEmbeddingProvider _embeddingProvider;

        public EmbeddingService(
            ArNirDbContext sqlContext,
            VectorDbContext vectorContext,
            IEmbeddingProvider embeddingProvider)
        {
            _sqlContext = sqlContext;
            _vectorContext = vectorContext;
            _embeddingProvider = embeddingProvider;
        }

        public async Task<List<EmbeddingResultDto>> GenerateForDocumentAsync(EmbeddingRequestDto request)
        {
            // ✅ Get document chunks from MS SQL
            var chunks = await _sqlContext.DocumentChunks
                .Where(c => c.DocumentId == request.DocumentId)
                .ToListAsync();

            var results = new List<EmbeddingResultDto>();

            foreach (var chunk in chunks)
            {
                // Generate embedding vector
                var vectorArray = await _embeddingProvider.GenerateEmbeddingAsync(chunk.Text, request.Model);

                // ✅ Wrap into Pgvector.Vector
                var vector = new Vector(vectorArray);

                var embedding = new Core.Entities.Embedding
                {
                    EmbeddingId = Guid.NewGuid(),
                    ChunkId = chunk.Id,
                    Model = request.Model,
                    Vector = vector,
                    CreatedAt = DateTime.UtcNow
                };

                _vectorContext.Embeddings.Add(embedding);

                results.Add(new EmbeddingResultDto
                {
                    EmbeddingId = embedding.EmbeddingId,
                    ChunkId = embedding.ChunkId,
                    Model = embedding.Model,
                    CreatedAt = embedding.CreatedAt
                });
            }

            await _vectorContext.SaveChangesAsync();
            return results;
        }

        public async Task<float[]> GenerateForQueryAsync(string text, string model = "text-embedding-ada-002")
        {
            return await _embeddingProvider.GenerateEmbeddingAsync(text, model);
        }
        public async Task DeleteEmbeddingsForDocumentAsync(int documentId)
        {
            // Get chunk IDs for this document from SQL Server
            var chunkIds = await _sqlContext.DocumentChunks
                .Where(c => c.DocumentId == documentId)
                .Select(c => c.Id)
                .ToListAsync();

            if (!chunkIds.Any()) return;

            // Delete embeddings from Postgres
            var embeddings = _vectorContext.Embeddings
                .Where(e => chunkIds.Contains(e.ChunkId));

            _vectorContext.Embeddings.RemoveRange(embeddings);

            await _vectorContext.SaveChangesAsync();
        }
        public async Task<List<EmbeddingResultDto>> RebuildEmbeddingsForDocumentAsync(int documentId, string model = "text-embedding-ada-002")
        {
            // 1. Delete old embeddings
            await DeleteEmbeddingsForDocumentAsync(documentId);

            // 2. Rebuild embeddings from cleaned chunks
            var request = new EmbeddingRequestDto
            {
                DocumentId = documentId,
                Model = model
            };

            return await GenerateForDocumentAsync(request);
        }

    }
}
