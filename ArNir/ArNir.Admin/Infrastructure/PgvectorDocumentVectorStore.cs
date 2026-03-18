using ArNir.Core.Entities;
using ArNir.Data;
using ArNir.RAG.Interfaces;
using ArNir.RAG.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;
using Pgvector;
// Alias avoids ambiguity with ArNir.RAG.Models.RetrievalResult
using EmbeddingSearchResultDto = ArNir.Core.DTOs.Embeddings.EmbeddingSearchResult;

namespace ArNir.Admin.Infrastructure;

/// <summary>
/// Real implementation of <see cref="IDocumentVectorStore"/> that persists chunk embeddings
/// in PostgreSQL using the pgvector extension via <see cref="VectorDbContext"/>.
/// <para>
/// <b>ChunkId encoding:</b> The pipeline encodes the SQL Server document ID and chunk index
/// in the <c>chunkId</c> string as <c>"sql:{sqlDocId}:{chunkIndex}"</c> when a
/// <c>LegacySqlDocumentId</c> is available on <see cref="IngestionRequest"/>.
/// This class parses that encoding to resolve the correct <see cref="DocumentChunk.Id"/> (int FK)
/// needed by the <see cref="Embedding"/> entity.
/// </para>
/// <para>
/// Replaces the dev-only <c>NullDocumentVectorStore</c> stub. Registered as <b>Scoped</b> in
/// <c>ArNir.Admin/Program.cs</c> after <c>AddArNirRAG()</c>.
/// </para>
/// </summary>
public sealed class PgvectorDocumentVectorStore : IDocumentVectorStore
{
    private readonly IDbContextFactory<VectorDbContext>  _pgFactory;
    private readonly IDbContextFactory<ArNirDbContext>   _sqlFactory;
    private readonly ILogger<PgvectorDocumentVectorStore> _logger;

    /// <summary>
    /// Initialises a new instance of <see cref="PgvectorDocumentVectorStore"/>.
    /// </summary>
    public PgvectorDocumentVectorStore(
        IDbContextFactory<VectorDbContext>   pgFactory,
        IDbContextFactory<ArNirDbContext>    sqlFactory,
        ILogger<PgvectorDocumentVectorStore> logger)
    {
        _pgFactory  = pgFactory;
        _sqlFactory = sqlFactory;
        _logger     = logger;
    }

    /// <inheritdoc />
    public async Task StoreAsync(string chunkId, float[] vector, Dictionary<string, string>? metadata = null)
    {
        await StoreBatchAsync(new[] { (chunkId, vector) });
    }

