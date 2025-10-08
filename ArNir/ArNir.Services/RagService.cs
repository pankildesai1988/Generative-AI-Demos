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

        public async Task<RagResultDto> RunRagAsync(string query, int topK = 5, bool useHybrid = true, string promptStyle = "rag", bool saveAsNew = true)
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

            // Build prompts based on style
            string baselinePrompt = BuildPrompt(query, context, promptStyle == "rag" || promptStyle == "hybrid" ? "zero-shot" : promptStyle);
            string ragPrompt = BuildPrompt(query, context, promptStyle);

            result.BaselineAnswer = await _openAiService.GetCompletionAsync(baselinePrompt);
            result.RagAnswer = await _openAiService.GetCompletionAsync(ragPrompt);
            swLlm.Stop();
            result.LlmLatencyMs = swLlm.ElapsedMilliseconds;

            if (saveAsNew)
            {
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
                        IsWithinSla = result.IsWithinSla,
                        PromptStyle = promptStyle
                    };
                    sqlContext.RagComparisonHistories.Add(history);
                    await sqlContext.SaveChangesAsync();
                }
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

        private string BuildPrompt(string query, string retrievedChunks, string style)
        {
            switch (style)
            {
                case "zero-shot":
                    return $"You are a helpful assistant.\nAnswer the following question:\n\nQuery: {query}";

                case "few-shot":
                    return $@"You are a helpful assistant.
Here are some examples of how to answer:

Q: What is cloud computing?
A: Cloud computing means using remote servers to store and process data instead of local machines.

Q: What is AI?
A: AI is the simulation of human intelligence by machines.

Now, answer this query:

Query: {query}";

                case "role":
                    return $"You are an expert meteorologist specializing in explaining weather concepts.\nAnswer clearly:\n\nQuery: {query}";

                case "rag":
                    return $"You are a helpful assistant.\nUse the following context to answer:\n\nContext:\n{retrievedChunks}\n\nQuery: {query}\n\nAnswer ONLY using the context above.";

                case "hybrid":
                    return $@"You are an expert meteorologist.
Use the retrieved context below to answer the query.

Context:
{retrievedChunks}

Query: {query}

Rules:
1. Use only relevant context.
2. Be concise (max 3 sentences).
3. Cite the source document titles.

Final Answer:";

                default:
                    return query;
            }
        }

    }
}
