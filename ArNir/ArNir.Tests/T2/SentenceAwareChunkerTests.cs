using ArNir.Platform.Configuration;
using ArNir.RAG.Chunking;
using ArNir.RAG.DependencyInjection;
using ArNir.RAG.Interfaces;
using ArNir.RAG.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;

namespace ArNir.Tests.T2;

/// <summary>
/// Unit tests for <see cref="SentenceAwareChunker"/> (T2.1 — sentence-boundary-aware chunking)
/// and the <c>Rag:ChunkingStrategy</c> DI selection in
/// <see cref="ServiceCollectionExtensions.AddArNirRAG"/>.
/// </summary>
public class SentenceAwareChunkerTests
{
    private static SentenceAwareChunker CreateChunker(RagSettings? settings = null)
        => new(Options.Create(settings ?? new RagSettings()));

    private static RagDocument CreateDocument(string content)
        => new() { FileName = "test.txt", Content = content };

    [Fact]
    public void Chunk_MultipleSentences_NeverSplitsMidSentence()
    {
        var chunker = CreateChunker();
        var document = CreateDocument(
            "First sentence about revenue. Second sentence about growth. " +
            "Third sentence about margins. Fourth sentence about costs.");

        var chunks = chunker.Chunk(document, chunkSize: 60, overlap: 0);

        Assert.True(chunks.Count > 1);
        foreach (var chunk in chunks)
        {
            Assert.EndsWith(".", chunk.Text);
        }
    }

    [Fact]
    public void Chunk_SentenceLongerThanChunkSize_HardSplitsWithoutLosingContent()
    {
        var chunker = CreateChunker();
        var longSentence = new string('a', 250) + ".";
        var document = CreateDocument(longSentence);

        var chunks = chunker.Chunk(document, chunkSize: 100, overlap: 0);

        Assert.True(chunks.Count >= 3);
        var reassembled = string.Concat(chunks.Select(c => c.Text));
        Assert.Equal(longSentence.Length, reassembled.Length);
    }

    [Fact]
    public void Chunk_WithOverlap_CarriesTrailingSentenceIntoNextChunk()
    {
        var chunker = CreateChunker();
        var document = CreateDocument(
            "Alpha sentence one here. Bravo sentence two here. Charlie sentence three here.");

        var chunks = chunker.Chunk(document, chunkSize: 55, overlap: 30);

        Assert.True(chunks.Count > 1);
        // The last sentence of chunk N must reappear at the start of chunk N+1.
        for (var i = 1; i < chunks.Count; i++)
        {
            var previousLastSentence = chunks[i - 1].Text
                .Split(". ", StringSplitOptions.RemoveEmptyEntries)
                .Last()
                .TrimEnd('.');
            Assert.StartsWith(previousLastSentence, chunks[i].Text);
        }
    }

    [Fact]
    public void Chunk_EmptyContent_ReturnsNoChunks()
    {
        var chunker = CreateChunker();

        var chunks = chunker.Chunk(CreateDocument(string.Empty));

        Assert.Empty(chunks);
    }

    [Fact]
    public void Chunk_WhitespaceOnlyContent_ReturnsNoChunks()
    {
        var chunker = CreateChunker();

        var chunks = chunker.Chunk(CreateDocument("   \n\n   "));

        Assert.Empty(chunks);
    }

    [Fact]
    public void Chunk_PaginatedDocument_TagsChunksWithPageNumber()
    {
        var chunker = CreateChunker();
        var document = new RagDocument
        {
            FileName = "report.pdf",
            Pages =
            [
                new RagPageContent { PageNumber = 1, Text = "Page one sentence." },
                new RagPageContent { PageNumber = 2, Text = "Page two sentence." },
            ],
        };

        var chunks = chunker.Chunk(document, chunkSize: 500, overlap: 0);

        Assert.Equal(2, chunks.Count);
        Assert.Equal(1, chunks[0].PageNumber);
        Assert.Equal(2, chunks[1].PageNumber);
        Assert.Equal("1", chunks[0].Metadata["PageNumber"]);
        Assert.Equal("2", chunks[1].Metadata["PageNumber"]);
    }

    [Fact]
    public void Chunk_NoArguments_UsesRagSettingsDefaults()
    {
        var chunker = CreateChunker(new RagSettings { ChunkSize = 40, ChunkOverlap = 0 });
        var document = CreateDocument(
            "Sentence number one is here. Sentence number two is here. Sentence number three is here.");

        var chunks = chunker.Chunk(document);

        // ChunkSize 40 fits one ~28-char sentence per chunk but not two.
        Assert.Equal(3, chunks.Count);
    }

    [Theory]
    [InlineData(0, 0)]    // chunkSize must be > 0
    [InlineData(100, -1)] // overlap must be >= 0
    [InlineData(100, 100)] // overlap must be < chunkSize
    public void Chunk_InvalidArguments_Throws(int chunkSize, int overlap)
    {
        var chunker = CreateChunker();

        Assert.Throws<ArgumentOutOfRangeException>(
            () => chunker.Chunk(CreateDocument("Some text."), chunkSize, overlap));
    }

    [Fact]
    public void Chunk_AssignsSequentialChunkIndexes()
    {
        var chunker = CreateChunker();
        var document = CreateDocument(
            "One sentence here. Two sentence here. Three sentence here. Four sentence here.");

        var chunks = chunker.Chunk(document, chunkSize: 40, overlap: 0);

        for (var i = 0; i < chunks.Count; i++)
        {
            Assert.Equal(i, chunks[i].ChunkIndex);
        }
    }

    [Theory]
    [InlineData("sentence", typeof(SentenceAwareChunker))]
    [InlineData("Sentence", typeof(SentenceAwareChunker))] // case-insensitive
    [InlineData("sliding", typeof(SlidingWindowChunker))]
    [InlineData("unknown-strategy", typeof(SlidingWindowChunker))] // unknown → sliding fallback
    public void AddArNirRAG_ChunkingStrategy_SelectsExpectedChunker(string strategy, Type expectedType)
    {
        var services = new ServiceCollection();
        services.AddOptions();
        services.Configure<RagSettings>(options => options.ChunkingStrategy = strategy);
        services.AddArNirRAG();

        using var provider = services.BuildServiceProvider();
        var chunker = provider.GetRequiredService<IDocumentChunker>();

        Assert.IsType(expectedType, chunker);
    }
}
