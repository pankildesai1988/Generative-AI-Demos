using ArNir.Observability.Interfaces;
using ArNir.Observability.Models;
using ArNir.Services.Interfaces;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace ArNir.Services;

/// <summary>
/// LLM-as-judge implementation of <see cref="IEvaluationService"/>.
/// Sends a structured prompt to an LLM requesting JSON-formatted Relevance
/// and Faithfulness scores, then parses the response.
/// </summary>
public class LlmEvaluationService : IEvaluationService
{
    private readonly ILlmService _llmService;
    private readonly ILogger<LlmEvaluationService> _logger;

    private const string EvalModel = "gpt-4o-mini";

    private const string EvalPromptTemplate = @"You are an AI evaluation judge. Evaluate the following answer for:
1. RELEVANCE (0.0-1.0): Does the answer address the question asked?
2. FAITHFULNESS (0.0-1.0): Is the answer grounded in the provided context without hallucination?

Question: {0}
Context: {1}
Answer: {2}

Respond ONLY with valid JSON (no markdown, no explanation):
{{""relevance"": 0.0, ""faithfulness"": 0.0, ""reasoning"": ""...""}}";

    /// <summary>Initialises the evaluation service with an LLM provider.</summary>
    public LlmEvaluationService(ILlmService llmService, ILogger<LlmEvaluationService> logger)
    {
        _llmService = llmService;
        _logger     = logger;
    }

    /// <inheritdoc />
    public async Task<EvaluationResult> EvaluateAsync(
        string question, string answer, string context, CancellationToken ct = default)
    {
        var prompt = string.Format(EvalPromptTemplate,
            Truncate(question, 1500),
            Truncate(context, 2000),
            Truncate(answer, 1500));

        try
        {
            var response = await _llmService.GetCompletionAsync(prompt, EvalModel);
            return ParseEvaluationResponse(response);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "LLM evaluation call failed; returning zero scores.");
            return new EvaluationResult
            {
                RelevanceScore    = 0.0,
                FaithfulnessScore = 0.0,
                Reasoning         = $"Evaluation failed: {ex.Message}",
                EvaluatedAt       = DateTime.UtcNow
            };
        }
    }

    private EvaluationResult ParseEvaluationResponse(string response)
    {
        try
        {
            // Strip markdown code fences if present
            var json = response.Trim();
            if (json.StartsWith("```"))
            {
                var startIdx = json.IndexOf('{');
                var endIdx   = json.LastIndexOf('}');
                if (startIdx >= 0 && endIdx > startIdx)
                    json = json[startIdx..(endIdx + 1)];
            }

            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            var relevance    = root.TryGetProperty("relevance", out var r)    ? r.GetDouble() : 0.0;
            var faithfulness = root.TryGetProperty("faithfulness", out var f) ? f.GetDouble() : 0.0;
            var reasoning    = root.TryGetProperty("reasoning", out var rsn)  ? rsn.GetString() ?? "" : "";

            return new EvaluationResult
            {
                RelevanceScore    = Clamp(relevance),
                FaithfulnessScore = Clamp(faithfulness),
                Reasoning         = reasoning,
                EvaluatedAt       = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to parse evaluation JSON: {Response}", Truncate(response, 200));
            return new EvaluationResult
            {
                RelevanceScore    = 0.0,
                FaithfulnessScore = 0.0,
                Reasoning         = $"Parse error: {ex.Message}. Raw: {Truncate(response, 100)}",
                EvaluatedAt       = DateTime.UtcNow
            };
        }
    }

    private static double Clamp(double value) => Math.Max(0.0, Math.Min(1.0, value));
    private static string Truncate(string s, int maxLen) =>
        s.Length <= maxLen ? s : s[..maxLen] + "...";
}
