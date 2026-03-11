using ArNir.Core.DTOs.Chat;
using ArNir.Core.DTOs.Analytics;
using ArNir.Core.Entities;
using ArNir.Data;
using ArNir.Services.AI.Interfaces;
using ArNir.Services.Helper;
using ArNir.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace ArNir.Services.AI
{
    public class ChatInsightService : IChatInsightService
    {
        private readonly ArNirDbContext _sqlContext;
        private readonly VectorDbContext _pgContext;
        private readonly INaturalQueryService _queryService;
        private readonly IVisualizationService _visualService;
        private readonly IInsightEngineService _insightEngine;
        private readonly IChatEmbeddingService _chatEmbeddingService;
        private readonly IEmbeddingService _embeddingService;
        private readonly IActionEngineService _actionEngineService;
        private readonly ILogger<ChatInsightService> _logger;

        public ChatInsightService(
            ArNirDbContext sqlContext,
            VectorDbContext pgContext,
            INaturalQueryService queryService,
            IVisualizationService visualService,
            IInsightEngineService insightEngine,
            IChatEmbeddingService chatEmbeddingService,
            IEmbeddingService embeddingService,
            IActionEngineService actionEngineService,
            ILogger<ChatInsightService> logger)
        {
            _sqlContext = sqlContext;
            _pgContext = pgContext;
            _queryService = queryService;
            _visualService = visualService;
            _insightEngine = insightEngine;
            _chatEmbeddingService = chatEmbeddingService;
            _embeddingService = embeddingService;
            _actionEngineService = actionEngineService;
            _logger = logger;
        }

        public async Task<ChatResponseDto> ProcessUserQueryAsync(ChatQueryDto query)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(query.UserQuery))
                {
                    return new ChatResponseDto
                    {
                        ResponseText = "Prompt cannot be empty.",
                        IsError = true,
                        SuggestedActions = new[] { "Retry", "Contact Support" }
                    };
                }

                // 1️⃣ Save user query to memory (SQL Server)
                var memory = new ChatMemory
                {
                    SessionId = query.SessionId,
                    UserMessage = query.UserQuery,
                    CreatedAt = DateTime.UtcNow
                };
                _sqlContext.ChatMemories.Add(memory);
                await _sqlContext.SaveChangesAsync();

                // 2️⃣ Generate embedding for chat message (PostgreSQL)
                await _chatEmbeddingService.GenerateEmbeddingForMessageAsync(memory.Id, query.UserQuery, "text-embedding-3-small");

                // 3️⃣ Generate query embedding for recall
                var queryEmbedding = await _embeddingService.GenerateForQueryAsync(query.UserQuery, "text-embedding-3-small");

                // 4️⃣ Retrieve related chat memory context
                var similar = await _chatEmbeddingService.FindSimilarAsync(queryEmbedding, 1);
                string contextText = "";
                if (similar != null)
                {
                    var related = await _sqlContext.ChatMemories.FirstOrDefaultAsync(x => x.Id == similar.ChatMemoryId);
                    if (related != null && related.Id != memory.Id)
                        contextText = $"Previous related message: {related.UserMessage}\n";
                }

                // 5️⃣ Create enriched GPT prompt
                var enrichedPrompt = $"{contextText}User: {query.UserQuery}\nGenerate analytical insight with metrics if available.";

                // 6️⃣ Generate AI insight using GenerateSummaryAsync
                var insightText = await _insightEngine.GenerateSummaryAsync(
                    provider: null,
                    startDate: null,
                    endDate: null,
                    userPrompt: enrichedPrompt
                );

                // 7️⃣ Optional: Build analytics data + visualization
                var analyticsData = await _queryService.TranslateQueryAsync(query.UserQuery);
                var analyticsChart = await _visualService.BuildChartDto(analyticsData);

                // 🔧 Normalize chart if numeric data exists
                if (analyticsChart != null && analyticsChart.Data != null)
                {
                    bool hasNumeric = analyticsChart.Data.Any(d => d.Value > 0);
                    if (hasNumeric && analyticsChart.Type == "text")
                        analyticsChart.Type = analyticsChart.Data.Count > 3 ? "bar" : "line";
                }

                // 8️⃣ Detect contextual actions
                var detectedActions = ActionParser.ExtractActions(insightText ?? string.Empty);

                // 9️⃣ Save assistant reply
                memory.AssistantMessage = insightText;
                _sqlContext.ChatMemories.Update(memory);
                await _sqlContext.SaveChangesAsync();

                // 🔟 Return structured response
                return new ChatResponseDto
                {
                    ResponseText = insightText ?? "No insights generated.",
                    Chart = analyticsChart,
                    InsightSummary = "Insight generated using context memory and analytics services.",
                    SuggestedActions = detectedActions.Any()
                        ? detectedActions.Select(a =>
                            a switch
                            {
                                "compare_models" => "Compare Models",
                                "view_trends" => "View Trends",
                                "sla_summary" => "SLA Summary",
                                _ => a
                            }).ToArray()
                        : new[] { "Export Report", "View Dashboard" },
                    IsError = false
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while processing user query.");
                return new ChatResponseDto
                {
                    ResponseText = "⚠️ Failed to process your request.",
                    InsightSummary = "Error occurred while generating insights.",
                    SuggestedActions = new[] { "Retry", "Contact Support" },
                    IsError = true
                };
            }
        }

        public async Task<object> GetSessionContextAsync(string sessionId)
        {
            var messages = await _sqlContext.ChatMemories
                .Where(x => x.SessionId == sessionId)
                .OrderByDescending(x => x.CreatedAt)
                .Select(x => new
                {
                    x.Id,
                    x.UserMessage,
                    x.AssistantMessage,
                    x.CreatedAt
                })
                .ToListAsync();

            return new { SessionId = sessionId, Messages = messages };
        }

        public async Task<object?> ExecuteActionAsync(string action)
        {
            _logger.LogInformation("Executing contextual action: {Action}", action);
            return await _actionEngineService.ExecuteActionAsync(action);
        }
    }
}
