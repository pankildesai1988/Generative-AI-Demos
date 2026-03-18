using ArNir.Core.Interfaces;
using ArNir.RAG.Pgvector;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace ArNir.Tests.Sprint1;

public class PgvectorDocumentEmbedderTests
{
    private readonly Mock<IEmbeddingProvider> _providerMock;
    private readonly PgvectorDocumentEmbedder _embedder;

    public PgvectorDocumentEmbedderTests()
    {
        _providerMock = new Mock<IEmbeddingProvider>();
        var loggerMock = new Mock<ILogger<PgvectorDocumentEmbedder>>();
        _embedder = new PgvectorDocumentEmbedder(_providerMock.Object, loggerMock.Object);
    }

    [Fact]
    public async Task GenerateAsync_CallsProviderAndReturnsVector()
    {
        // Arrange
        var expected = new float[] { 0.1f, 0.2f, 0.3f };
        _providerMock
            .Setup(p => p.GenerateEmbeddingAsync("hello", "text-embedding-ada-002"))
            .ReturnsAsync(expected);

        // Act
        var result = await _embedder.GenerateAsync("hello", "text-embedding-ada-002");

        // Assert
        Assert.Equal(expected, result);
        _providerMock.Verify(p => p.GenerateEmbeddingAsync("hello", "text-embedding-ada-002"), Times.Once);
    }

    [Fact]
    public async Task GenerateBatchAsync_CallsProviderForEachText()
    {
        // Arrange
        var texts = new[] { "alpha", "beta", "gamma" };
        _providerMock
            .Setup(p => p.GenerateEmbeddingAsync(It.IsAny<string>(), "model"))
            .ReturnsAsync((string text, string _) => new float[] { text.Length });

        // Act
        var result = await _embedder.GenerateBatchAsync(texts, "model");

        // Assert
        Assert.Equal(3, result.Count);
        Assert.Equal(new float[] { 5 }, result[0]); // "alpha".Length
        Assert.Equal(new float[] { 4 }, result[1]); // "beta".Length
        Assert.Equal(new float[] { 5 }, result[2]); // "gamma".Length
    }

    [Fact]
    public async Task GenerateBatchAsync_EmptyList_ReturnsEmptyList()
    {
        // Act
        var result = await _embedder.GenerateBatchAsync(Array.Empty<string>(), "model");

        // Assert
        Assert.Empty(result);
        _providerMock.Verify(p => p.GenerateEmbeddingAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }
}
