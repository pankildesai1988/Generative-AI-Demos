using ArNir.RAG.Interfaces;
using ArNir.RAG.Models;

namespace ArNir.RAG.Chunking;

/// <summary>
/// Splits a <see cref="RagDocument"/> into overlapping text chunks using a sliding window strategy.
/// Adjacent chunks share <c>overlap</c> characters to preserve context at boundaries.
/// When <see cref="RagDocument.Pages"/> is populated the chunker runs per-page so each chunk is
/// tagged with the originating <see cref="RagChunk.PageNumber"/>.
/// </summary>
public sealed class SlidingWindowChunker : IDocumentChunker
{
    /// <inheritdoc />
    /// <remarks>
    /// The sliding window advances by <c>chunkSize - overlap</c> characters on each step.
    /// Chunks that consist entirely of whitespace are skipped.
    /// Each chunk's <see cref="RagChunk.Metadata"/> contains <c>DocumentName</c>, <c>ChunkIndex</c>,
    /// and <c>PageNumber</c>.
    /// </remarks>
    public IReadOnlyList<RagChunk> Chunk(RagDocument document, int chunkSize = 500, int overlap = 50)
    {
        if (chunkSize <= 0) throw new ArgumentOutOfRangeException(nameof(chunkSize), "chunkSize must be > 0.");
        if (overlap < 0)    throw new ArgumentOutOfRangeException(nameof(overlap),    "overlap must be >= 0.");
        if (overlap >= chunkSize) throw new ArgumentOutOfRangeException(nameof(overlap), "overlap must be < chunkSize.");

        var chunks = new List<RagChunk>();
        var index  = 0;

        // Prefer per-page chunking when the parser supplied pages; otherwise fall back to
        // the concatenated Content with PageNumber = 1.
        var pages = document.Pages is { Count: > 0 }
            ? document.Pages
            : new List<RagPageContent> { new() { PageNumber = 1, Text = document.Content ?? string.Empty } };

        var step = chunkSize - overlap;

        foreach (var page in pages)
        {
            var pageText = page.Text ?? string.Empty;

            for (var start = 0; start < pageText.Length; start += step)
            {
                var length = Math.Min(chunkSize, pageText.Length - start);
                var text   = pageText.Substring(start, length);

                if (string.IsNullOrWhiteSpace(text))
                    continue;

                chunks.Add(new RagChunk
                {
                    DocumentId = document.Id,
                    ChunkIndex = index,
                    Text       = text,
                    TokenCount = EstimateTokenCount(text),
                    PageNumber = page.PageNumber,
                    Metadata   = new Dictionary<string, string>
                    {
                        ["DocumentName"] = document.FileName,
                        ["ChunkIndex"]   = index.ToString(),
                        ["PageNumber"]   = page.PageNumber.ToString()
                    }
                });

                index++;
            }
        }

        return chunks.AsReadOnly();
    }

    /// <summary>
    /// Provides a rough token-count estimate using the common 4-characters-per-token heuristic.
    /// </summary>
    /// <param name="text">The text to estimate.</param>
    /// <returns>An approximate token count.</returns>
    private static int EstimateTokenCount(string text)
        => (int)Math.Ceiling(text.Length / 4.0);
}