    /// <inheritdoc />
    /// <remarks>
    /// Each <paramref name="items"/> entry has a <c>chunkId</c> string.
    /// When the format is <c>"sql:{sqlDocId}:{chunkIndex}"</c> this method resolves the
    /// <see cref="DocumentChunk.Id"/> FK from SQL Server and stores a real <see cref="Embedding"/>
    /// row in PostgreSQL. Unknown formats are skipped with a warning log.
    /// </remarks>
    public async Task StoreBatchAsync(IEnumerable<(string chunkId, float[] vector)> items)
    {
        var itemList = items.ToList();
        if (itemList.Count == 0) return;

        // Separate items we can resolve from those we cannot
        var embeddings = new List<Embedding>();

        // Batch-load SQL chunks for all unique SQL document IDs in this batch
        var sqlDocIds = itemList
            .Select(i => TryParseSqlKey(i.chunkId, out var d, out _) ? d : (int?)null)
            .Where(d => d.HasValue)
            .Select(d => d!.Value)
            .Distinct()
            .ToList();

        Dictionary<(int docId, int chunkOrder), int> chunkKeyToSqlId = new();

        if (sqlDocIds.Count > 0)
        {
            await using var sqlCtx = await _sqlFactory.CreateDbContextAsync();
            var sqlChunks = await sqlCtx.DocumentChunks
                .Where(c => sqlDocIds.Contains(c.DocumentId))
                .OrderBy(c => c.DocumentId).ThenBy(c => c.ChunkOrder)
                .Select(c => new { c.Id, c.DocumentId, c.ChunkOrder })
                .ToListAsync();

            foreach (var sc in sqlChunks)
                chunkKeyToSqlId[(sc.DocumentId, sc.ChunkOrder)] = sc.Id;
        }

        var now = DateTime.UtcNow;

        foreach (var (chunkId, vector) in itemList)
        {
            if (!TryParseSqlKey(chunkId, out int sqlDocId, out int chunkIndex))
            {
                _logger.LogWarning(
                    "VectorStore: chunkId '{ChunkId}' is not in 'sql:{{docId}}:{{index}}' format — skipped.",
                    chunkId);
                continue;
            }

            if (!chunkKeyToSqlId.TryGetValue((sqlDocId, chunkIndex), out int sqlChunkId))
            {
                // Fallback: try positional index (in case ChunkOrder differs from ChunkIndex)
                await using var sqlCtx2 = await _sqlFactory.CreateDbContextAsync();
                var allChunks = await sqlCtx2.DocumentChunks
                    .Where(c => c.DocumentId == sqlDocId)
                    .OrderBy(c => c.ChunkOrder)
                    .Select(c => c.Id)
                    .ToListAsync();

                if (chunkIndex < allChunks.Count)
                {
                    sqlChunkId = allChunks[chunkIndex];
                    _logger.LogDebug(
                        "VectorStore: ChunkOrder {Order} not found for doc {DocId}; " +
                        "using positional index {Idx} → SqlChunkId={SqlId}.",
                        chunkIndex, sqlDocId, chunkIndex, sqlChunkId);
                }
                else
                {
                    _logger.LogWarning(
                        "VectorStore: No DocumentChunk found for DocId={DocId}, ChunkIndex={Idx} — skipped.",
                        sqlDocId, chunkIndex);
                    continue;
                }
            }

            if (vector == null || vector.Length == 0)
            {
                _logger.LogWarning("VectorStore: Empty vector for chunkId '{ChunkId}' — skipped.", chunkId);
                continue;
            }

            embeddings.Add(new Embedding
            {
                EmbeddingId = Guid.NewGuid(),
                ChunkId     = sqlChunkId,
                Model       = "text-embedding-ada-002",
                Vector      = new Vector(vector),
                CreatedAt   = now
            });
        }

        if (embeddings.Count == 0)
        {
            _logger.LogWarning("VectorStore: No embeddings to persist from batch of {Total} items.", itemList.Count);
            return;
        }

        await using var pgCtx = await _pgFactory.CreateDbContextAsync();
        await pgCtx.Embeddings.AddRangeAsync(embeddings);
        await pgCtx.SaveChangesAsync();

        _logger.LogInformation(
            "VectorStore: Persisted {Count} embeddings to PostgreSQL (skipped {Skipped}).",
            embeddings.Count, itemList.Count - embeddings.Count);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<RetrievalResult>> SearchAsync(float[] queryVector, int topK = 5)
    {
        await using var pgCtx = await _pgFactory.CreateDbContextAsync();
        var qv = new Vector(queryVector);

        // Cosine similarity search via pgvector <=> operator (cosine distance, lower = closer).
        // Uses raw SQL identical to RetrievalService — the LINQ provider does not support <=>
        // directly, so we fall back to Database.SqlQueryRaw which maps to EF's keyless entity support.
        var results = await pgCtx.Database
            .SqlQueryRaw<EmbeddingSearchResultDto>(@"
                SELECT e.""ChunkId"",
                       1 - (e.""Vector"" <=> @queryVec) AS ""Score""
                FROM ""Embeddings"" e
                ORDER BY e.""Vector"" <=> @queryVec
                LIMIT {0}",
                topK,
                new NpgsqlParameter("queryVec", qv))
            .ToListAsync();

        // RetrievalResult.ChunkId is Guid; SQL stores int IDs.
        // Encode the int in the first component of the Guid so the value round-trips.
        return results
            .Select(r => new RetrievalResult
            {
                ChunkId = new Guid(r.ChunkId, 0, 0, new byte[8]),
                Score   = (float)r.Score
            })
            .ToList();
    }

    /// <inheritdoc />
    public async Task DeleteByDocumentAsync(string documentId)
    {
        if (!int.TryParse(documentId, out int sqlDocId))
        {
            _logger.LogWarning(
                "VectorStore: DeleteByDocumentAsync called with non-integer documentId '{Id}' — skipped.",
                documentId);
            return;
        }

        await using var sqlCtx = await _sqlFactory.CreateDbContextAsync();
        var chunkIds = await sqlCtx.DocumentChunks
            .Where(c => c.DocumentId == sqlDocId)
            .Select(c => c.Id)
            .ToListAsync();

        if (chunkIds.Count == 0) return;

        await using var pgCtx = await _pgFactory.CreateDbContextAsync();
        var toDelete = await pgCtx.Embeddings
            .Where(e => chunkIds.Contains(e.ChunkId))
            .ToListAsync();

        pgCtx.Embeddings.RemoveRange(toDelete);
        await pgCtx.SaveChangesAsync();

        _logger.LogInformation(
            "VectorStore: Deleted {Count} embeddings for document {DocId}.",
            toDelete.Count, sqlDocId);
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Tries to parse a chunkId in the format <c>"sql:{docId}:{chunkIndex}"</c>.
    /// </summary>
    private static bool TryParseSqlKey(string chunkId, out int docId, out int chunkIndex)
    {
        docId      = 0;
        chunkIndex = 0;

        if (!chunkId.StartsWith("sql:", StringComparison.OrdinalIgnoreCase))
            return false;

        var parts = chunkId.Split(':');
        if (parts.Length != 3) return false;

        return int.TryParse(parts[1], out docId) &&
               int.TryParse(parts[2], out chunkIndex);
    }
}
