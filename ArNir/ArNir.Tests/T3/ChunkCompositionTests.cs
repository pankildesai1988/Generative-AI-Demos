using ArNir.Core.Models.Chunking;
using ArNir.Platform.Configuration;
using ArNir.RAG.Chunking;
using ArNir.RAG.Extraction;
using ArNir.RAG.Models;
using Microsoft.Extensions.Options;
using Xunit;

namespace ArNir.Tests.T3;

/// <summary>
/// Tests for <see cref="UnifiedChunkExtractor.ComposeChunks"/> and
/// <see cref="UnifiedChunkExtractor.BuildRowSentence"/> — table row-to-sentence conversion,
/// image stubs, per-page ordering, and global sequential indexing.
/// </summary>
public class ChunkCompositionTests
{
    private static RagTable SpecTable() => new()
    {
        Headers = ["Model", "Frequency Range", "RBW"],
        Rows =
        [
            ["N9020B", "10Hz-26.5GHz", "1Hz"],
            ["N9030B", "2Hz-50GHz", "1Hz"],
        ],
        X1 = 50f, Y1 = 400f, X2 = 500f, Y2 = 600f
    };

    private static IReadOnlyList<RagChunk> ChunkText(RagDocument document, int chunkSize)
        => new SentenceAwareChunker(Options.Create(new RagSettings { MinChunkSize = 0 }))
            .Chunk(document, chunkSize, 0);

    [Fact]
    public void BuildRowSentence_ContainsHeaderTermsAndRowValues()
    {
        var sentence = UnifiedChunkExtractor.BuildRowSentence(
            ["Model", "Frequency Range", "RBW"],
            ["N9020B", "10Hz-26.5GHz", "1Hz"]);

        Assert.Equal("The N9020B has Frequency Range 10Hz-26.5GHz, RBW 1Hz.", sentence);
    }

    [Fact]
    public void BuildRowSentence_NonTextualHeaders_FallsBackToKeyValuePairs()
    {
        var sentence = UnifiedChunkExtractor.BuildRowSentence(
            ["2023", "2024"],
            ["100", "200"]);

        Assert.Equal("2023: 100; 2024: 200.", sentence);
    }

    [Fact]
    public void BuildRowSentence_EmptyRow_ReturnsEmpty()
    {
        Assert.Equal(string.Empty,
            UnifiedChunkExtractor.BuildRowSentence(["A", "B"], ["", "  "]));
    }

    [Fact]
    public void BuildRowSentence_KeyValueTable_RendersKeyColonValue()
    {
        // Empty headers ⇒ key/value table (the Mercedes slide case).
        var sentence = UnifiedChunkExtractor.BuildRowSentence(
            [],
            ["Interfaces", "FPD link / GMSL / Ethernet"]);

        Assert.Equal("Interfaces: FPD link / GMSL / Ethernet.", sentence);
    }

    [Fact]
    public void ComposeChunks_KeyValueTable_EmitsKeyValueSentences()
    {
        var document = new RagDocument
        {
            FileName = "spec.pdf",
            Pages =
            [
                new RagPageContent
                {
                    PageNumber = 3,
                    Text = string.Empty,
                    Tables =
                    [
                        new RagTable
                        {
                            Headers = new List<string>(), // key/value table
                            Rows =
                            [
                                ["Driving Cams", "8 MP is Standard"],
                                ["Interfaces", "FPD link / GMSL / Ethernet"],
                            ],
                            X1 = 50f, Y1 = 200f, X2 = 600f, Y2 = 420f
                        }
                    ]
                }
            ]
        };

        var chunks = UnifiedChunkExtractor.ComposeChunks(document, ChunkText(document, 600), 600);

        var tableChunk = Assert.Single(chunks, c => c.ChunkType == ChunkTypes.Table);
        Assert.Contains("Interfaces: FPD link / GMSL / Ethernet.", tableChunk.Text);
        Assert.Contains("Driving Cams: 8 MP is Standard.", tableChunk.Text);
    }

