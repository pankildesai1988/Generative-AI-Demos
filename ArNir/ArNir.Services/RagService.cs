using ArNir.Core.DTOs.RAG;
using ArNir.Core.Entities;
using ArNir.Data;
using ArNir.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ArNir.Services
{
    public class RagService : IRagService
    {
        private readonly IRetrievalService _retrievalService;
        private readonly IOpenAiService _openAiService;
        private readonly IDbContextFactory<ArNirDbContext> _sqlFactory;

        public RagService(IRetrievalService retrievalService, IOpenAiService openAiService, IDbContextFactory<ArNirDbContext> sqlFactory)
        {
            _retrievalService = retrievalService;
            _openAiService = openAiService;
            _sqlFactory = sqlFactory;
        }

        public async Task<RagResultDto> RunRagAsync(string query, int topK = 5, bool useHybrid = true)
        {
            var result = new RagResultDto { UserQuery = query };

            // 1. Retrieve chunks
            var swRetrieval = Stopwatch.StartNew();
            var retrievedChunks = await _retrievalService.SearchAsync(query, topK, useHybrid);
            swRetrieval.Stop();
            result.RetrievalLatencyMs = swRetrieval.ElapsedMilliseconds;

            result.RetrievedChunks = retrievedChunks.Select((c, i) => new RagChunkDto
            {
                DocumentId = c.DocumentId,
                DocumentTitle = c.Metadata != null && c.Metadata.ContainsKey("DocumentName")
                    ? c.Metadata["DocumentName"]
                    : "Unknown",
                ChunkText = c.Text,
                Rank = i + 1,
                RetrievalType = c.Source
            }).ToList();

            // 2. Baseline LLM (no context)
            var swLlm = Stopwatch.StartNew();
            result.BaselineAnswer = await _openAiService.GetCompletionAsync(query);

            // 3. RAG-enhanced LLM (with context)
            var context = BuildContextBlock(result.RetrievedChunks);
            var ragPrompt = $@"
You are an AI assistant. Use the following context to answer the question.
Context:
{context}

Question:
{query}";

            result.RagAnswer = await _openAiService.GetCompletionAsync(ragPrompt);
            swLlm.Stop();
            result.LlmLatencyMs = swLlm.ElapsedMilliseconds;

            // Save history
            using (var sqlContext = _sqlFactory.CreateDbContext())
            {
                var history = new RagComparisonHistory
                {
                    UserQuery = result.UserQuery,
                    BaselineAnswer = result.BaselineAnswer,
                    RagAnswer = result.RagAnswer,
                    RetrievedChunksJson = JsonSerializer.Serialize(result.RetrievedChunks),
                    RetrievalLatencyMs = result.RetrievalLatencyMs,
                    LlmLatencyMs = result.LlmLatencyMs,
                    TotalLatencyMs = result.TotalLatencyMs,
                    IsWithinSla = result.IsWithinSla
                };
                sqlContext.RagComparisonHistories.Add(history);
                await sqlContext.SaveChangesAsync();
            }

            return result;
        }

        private string BuildContextBlock(IEnumerable<RagChunkDto> chunks, int maxLength = 1500)
        {
            var sb = new StringBuilder();
            foreach (var chunk in chunks)
            {
                if (sb.Length + chunk.ChunkText.Length > maxLength) break;
                sb.AppendLine($"[Doc: {chunk.DocumentTitle} (ID: {chunk.DocumentId})] {chunk.ChunkText}");
            }
            return sb.ToString();
        }
    }
}
