using ArNir.Platform.Configuration;
using ArNir.RAG.Interfaces;
using ArNir.RAG.Models;
using Microsoft.Extensions.Options;

namespace ArNir.RAG.Chunking;

/// <summary>
/// Splits a <see cref="RagDocument"/> into overlapping text chunks using a sliding window strategy.
/// Adjacent chunks share <c>overlap</c> characters to preserve context at boundaries.
/// When <see cref="RagDocument.Pages"/> is populated the chunker runs per-page so each chunk is
/// tagged with the originating <see cref="RagChunk.PageNumber"/>.
/// <para>
/// Chunk size/overlap default to the <c>Rag</c> appsettings section (<see cref="RagSettings"/>),
/// tunable on a deployed environment without recompiling; explicit method arguments override.
/// </para>
/// </summary>
public sealed class SlidingWindowChunker : IDocumentChunker
{
    private readonly RagSettings _settings;

    /// <summary>Initialises the chunker with appsettings-bound <see cref="RagSettings"/>.</summary>
    /// <param name="settings">The bound RAG options; when unconfigured, property defaults apply.</param>
    public SlidingWindowChunker(IOptions<RagSettings> settings)
        => _settings = settings.Value;

    /// <inheritdoc />
    /// <remarks>
    /// The sliding window advances by <c>chunkSize - overlap</c> characters on each step.
    /// Chunks that consist entirely of whitespace are skipped.
    /// Each chunk's <see cref="RagChunk.Metadata"/> contains <c>DocumentName</c>, <c>ChunkIndex</c>,
    /// and <c>PageNumber</c>.
    /// </remarks>
    public IReadOnlyList<RagChunk> Chunk(RagDocument document, int? chunkSize = null, int? overlap = null)
    {
        var effectiveChunkSize = chunkSize ?? _settings.ChunkSize;
        var effectiveOverlap   = overlap   ?? _settings.ChunkOverlap;

        if (effectiveChunkSize <= 0) throw new ArgumentOutOfRangeException(nameof(chunkSize), "chunkSize must be > 0.");
        if (effectiveOverlap < 0)    throw new ArgumentOutOfRangeException(nameof(overlap),    "overlap must be >= 0.");
        if (effectiveOverlap >= effectiveChunkSize) throw new ArgumentOutOfRangeException(nameof(overlap), "overlap must be < chunkSize.");

        var chunks = new List<RagChunk>();
        var index  = 0;

        // Prefer per-page chunking when the parser supplied pages; otherwise fall back to
        // the concatenated Content with PageNumber = 1.
        var pages = document.Pages is { Count: > 0 }
            ? document.Pages
            : new List<RagPageContent> { new() { PageNumber = 1, Text = document.Content ?? string.Empty } };

        var step = effectiveChunkSize - effectiveOverlap;

        foreach (var page in pages)
        {
            var pageText = page.Text ?? string.Empty;

            for (var start = 0; start < pageText.Length; start += step)
            {
                var length = Math.Min(effectiveChunkSize, pageText.Length - start);
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
