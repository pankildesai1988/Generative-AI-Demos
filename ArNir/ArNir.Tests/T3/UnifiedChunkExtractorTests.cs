using System.Text;
using ArNir.Core.Interfaces;
using ArNir.Core.Models.Chunking;
using ArNir.Platform.Configuration;
using ArNir.RAG.Chunking;
using ArNir.RAG.Extraction;
using ArNir.RAG.Interfaces;
using ArNir.RAG.Parsing;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace ArNir.Tests.T3;

/// <summary>
/// Unit tests for <see cref="UnifiedChunkExtractor"/> — the single chunk producer shared by the
/// SQL path (DocumentService) and the vector path (IngestionPipeline).
/// </summary>
public class UnifiedChunkExtractorTests
{
    private static UnifiedChunkExtractor CreateExtractor(
        RagSettings? settings = null, IChunkingOptionsResolver? resolver = null)
    {
        var options = Options.Create(settings ?? new RagSettings());
        return new UnifiedChunkExtractor(
            new IDocumentParser[] { new PlainTextDocumentParser() },
            new SlidingWindowChunker(options),
            new SentenceAwareChunker(options),
            options,
            NullLogger<UnifiedChunkExtractor>.Instance,
            resolver);
    }

    private static MemoryStream TextStream(string content)
        => new(Encoding.UTF8.GetBytes(content));

    private static IChunkingOptionsResolver FixedResolver(int size, int overlap, string strategy)
    {
        var mock = new Mock<IChunkingOptionsResolver>();
        mock.Setup(r => r.ResolveAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ChunkingOptions(size, overlap, strategy));
        return mock.Object;
    }

    [Fact]
    public async Task ExtractAsync_PlainText_TagsPageNumberAndTextType()
    {
        var extractor = CreateExtractor(new RagSettings { ChunkingStrategy = "sentence" });

        var result = await extractor.ExtractAsync(
            TextStream("First sentence here. Second sentence here."), "doc.txt", "text/plain");

        Assert.True(result.Chunks.Count >= 1);
        Assert.Equal(1, result.PageCount);
        Assert.All(result.Chunks, c =>
        {
            Assert.Equal(1, c.PageNumber);
            Assert.Equal(ChunkTypes.Text, c.ChunkType);
            Assert.Null(c.BboxX1);
        });
    }

    [Fact]
    public async Task ExtractAsync_UnsupportedContentType_Throws()
    {
        var extractor = CreateExtractor();

        await Assert.ThrowsAsync<NotSupportedException>(() =>
            extractor.ExtractAsync(TextStream("x"), "doc.bin", "application/octet-stream"));
    }

    [Fact]
    public async Task ExtractAsync_NullResolver_UsesRagSettings()
    {
        // ChunkSize 40 fits one ~28-char sentence per chunk but not two (mirrors the T2 test).
        var extractor = CreateExtractor(new RagSettings
        {
            ChunkSize = 40,
            ChunkOverlap = 0,
            MinChunkSize = 0,
            ChunkingStrategy = "sentence"
        });

        var result = await extractor.ExtractAsync(
            TextStream("Sentence number one is here. Sentence number two is here. Sentence number three is here."),
            "doc.txt", "text/plain");

        Assert.Equal(3, result.Chunks.Count);
    }

    [Fact]
    public async Task ExtractAsync_ResolverOptions_OverrideRagSettings()
    {
        // RagSettings says 600 chars (one chunk); the resolver says 40 (three chunks) — DB layer wins.
        var extractor = CreateExtractor(
            new RagSettings { ChunkSize = 600, ChunkOverlap = 0, MinChunkSize = 0, ChunkingStrategy = "sentence" },
            FixedResolver(40, 0, "sentence"));

        var result = await extractor.ExtractAsync(
            TextStream("Sentence number one is here. Sentence number two is here. Sentence number three is here."),
            "doc.txt", "text/plain");

        Assert.Equal(3, result.Chunks.Count);
    }

    [Fact]
    public async Task ExtractAsync_ResolverStrategy_SelectsSlidingChunker()
    {
        const string content = "One two three four. Five six seven eight. Nine ten eleven twelve.";

        var sentenceExtractor = CreateExtractor(resolver: FixedResolver(25, 0, "sentence"));
        var slidingExtractor  = CreateExtractor(resolver: FixedResolver(25, 0, "sliding"));

        var sentenceResult = await sentenceExtractor.ExtractAsync(TextStream(content), "doc.txt", "text/plain");
        var slidingResult  = await slidingExtractor.ExtractAsync(TextStream(content), "doc.txt", "text/plain");

        // Sentence strategy never cuts mid-sentence; the fixed window must cut at least once.
        Assert.All(sentenceResult.Chunks, c => Assert.EndsWith(".", c.Text));
        Assert.Contains(slidingResult.Chunks, c => !c.Text.TrimEnd().EndsWith("."));
    }

    [Fact]
    public async Task ExtractAsync_SameBytes_ProducesIdenticalSequences()
    {
        var extractor = CreateExtractor(new RagSettings { ChunkSize = 50, ChunkOverlap = 10, ChunkingStrategy = "sentence" });
        const string content = "Alpha sentence one here. Bravo sentence two here. Charlie sentence three here.";

        var first  = await extractor.ExtractAsync(TextStream(content), "doc.txt", "text/plain");
        var second = await extractor.ExtractAsync(TextStream(content), "doc.txt", "text/plain");

        Assert.Equal(first.Chunks.Count, second.Chunks.Count);
        for (var i = 0; i < first.Chunks.Count; i++)
        {
            Assert.Equal(first.Chunks[i].Index,      second.Chunks[i].Index);
            Assert.Equal(first.Chunks[i].Text,       second.Chunks[i].Text);
            Assert.Equal(first.Chunks[i].PageNumber, second.Chunks[i].PageNumber);
            Assert.Equal(first.Chunks[i].ChunkType,  second.Chunks[i].ChunkType);
        }
    }

    [Fact]
    public async Task ExtractAsync_AssignsSequentialIndexes()
    {
        var extractor = CreateExtractor(new RagSettings { ChunkSize = 40, ChunkOverlap = 0, MinChunkSize = 0, ChunkingStrategy = "sentence" });

        var result = await extractor.ExtractAsync(
            TextStream("One sentence here. Two sentence here. Three sentence here. Four sentence here."),
            "doc.txt", "text/plain");

        for (var i = 0; i < result.Chunks.Count; i++)
        {
            Assert.Equal(i, result.Chunks[i].Index);
        }
    }
}
