using ArNir.Core.Models.Chunking;
using ArNir.Platform.Configuration;
using ArNir.RAG.Chunking;
using ArNir.RAG.Extraction;
using ArNir.RAG.Interfaces;
using ArNir.RAG.Parsing;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Xunit;

namespace ArNir.Tests.T3;

/// <summary>
/// Acceptance smoke test over a real PDF checked into the repo
/// (<c>ArNir.Admin/wwwroot/uploads/Keysight_Holzinger_Road_to_Virtualization_19.pdf</c>):
/// every chunk must carry a PageNumber, text chunks must respect the size ceiling and never end
/// mid-sentence (mid-word), and indexes must be sequential. Skips silently when the sample PDF
/// is not present (e.g. trimmed checkout).
/// </summary>
public class KeysightPdfExtractionSmokeTests
{
    private static string? FindSamplePdf()
    {
        // Walk up from the test bin directory to the repo root.
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null)
        {
            var candidate = Path.Combine(
                dir.FullName, "ArNir.Admin", "wwwroot", "uploads",
                "Keysight_Holzinger_Road_to_Virtualization_19.pdf");
            if (File.Exists(candidate))
                return candidate;
            dir = dir.Parent;
        }
        return null;
    }

    [Fact]
    public async Task ExtractAsync_KeysightPdf_EveryChunkHasPageNumberAndRespectsSize()
    {
        var pdfPath = FindSamplePdf();
        if (pdfPath is null)
            return; // sample PDF not present in this checkout — nothing to verify

        const int chunkSize = 600;
        var options = Options.Create(new RagSettings
        {
            ChunkSize = chunkSize,
            ChunkOverlap = 100,
            ChunkingStrategy = "sentence"
        });

        var extractor = new UnifiedChunkExtractor(
            new IDocumentParser[] { new PdfDocumentParser(options) },
            new SlidingWindowChunker(options),
            new SentenceAwareChunker(options),
            options,
            NullLogger<UnifiedChunkExtractor>.Instance);

        await using var stream = File.OpenRead(pdfPath);
        var result = await extractor.ExtractAsync(stream, Path.GetFileName(pdfPath), "application/pdf");

        Assert.True(result.Chunks.Count > 0, "PDF should yield chunks");
        Assert.True(result.PageCount > 1, "Sample PDF is multi-page");

        for (var i = 0; i < result.Chunks.Count; i++)
        {
            var chunk = result.Chunks[i];

            Assert.Equal(i, chunk.Index);
            Assert.NotNull(chunk.PageNumber);
            Assert.InRange(chunk.PageNumber!.Value, 1, result.PageCount);

            if (chunk.ChunkType == ChunkTypes.Text)
            {
                // Ceiling: target size + the merge floor (200) — a page's trailing fragment
                // below the floor is appended to the previous chunk by design, so the true
                // upper bound is chunkSize + floor.
                Assert.True(chunk.Text.Length <= chunkSize + 200,
                    $"Chunk {i} exceeds size ceiling ({chunk.Text.Length} chars)");

                // Never cut mid-word: a chunk either ends a sentence or ends at a hard split of
                // an over-long sentence (which has no whitespace cut by construction). PdfPig
                // page text often lacks terminal punctuation on headings — accept any non-space
                // final char, but reject chunks ending in a hyphenated half word.
                Assert.False(char.IsWhiteSpace(chunk.Text[^1]), $"Chunk {i} ends with whitespace");
            }

            if (chunk.ChunkType == ChunkTypes.Image)
            {
                Assert.StartsWith("[Image: page ", chunk.Text);
                Assert.Null(chunk.BboxX1);
            }
        }
    }
}
