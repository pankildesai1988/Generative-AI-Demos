using ArNir.Platform.Configuration;
using ArNir.RAG.Chunking;
using ArNir.RAG.Models;
using Microsoft.Extensions.Options;
using Xunit;

namespace ArNir.Tests.T3;

/// <summary>
/// Tests for the <see cref="SentenceAwareChunker"/> size floor: a page's trailing chunk whose
/// fresh content is shorter than <c>min(MinChunkSize, chunkSize / 3)</c> merges into the page's
/// previous chunk so fragment chunks don't pollute the embedding space.
/// </summary>
public class SentenceAwareChunkerFloorTests
{
    private static SentenceAwareChunker CreateChunker(int minChunkSize = 200)
        => new(Options.Create(new RagSettings { MinChunkSize = minChunkSize }));

    private static RagDocument CreateDocument(string content)
        => new() { FileName = "test.txt", Content = content };

    [Fact]
    public void Chunk_TrailingFragment_MergesIntoPreviousChunk()
    {
        var chunker = CreateChunker();
        var longSentence = new string('a', 89) + ".";
        var document = CreateDocument(longSentence + " Tiny tail.");

        // chunkSize 100 → floor = min(200, 33) = 33; "Tiny tail." (10 chars) < 33 → merged.
        var chunks = chunker.Chunk(document, chunkSize: 100, overlap: 0);

        Assert.Single(chunks);
        Assert.EndsWith("Tiny tail.", chunks[0].Text);
        Assert.Contains(longSentence, chunks[0].Text);
    }

    [Fact]
    public void Chunk_TrailingChunkAboveFloor_IsNotMerged()
    {
        var chunker = CreateChunker();
        var longSentence = new string('a', 89) + ".";
        var tail = "This trailing sentence is clearly long enough."; // 47 chars > floor 33
        var document = CreateDocument(longSentence + " " + tail);

        var chunks = chunker.Chunk(document, chunkSize: 100, overlap: 0);

        Assert.Equal(2, chunks.Count);
        Assert.Equal(tail, chunks[1].Text);
    }

    [Fact]
    public void Chunk_FloorClampsAtThirdOfChunkSize()
    {
        // MinChunkSize 200 but chunkSize 60 → effective floor = 20, not 200.
        var chunker = CreateChunker(minChunkSize: 200);
        var first = new string('b', 49) + "."; // 50 chars → own chunk
        var tail = "Twentyfive char sentence."; // 25 chars ≥ 20 → must NOT merge
        var document = CreateDocument(first + " " + tail);

        var chunks = chunker.Chunk(document, chunkSize: 60, overlap: 0);

        Assert.Equal(2, chunks.Count);
    }

    [Fact]
    public void Chunk_SingleChunkPage_NeverMerges()
    {
        var chunker = CreateChunker();
        var document = new RagDocument
        {
            FileName = "report.pdf",
            Pages =
            [
                new RagPageContent { PageNumber = 1, Text = "Page one tiny." },
                new RagPageContent { PageNumber = 2, Text = "Page two tiny." },
            ],
        };

        // Each page yields a single small chunk; no previous chunk exists on the same page,
        // so nothing merges (and page 2 must never merge into page 1).
        var chunks = chunker.Chunk(document, chunkSize: 600, overlap: 0);

        Assert.Equal(2, chunks.Count);
        Assert.Equal(1, chunks[0].PageNumber);
        Assert.Equal(2, chunks[1].PageNumber);
    }

    [Fact]
    public void Chunk_MergedFragment_DoesNotDuplicateOverlapSentences()
    {
        var chunker = CreateChunker();
        var s1 = new string('c', 69) + "."; // 70 chars
        var s2 = "Overlap carrier sentence here padding pad."; // 42 chars
        var s3 = "Tail."; // 5 chars — fresh fragment below floor
        var document = CreateDocument($"{s1} {s2} {s3}");

        // chunkSize 115, overlap 50: chunk1 = [s1, s2] (113 chars); s3 overflows → chunk2 seeded
        // with carried [s2] + fresh [s3]. floor = min(200, 38) = 38 → fresh "Tail." merges, but
        // ONLY the fresh part is appended — s2 must appear exactly once in the merged chunk.
        var chunks = chunker.Chunk(document, chunkSize: 115, overlap: 50);

        Assert.Single(chunks);
        var occurrences = chunks[0].Text.Split(s2).Length - 1;
        Assert.Equal(1, occurrences);
        Assert.EndsWith(s3, chunks[0].Text);
    }
}
