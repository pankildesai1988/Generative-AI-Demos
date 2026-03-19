using ArNir.Observability.Models;
using ArNir.Services;
using ArNir.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace ArNir.Tests.Sprint6;

/// <summary>
/// Unit tests for <see cref="LlmEvaluationService"/> — the LLM-as-judge scorer.
/// </summary>
public class LlmEvaluationServiceTests
{
    private readonly Mock<ILlmService> _llmMock = new();
    private readonly Mock<ILogger<LlmEvaluationService>> _loggerMock = new();

    private LlmEvaluationService CreateSut() => new(_llmMock.Object, _loggerMock.Object);

    [Fact]
    public async Task EvaluateAsync_ValidJson_ReturnsScores()
    {
        _llmMock
            .Setup(x => x.GetCompletionAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync("{\"relevance\": 0.85, \"faithfulness\": 0.92, \"reasoning\": \"Good answer\"}");

        var result = await CreateSut().EvaluateAsync("What is AI?", "AI is...", "Context about AI");

        Assert.Equal(0.85, result.RelevanceScore);
        Assert.Equal(0.92, result.FaithfulnessScore);
        Assert.Equal("Good answer", result.Reasoning);
    }

    [Fact]
    public async Task EvaluateAsync_MarkdownWrappedJson_ParsesCorrectly()
    {
        _llmMock
            .Setup(x => x.GetCompletionAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync("```json\n{\"relevance\": 0.7, \"faithfulness\": 0.6, \"reasoning\": \"OK\"}\n```");

        var result = await CreateSut().EvaluateAsync("Q", "A", "C");

        Assert.Equal(0.7, result.RelevanceScore);
        Assert.Equal(0.6, result.FaithfulnessScore);
    }

    [Fact]
    public async Task EvaluateAsync_OutOfRangeScores_ClampedTo01()
    {
        _llmMock
            .Setup(x => x.GetCompletionAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync("{\"relevance\": 1.5, \"faithfulness\": -0.3, \"reasoning\": \"Clamped\"}");

        var result = await CreateSut().EvaluateAsync("Q", "A", "C");

        Assert.Equal(1.0, result.RelevanceScore);
        Assert.Equal(0.0, result.FaithfulnessScore);
    }

    [Fact]
    public async Task EvaluateAsync_InvalidJson_ReturnsZeroScores()
    {
        _llmMock
            .Setup(x => x.GetCompletionAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync("This is not valid JSON at all!");

        var result = await CreateSut().EvaluateAsync("Q", "A", "C");

        Assert.Equal(0.0, result.RelevanceScore);
        Assert.Equal(0.0, result.FaithfulnessScore);
        Assert.Contains("Parse error", result.Reasoning);
    }

    [Fact]
    public async Task EvaluateAsync_LlmThrowsException_ReturnsZeroWithErrorMessage()
    {
        _llmMock
            .Setup(x => x.GetCompletionAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ThrowsAsync(new HttpRequestException("Service unavailable"));

        var result = await CreateSut().EvaluateAsync("Q", "A", "C");

        Assert.Equal(0.0, result.RelevanceScore);
        Assert.Equal(0.0, result.FaithfulnessScore);
        Assert.Contains("Evaluation failed", result.Reasoning);
    }

    [Fact]
    public async Task EvaluateAsync_MissingFields_DefaultsToZero()
    {
        _llmMock
            .Setup(x => x.GetCompletionAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync("{\"reasoning\": \"Only reasoning provided\"}");

        var result = await CreateSut().EvaluateAsync("Q", "A", "C");

        Assert.Equal(0.0, result.RelevanceScore);
        Assert.Equal(0.0, result.FaithfulnessScore);
        Assert.Equal("Only reasoning provided", result.Reasoning);
    }
}