    [Fact]
    public void ComposeChunks_TableChunk_CarriesBboxTypeAndPageNumber()
    {
        var document = new RagDocument
        {
            FileName = "spec.pdf",
            Pages = [new RagPageContent { PageNumber = 3, Text = "Intro sentence.", Tables = [SpecTable()] }]
        };

        var chunks = UnifiedChunkExtractor.ComposeChunks(document, ChunkText(document, 600), 600);

        var tableChunks = chunks.Where(c => c.ChunkType == ChunkTypes.Table).ToList();
        Assert.NotEmpty(tableChunks);
        Assert.All(tableChunks, c =>
        {
            Assert.Equal(3, c.PageNumber);
            Assert.Equal(50f, c.BboxX1);
            Assert.Equal(600f, c.BboxY2);
        });
        Assert.Contains(tableChunks, c => c.Text.Contains("N9020B") && c.Text.Contains("Frequency Range"));
    }

    [Fact]
    public void ComposeChunks_ShortRows_PackIntoOneChunkWithinSize()
    {
        var document = new RagDocument
        {
            FileName = "spec.pdf",
            Pages = [new RagPageContent { PageNumber = 1, Text = string.Empty, Tables = [SpecTable()] }]
        };

        var chunks = UnifiedChunkExtractor.ComposeChunks(document, ChunkText(document, 600), 600);

        // Both ~55-char row sentences fit one 600-char chunk.
        var tableChunk = Assert.Single(chunks, c => c.ChunkType == ChunkTypes.Table);
        Assert.Contains("N9020B", tableChunk.Text);
        Assert.Contains("N9030B", tableChunk.Text);
        Assert.True(tableChunk.Text.Length <= 600);
    }

    [Fact]
    public void ComposeChunks_LongRows_SplitAcrossChunks()
    {
        var document = new RagDocument
        {
            FileName = "spec.pdf",
            Pages = [new RagPageContent { PageNumber = 1, Text = string.Empty, Tables = [SpecTable()] }]
        };

        // Chunk size smaller than two row sentences → one chunk per row.
        var chunks = UnifiedChunkExtractor.ComposeChunks(document, ChunkText(document, 60), 60);

        var tableChunks = chunks.Where(c => c.ChunkType == ChunkTypes.Table).ToList();
        Assert.Equal(2, tableChunks.Count);
    }

    [Fact]
    public void ComposeChunks_ImageStub_HasPlaceholderTextAndNullBbox()
    {
        var document = new RagDocument
        {
            FileName = "scan.pdf",
            Pages = [new RagPageContent { PageNumber = 2, Text = "Caption text here.", Images = [new RagImageRef { Index = 1 }] }]
        };

        var chunks = UnifiedChunkExtractor.ComposeChunks(document, ChunkText(document, 600), 600);

        var imageChunk = Assert.Single(chunks, c => c.ChunkType == ChunkTypes.Image);
        Assert.Equal("[Image: page 2, image 1]", imageChunk.Text);
        Assert.Equal(2, imageChunk.PageNumber);
        Assert.Null(imageChunk.BboxX1);
        Assert.Null(imageChunk.BboxY2);
    }

    [Fact]
    public void ComposeChunks_PerPageOrder_TextThenTableThenImage_WithSequentialIndexes()
    {
        var document = new RagDocument
        {
            FileName = "spec.pdf",
            Pages =
            [
                new RagPageContent
                {
                    PageNumber = 1,
                    Text = "Page one prose sentence.",
                    Tables = [SpecTable()],
                    Images = [new RagImageRef { Index = 1 }]
                },
                new RagPageContent { PageNumber = 2, Text = "Page two prose sentence." }
            ]
        };

        var chunks = UnifiedChunkExtractor.ComposeChunks(document, ChunkText(document, 600), 600);

        var page1Types = chunks.Where(c => c.PageNumber == 1).Select(c => c.ChunkType).ToList();
        Assert.Equal(new[] { ChunkTypes.Text, ChunkTypes.Table, ChunkTypes.Image }, page1Types);

        // Page 2 text comes after all of page 1's chunks.
        Assert.Equal(ChunkTypes.Text, chunks[^1].ChunkType);
        Assert.Equal(2, chunks[^1].PageNumber);

        for (var i = 0; i < chunks.Count; i++)
        {
            Assert.Equal(i, chunks[i].Index);
        }
    }
}
