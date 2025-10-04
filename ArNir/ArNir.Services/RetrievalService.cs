using ArNir.Core.DTOs;
using ArNir.Core.DTOs.Documents;
using ArNir.Core.DTOs.Embeddings;
using ArNir.Core.Entities;
using ArNir.Data;
using ArNir.Services.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Pgvector;
using System.Diagnostics;

namespace ArNir.Services
{
    public class RetrievalService : IRetrievalService
    {
        private readonly IDbContextFactory<ArNirDbContext> _sqlFactory;
        private readonly IDbContextFactory<VectorDbContext> _pgFactory;
        private readonly IEmbeddingService _embeddingService;

        public RetrievalService(
            IDbContextFactory<ArNirDbContext> sqlFactory,
            IDbContextFactory<VectorDbContext> pgFactory,
            IEmbeddingService embeddingService)
        {
            _sqlFactory = sqlFactory;
            _pgFactory = pgFactory;
            _embeddingService = embeddingService;
        }

        public async Task<List<ChunkResultDto>> SearchAsync(string query, int topK = 3, bool useHybrid = false)
        {
            var stopwatch = Stopwatch.StartNew();

            // --- 1. Generate embedding for query ---
            var embedWatch = Stopwatch.StartNew();
            var queryEmbedding = await _embeddingService.GenerateForQueryAsync(query);
            var queryVector = new Vector(queryEmbedding);
            embedWatch.Stop();

            List<EmbeddingSearchResult> semanticResults;

            // --- 2. Semantic search (Postgres pgvector) ---
            var semanticWatch = Stopwatch.StartNew();
            using (var pgContext = _pgFactory.CreateDbContext())
            {
                semanticResults = await pgContext.Database
                    .SqlQueryRaw<EmbeddingSearchResult>(@"
                        SELECT e.""ChunkId"",
                               1 - (e.""Vector"" <=> @queryVec) AS Score
                        FROM ""Embeddings"" e
                        ORDER BY e.""Vector"" <=> @queryVec
                        LIMIT {0}",
                        topK,
                        new NpgsqlParameter("queryVec", queryVector))
                    .ToListAsync();
            }
            semanticWatch.Stop();

            var chunkIds = semanticResults.Select(r => r.ChunkId).ToList();
            List<DocumentChunk> chunks;

            // --- 3. Fetch matching chunks (SQL Server) ---
            var chunkWatch = Stopwatch.StartNew();
            using (var sqlContext = _sqlFactory.CreateDbContext())
            {
                chunks = await sqlContext.DocumentChunks
                    .Include(c => c.Document)
                    .Where(c => chunkIds.Contains(c.Id))
                    .ToListAsync();
            }
            chunkWatch.Stop();

            // --- 4. Map semantic results ---
            var semanticDtos = (from r in semanticResults
                                join c in chunks on r.ChunkId equals c.Id
                                select new ChunkResultDto
                                {
                                    ChunkId = c.Id,
                                    DocumentId = c.DocumentId,
                                    Text = c.Text,
                                    Score = r.Score,
                                    Metadata = new Dictionary<string, string>
                                    {
                                        { "DocumentName", c.Document?.Name ?? "Unknown" }
                                    },
                                    Source = "Semantic"
                                }).ToList();

            if (!useHybrid)
            {
                stopwatch.Stop();
                LogTimes("Semantic", embedWatch.ElapsedMilliseconds, semanticWatch.ElapsedMilliseconds, chunkWatch.ElapsedMilliseconds, 0, stopwatch.ElapsedMilliseconds);
                return semanticDtos.OrderByDescending(x => x.Score).ToList();
            }

            // --- 5. Keyword search (SQL Server Full-Text Search with tokenized query) ---
            var keywordWatch = Stopwatch.StartNew();

            // Tokenize query into words, escape quotes, build FTS condition
            var tokens = query.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var safeTerms = tokens.Select(t => $"FORMSOF(INFLECTIONAL, \"{t.Replace("\"", "\"\"")}\")");
            var ftsCondition = string.Join(" OR ", safeTerms);

            List<DocumentChunk> keywordMatches;
            using (var sqlContext = _sqlFactory.CreateDbContext())
            {
                keywordMatches = await sqlContext.DocumentChunks
                    .FromSqlRaw(@"
                        SELECT TOP (@topK) *
                        FROM DocumentChunks
                        WHERE CONTAINS(Text, @p0)",
                        new SqlParameter("@topK", topK),
                        new SqlParameter("@p0", ftsCondition))
                    .Include(c => c.Document)
                    .ToListAsync();
            }
            keywordWatch.Stop();

            // ✅ Hybrid fallback: if keyword returned 0, just return semantic
            if (keywordMatches.Count == 0)
            {
                stopwatch.Stop();
                LogTimes("Hybrid (Fallback to Semantic)", embedWatch.ElapsedMilliseconds, semanticWatch.ElapsedMilliseconds, chunkWatch.ElapsedMilliseconds, keywordWatch.ElapsedMilliseconds, stopwatch.ElapsedMilliseconds);
                return semanticDtos.OrderByDescending(x => x.Score).ToList();
            }

            var keywordDtos = keywordMatches.Select(c => new ChunkResultDto
            {
                ChunkId = c.Id,
                DocumentId = c.DocumentId,
                Text = c.Text,
                Score = 1.0, // keyword hits get max raw score
                Metadata = new Dictionary<string, string>
                {
                    { "DocumentName", c.Document?.Name ?? "Unknown" }
                },
                Source = "Keyword"
            }).ToList();

            // --- 6. Merge & re-rank (semantic 70%, keyword 30%) ---
            var merged = semanticDtos.Concat(keywordDtos)
                .GroupBy(x => x.ChunkId)
                .Select(g => new ChunkResultDto
                {
                    ChunkId = g.Key,
                    DocumentId = g.First().DocumentId,
                    Text = g.First().Text,
                    Score = g.Max(r => r.Score) * 0.7 + (keywordDtos.Any(k => k.ChunkId == g.Key) ? 0.3 : 0),
                    Metadata = g.First().Metadata,
                    Source = g.Any(r => r.Source == "Keyword") && g.Any(r => r.Source == "Semantic")
                        ? "Hybrid"
                        : g.First().Source
                })
                .OrderByDescending(x => x.Score)
                .Take(topK)
                .ToList();

            stopwatch.Stop();

            // --- Log timings ---
            LogTimes("Hybrid", embedWatch.ElapsedMilliseconds, semanticWatch.ElapsedMilliseconds, chunkWatch.ElapsedMilliseconds, keywordWatch.ElapsedMilliseconds, stopwatch.ElapsedMilliseconds);

            return merged;
        }

        private void LogTimes(string mode, long embeddingMs, long semanticMs, long chunkMs, long keywordMs, long totalMs)
        {
            Console.WriteLine($"[{mode} Retrieval Timing] Embedding: {embeddingMs} ms | Semantic: {semanticMs} ms | ChunkFetch: {chunkMs} ms | Keyword: {keywordMs} ms | Total: {totalMs} ms");
        }
    }
}
