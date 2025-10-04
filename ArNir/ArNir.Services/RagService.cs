using ArNir.Core.DTOs.Analytics;
using ArNir.Core.DTOs.RAG;
using ArNir.Core.Entities;
using ArNir.Core.Utils;
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
        private readonly IDictionary<string, ILlmService> _llmProviders;
        private readonly IRetrievalService _retrievalService;
        private readonly IDbContextFactory<ArNirDbContext> _sqlFactory;

        public RagService(IRetrievalService retrievalService,
                  OpenAiService openAiService,
                  GeminiService geminiService,
                  ClaudeService claudeService,
                  IDbContextFactory<ArNirDbContext> sqlFactory)
        {
            _retrievalService = retrievalService;
            _sqlFactory = sqlFactory;

            _llmProviders = new Dictionary<string, ILlmService>(StringComparer.OrdinalIgnoreCase)
    {
        { "OpenAI", openAiService },
        { "Gemini", geminiService },
        { "Claude", claudeService }
    };
        }

        public async Task<RagResultDto> RunRagAsync(
            string query,
            int topK = 3,
            bool useHybrid = true,
            string promptStyle = "rag",
            bool saveAsNew = true,
            string provider = "OpenAI",
            string model = "gpt-4o-mini")
        {
            var result = new RagResultDto { UserQuery = query, Provider = provider, Model = model };

            // 1. Retrieval
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

            // 2. Build prompts
            var context = BuildContextBlock(result.RetrievedChunks);

            // ✅ Token counting
            var queryTokens = TokenizerUtil.CountTokens(query);
            var contextTokens = TokenizerUtil.CountTokens(context);
            var totalTokens = queryTokens + contextTokens;
            Console.WriteLine($"[Token Debug] Query={queryTokens}, Context={contextTokens}, Total={totalTokens}");

            // Trim context if too large
            if (contextTokens > 3000)
            {
                context = string.Join("\n\n", result.RetrievedChunks.Take(2).Select(c => c.ChunkText));
                contextTokens = TokenizerUtil.CountTokens(context);
                totalTokens = queryTokens + contextTokens;
                Console.WriteLine($"[Token Debug] Context trimmed -> Query={queryTokens}, Context={contextTokens}, Total={totalTokens}");
            }

            string baselinePrompt = BuildPrompt(query, context, promptStyle == "rag" || promptStyle == "hybrid" ? "zero-shot" : promptStyle);
            string ragPrompt = BuildPrompt(query, context, promptStyle);

            // 3. Provider dispatch
            if (!_llmProviders.ContainsKey(provider))
                throw new NotImplementedException($"Provider {provider} not implemented.");

            var llmService = _llmProviders[provider];

            var swLlm = Stopwatch.StartNew();
            result.BaselineAnswer = await llmService.GetCompletionAsync(baselinePrompt, model);
            result.RagAnswer = await llmService.GetCompletionAsync(ragPrompt, model);
            swLlm.Stop();
            result.LlmLatencyMs = swLlm.ElapsedMilliseconds;

            
            // 4. Save history
            if (saveAsNew)
            {
                const int SLA_THRESHOLD_MS = 5000; // 5 seconds
                using var sqlContext = _sqlFactory.CreateDbContext();
                var history = new RagComparisonHistory
                {
                    UserQuery = result.UserQuery,
                    BaselineAnswer = result.BaselineAnswer,
                    RagAnswer = result.RagAnswer,
                    RetrievedChunksJson = JsonSerializer.Serialize(result.RetrievedChunks),
                    RetrievalLatencyMs = result.RetrievalLatencyMs,
                    LlmLatencyMs = result.LlmLatencyMs,
                    TotalLatencyMs = result.TotalLatencyMs,
                    IsWithinSla = result.TotalLatencyMs <= SLA_THRESHOLD_MS, // ✅ SLA flag
                    PromptStyle = promptStyle,
                    Provider = provider,
                    Model = model,
                    // ✅ New: token counts
                    QueryTokens = queryTokens,
                    ContextTokens = contextTokens,
                    TotalTokens = totalTokens
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
        public async Task<AnalyticsResponse<AvgLatencyDto>> GetAverageLatenciesAsync(
            DateTime? startDate, DateTime? endDate, string? slaStatus, string? promptStyle)
        {
            using var ctx = _sqlFactory.CreateDbContext();
            var q = ctx.RagComparisonHistories.AsQueryable();

            if (startDate.HasValue)
                q = q.Where(x => x.CreatedAt >= startDate.Value);

            if (endDate.HasValue)
                q = q.Where(x => x.CreatedAt <= endDate.Value);

            if (!string.IsNullOrEmpty(promptStyle))
                q = q.Where(x => x.PromptStyle == promptStyle);

            if (!string.IsNullOrEmpty(slaStatus))
            {
                if (slaStatus.Equals("ok", StringComparison.OrdinalIgnoreCase))
                    q = q.Where(x => x.IsWithinSla);
                else if (slaStatus.Equals("slow", StringComparison.OrdinalIgnoreCase))
                    q = q.Where(x => !x.IsWithinSla);
            }

            var data = await q
                .GroupBy(x => new { x.Provider, x.Model })
                .Select(g => new AvgLatencyDto
                {
                    Provider = g.Key.Provider,
                    Model = g.Key.Model,
                    AvgRetrievalLatencyMs = g.Average(x => x.RetrievalLatencyMs),
                    AvgLlmLatencyMs = g.Average(x => x.LlmLatencyMs),
                    AvgTotalLatencyMs = g.Average(x => x.TotalLatencyMs)
                })
                .ToListAsync();

            return new AnalyticsResponse<AvgLatencyDto>
            {
                Data = data,
                TotalCount = data.Count,
                StartDate = startDate,
                EndDate = endDate,
                PromptStyle = promptStyle,
                SlaStatus = slaStatus
            };
        }

        public async Task<AnalyticsResponse<SlaComplianceDto>> GetSlaComplianceAsync(
            DateTime? startDate, DateTime? endDate, string? slaStatus, string? promptStyle)
        {
            using var ctx = _sqlFactory.CreateDbContext();
            var q = ctx.RagComparisonHistories.AsQueryable();

            if (startDate.HasValue)
                q = q.Where(x => x.CreatedAt >= startDate.Value);

            if (endDate.HasValue)
                q = q.Where(x => x.CreatedAt <= endDate.Value);

            if (!string.IsNullOrEmpty(promptStyle))
                q = q.Where(x => x.PromptStyle == promptStyle);

            if (!string.IsNullOrEmpty(slaStatus))
            {
                if (slaStatus.Equals("ok", StringComparison.OrdinalIgnoreCase))
                    q = q.Where(x => x.IsWithinSla);
                else if (slaStatus.Equals("slow", StringComparison.OrdinalIgnoreCase))
                    q = q.Where(x => !x.IsWithinSla);
            }

            var data = await q
                .GroupBy(x => x.PromptStyle)
                .Select(g => new SlaComplianceDto
                {
                    PromptStyle = g.Key,
                    TotalRuns = g.Count(),
                    WithinSlaCount = g.Count(x => x.IsWithinSla),
                    // ✅ Explicit compliance rate calculation
                    ComplianceRate = g.Count() == 0 ? 0 : (g.Count(x => x.IsWithinSla) * 100.0 / g.Count())
                })
                .ToListAsync();

            return new AnalyticsResponse<SlaComplianceDto>
            {
                Data = data,
                TotalCount = data.Sum(x => x.TotalRuns),
                StartDate = startDate,
                EndDate = endDate,
                PromptStyle = promptStyle,
                SlaStatus = slaStatus
            };
        }


        public async Task<AnalyticsResponse<PromptStyleUsageDto>> GetPromptStyleUsageAsync(
            DateTime? startDate, DateTime? endDate, string? slaStatus, string? promptStyle)
        {
            using var ctx = _sqlFactory.CreateDbContext();
            var q = ctx.RagComparisonHistories.AsQueryable();

            if (startDate.HasValue)
                q = q.Where(x => x.CreatedAt >= startDate.Value);

            if (endDate.HasValue)
                q = q.Where(x => x.CreatedAt <= endDate.Value);

            if (!string.IsNullOrEmpty(promptStyle))
                q = q.Where(x => x.PromptStyle == promptStyle);

            if (!string.IsNullOrEmpty(slaStatus))
            {
                if (slaStatus.Equals("ok", StringComparison.OrdinalIgnoreCase))
                    q = q.Where(x => x.IsWithinSla);
                else if (slaStatus.Equals("slow", StringComparison.OrdinalIgnoreCase))
                    q = q.Where(x => !x.IsWithinSla);
            }

            var data = await q
                .GroupBy(x => x.PromptStyle)
                .Select(g => new PromptStyleUsageDto
                {
                    PromptStyle = g.Key,
                    Count = g.Count()
                })
                .ToListAsync();

            return new AnalyticsResponse<PromptStyleUsageDto>
            {
                Data = data,
                TotalCount = data.Sum(x => x.Count),
                StartDate = startDate,
                EndDate = endDate,
                PromptStyle = promptStyle,
                SlaStatus = slaStatus
            };
        }

        public async Task<AnalyticsResponse<TrendDto>> GetTrendsAsync(
            DateTime startDate, DateTime endDate, string? slaStatus, string? promptStyle)
        {
            using var ctx = _sqlFactory.CreateDbContext();
            var q = ctx.RagComparisonHistories
                .Where(x => x.CreatedAt >= startDate && x.CreatedAt <= endDate);

            if (!string.IsNullOrEmpty(promptStyle))
                q = q.Where(x => x.PromptStyle == promptStyle);

            if (!string.IsNullOrEmpty(slaStatus))
            {
                if (slaStatus.Equals("ok", StringComparison.OrdinalIgnoreCase))
                    q = q.Where(x => x.IsWithinSla);
                else if (slaStatus.Equals("slow", StringComparison.OrdinalIgnoreCase))
                    q = q.Where(x => !x.IsWithinSla);
            }

            var data = await q
                .GroupBy(x => x.CreatedAt.Date)
                .Select(g => new TrendDto
                {
                    Date = g.Key,
                    AvgTotalLatencyMs = g.Average(x => x.TotalLatencyMs)
                })
                .OrderBy(t => t.Date)
                .ToListAsync();

            return new AnalyticsResponse<TrendDto>
            {
                Data = data,
                TotalCount = data.Count,
                StartDate = startDate,
                EndDate = endDate,
                PromptStyle = promptStyle,
                SlaStatus = slaStatus
            };
        }

        public async Task<AnalyticsResponse<ProviderAnalyticsDto>> GetProviderAnalyticsAsync(
            DateTime? startDate = null, DateTime? endDate = null, string? promptStyle = null)
        {
            using var ctx = _sqlFactory.CreateDbContext();
            var q = ctx.RagComparisonHistories.AsQueryable();

            if (startDate.HasValue)
                q = q.Where(x => x.CreatedAt >= startDate.Value);

            if (endDate.HasValue)
                q = q.Where(x => x.CreatedAt <= endDate.Value);

            if (!string.IsNullOrEmpty(promptStyle))
                q = q.Where(x => x.PromptStyle == promptStyle);

            var data = await q
                .GroupBy(x => new { x.Provider, x.Model })
                .Select(g => new ProviderAnalyticsDto
                {
                    Provider = g.Key.Provider,
                    Model = g.Key.Model,
                    AvgRetrievalLatencyMs = g.Average(x => x.RetrievalLatencyMs),
                    AvgLlmLatencyMs = g.Average(x => x.LlmLatencyMs),
                    AvgTotalLatencyMs = g.Average(x => x.TotalLatencyMs),
                    TotalRuns = g.Count(),
                    WithinSlaCount = g.Count(x => x.IsWithinSla),
                    // ✅ force SLA compliance calculation during projection
                    SlaComplianceRate = g.Count() == 0 ? 0 : (g.Count(x => x.IsWithinSla) * 100.0 / g.Count())
                })
                .OrderByDescending(p => p.TotalRuns)
                .ToListAsync();

            return new AnalyticsResponse<ProviderAnalyticsDto>
            {
                Data = data,
                TotalCount = data.Sum(x => x.TotalRuns),
                StartDate = startDate,
                EndDate = endDate,
                PromptStyle = promptStyle
            };
        }

    }
}
