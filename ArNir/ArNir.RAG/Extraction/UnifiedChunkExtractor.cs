using ArNir.Core.Interfaces;
using ArNir.Core.Models.Chunking;
using ArNir.Platform.Configuration;
using ArNir.Platform.Constants;
using ArNir.RAG.Chunking;
using ArNir.RAG.Interfaces;
using ArNir.RAG.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ArNir.RAG.Extraction;

/// <summary>
/// The single authoritative chunk producer shared by both ingestion paths
/// (see <see cref="IUnifiedChunkExtractor"/>). Parses the file with the registered
/// <see cref="IDocumentParser"/>s, resolves the effective chunking options per call
/// (DB layer via <see cref="IChunkingOptionsResolver"/> when wired, otherwise the appsettings-bound
/// <see cref="RagSettings"/>), picks the chunking strategy per call, and composes the final
/// page-ordered, sequentially indexed chunk sequence.
/// <para>
/// Determinism contract: the same bytes with the same resolved options always yield an
/// element-wise identical sequence — this is what keeps <c>DocumentChunk.ChunkOrder</c> (SQL path)
/// and the embedding key <c>"sql:{docId}:{index}"</c> (vector path) referring to identical text.
/// </para>
/// </summary>
public sealed class UnifiedChunkExtractor : IUnifiedChunkExtractor
{
    private readonly IEnumerable<IDocumentParser>   _parsers;
    private readonly SlidingWindowChunker           _slidingChunker;
    private readonly SentenceAwareChunker           _sentenceChunker;
    private readonly RagSettings                    _settings;
    private readonly ILogger<UnifiedChunkExtractor> _logger;
    private readonly IChunkingOptionsResolver?      _optionsResolver;

    /// <summary>
    /// Initialises a new instance of <see cref="UnifiedChunkExtractor"/>.
    /// </summary>
    /// <param name="parsers">All registered <see cref="IDocumentParser"/> implementations.</param>
    /// <param name="slidingChunker">The fixed-size sliding-window chunker.</param>
    /// <param name="sentenceChunker">The sentence-boundary-aware chunker.</param>
    /// <param name="settings">Appsettings-bound RAG options (fallback layer when no resolver is wired).</param>
    /// <param name="logger">Logger for diagnostic output.</param>
    /// <param name="optionsResolver">
    /// Optional DB-layer options resolver; registered by the composition roots (Admin/API) so
    /// PlatformSettings values — including the strategy — apply per call without restart.
    /// </param>
    public UnifiedChunkExtractor(
        IEnumerable<IDocumentParser>   parsers,
        SlidingWindowChunker           slidingChunker,
        SentenceAwareChunker           sentenceChunker,
        IOptions<RagSettings>          settings,
        ILogger<UnifiedChunkExtractor> logger,
        IChunkingOptionsResolver?      optionsResolver = null)
    {
        _parsers         = parsers;
        _slidingChunker  = slidingChunker;
        _sentenceChunker = sentenceChunker;
        _settings        = settings.Value;
        _logger          = logger;
        _optionsResolver = optionsResolver;
    }

    /// <inheritdoc />
    public async Task<ChunkExtractionResult> ExtractAsync(
        Stream content, string fileName, string contentType, CancellationToken ct = default)
    {
        var parser = _parsers.FirstOrDefault(p => p.CanParse(contentType))
            ?? throw new NotSupportedException($"No parser registered for content type '{contentType}'.");

        var document = await parser.ParseAsync(content, fileName, contentType);

        var options = _optionsResolver is not null
            ? await _optionsResolver.ResolveAsync(ct)
            : new ChunkingOptions(_settings.ChunkSize, _settings.ChunkOverlap, _settings.ChunkingStrategy);

        var chunker = SelectChunker(options.Strategy);

        _logger.LogInformation(
            "Extracting chunks for '{FileName}' (strategy: {Strategy}, size: {Size}, overlap: {Overlap})",
            fileName, options.Strategy, options.ChunkSize, options.ChunkOverlap);

        var textChunks = chunker.Chunk(document, options.ChunkSize, options.ChunkOverlap);
        var chunks     = ComposeChunks(document, textChunks, options.ChunkSize);

        return new ChunkExtractionResult
        {
            DocumentId = document.Id,
            PageCount  = document.Pages is { Count: > 0 } ? document.Pages.Count : 1,
            LowText    = document.Metadata.TryGetValue("LowText", out var lowText)
                         && string.Equals(lowText, "true", StringComparison.OrdinalIgnoreCase),
            Chunks     = chunks
        };
    }

    /// <summary>
    /// Maps the strategy name to a concrete chunker. <c>"sentence"</c> selects
    /// <see cref="SentenceAwareChunker"/>; any other value (including unknown) falls back to
    /// <see cref="SlidingWindowChunker"/> — the same semantics as the startup DI selection.
    /// </summary>
    /// <param name="strategy">The configured strategy name.</param>
    private IDocumentChunker SelectChunker(string strategy)
        => string.Equals(strategy, ApplicationConstants.ChunkingStrategySentence, StringComparison.OrdinalIgnoreCase)
            ? _sentenceChunker
            : _slidingChunker;

    /// <summary>
    /// Composes the final chunk sequence in page order: for each page, the page's text chunks,
    /// then its table chunks (row-to-sentence), then its image stubs. Indexes are assigned
    /// sequentially over the composed list. Public and static so tests can exercise the
    /// composition directly with fabricated inputs.
    /// </summary>
    /// <param name="document">The parsed document (provides per-page tables/images).</param>
    /// <param name="textChunks">The page-ordered text chunks produced by the chunking strategy.</param>
    /// <param name="chunkSize">The target chunk size used to pack short table row-sentences.</param>
    /// <returns>The sequentially indexed chunk sequence.</returns>
    public static IReadOnlyList<ExtractedChunk> ComposeChunks(
        RagDocument document, IReadOnlyList<RagChunk> textChunks, int chunkSize)
    {
        var result = new List<ExtractedChunk>();
        var index  = 0;

        // Text chunks arrive page-ordered from the chunker; group them into per-page runs so
        // table/image chunks can be appended after their page's text.
        var pages = document.Pages is { Count: > 0 }
            ? document.Pages
            : new List<RagPageContent> { new() { PageNumber = 1, Text = document.Content ?? string.Empty } };

        foreach (var page in pages)
        {
            foreach (var chunk in textChunks.Where(c => c.PageNumber == page.PageNumber))
            {
                result.Add(new ExtractedChunk
                {
                    Index      = index++,
                    Text       = chunk.Text,
                    PageNumber = chunk.PageNumber,
                    ChunkType  = ChunkTypes.Text,
                    BboxX1     = chunk.BboxX1,
                    BboxY1     = chunk.BboxY1,
                    BboxX2     = chunk.BboxX2,
                    BboxY2     = chunk.BboxY2,
                    TokenCount = chunk.TokenCount
                });
            }

            foreach (var table in page.Tables)
            {
                foreach (var packed in PackRowSentences(table, chunkSize))
                {
                    result.Add(new ExtractedChunk
                    {
                        Index      = index++,
                        Text       = packed,
                        PageNumber = page.PageNumber,
                        ChunkType  = ChunkTypes.Table,
                        BboxX1     = table.X1,
                        BboxY1     = table.Y1,
                        BboxX2     = table.X2,
                        BboxY2     = table.Y2,
                        TokenCount = (int)Math.Ceiling(packed.Length / 4.0)
                    });
                }
            }

            foreach (var image in page.Images)
            {
                // TODO: replace the placeholder with vision-model caption text when captioning
                // lands — the stub keeps the chunk sequence aligned and gives retrieval a citable
                // hook for image pages in the meantime. Bbox stays null per the pending decision.
                var text = $"[Image: page {page.PageNumber}, image {image.Index}]";
                result.Add(new ExtractedChunk
                {
                    Index      = index++,
                    Text       = text,
                    PageNumber = page.PageNumber,
                    ChunkType  = ChunkTypes.Image,
                    TokenCount = (int)Math.Ceiling(text.Length / 4.0)
                });
            }
        }

        return result;
    }

    /// <summary>
    /// Converts a table's data rows into natural-language sentences and packs consecutive
    /// sentences greedily while their combined length stays within <paramref name="chunkSize"/>
    /// (so 2–3 short rows share a chunk; long rows get their own).
    /// </summary>
    /// <param name="table">The detected table.</param>
    /// <param name="chunkSize">The target chunk size in characters.</param>
    private static IEnumerable<string> PackRowSentences(RagTable table, int chunkSize)
    {
        var current = new List<string>();
        var currentLength = 0;

        foreach (var row in table.Rows)
        {
            var sentence = BuildRowSentence(table.Headers, row);
            if (sentence.Length == 0)
                continue;

            var addedLength = currentLength == 0 ? sentence.Length : sentence.Length + 1;
            if (currentLength + addedLength > chunkSize && current.Count > 0)
            {
                yield return string.Join(" ", current);
                current = new List<string>();
                currentLength = 0;
                addedLength = sentence.Length;
            }

            current.Add(sentence);
            currentLength += addedLength;
        }

        if (current.Count > 0)
            yield return string.Join(" ", current);
    }

    /// <summary>
    /// Converts one table row into a retrievable sentence. For a <b>key/value</b> table (empty
    /// <paramref name="headers"/>) the row is rendered <c>"{key}: {value}"</c>
    /// (e.g. <c>"Interfaces: FPD link / GMSL / Ethernet"</c>). For a header table it uses the
    /// header names, e.g. <c>"The N9020B has Frequency Range 10Hz–26.5GHz, RBW 1Hz."</c>; when the
    /// headers carry no usable text (e.g. all numeric), it falls back to <c>"{header}: {value}"</c>
    /// pairs joined by <c>"; "</c>. Empty cells are skipped; an empty row yields an empty string.
    /// </summary>
    /// <param name="headers">The table's header cells; empty for a key/value table.</param>
    /// <param name="row">The row's cells.</param>
    public static string BuildRowSentence(IReadOnlyList<string> headers, IReadOnlyList<string> row)
    {
        if (row.Count == 0 || row.All(string.IsNullOrWhiteSpace))
            return string.Empty;

        // Key/value table: "{key}: {value}" (value = remaining cells joined).
        if (headers.Count == 0)
        {
            var key = row[0].Trim();
            var value = string.Join(" ", row.Skip(1).Where(c => !string.IsNullOrWhiteSpace(c)).Select(c => c.Trim()));
            return string.IsNullOrWhiteSpace(value) ? $"{key}." : $"{key}: {value}.";
        }

        var shared = Math.Min(headers.Count, row.Count);
        var headersUsable = headers.Count > 1
            && !string.IsNullOrWhiteSpace(row[0])
            && headers.Skip(1).Any(h => h.Any(char.IsLetter));

        if (headersUsable)
        {
            var pairs = new List<string>();
            for (var i = 1; i < shared; i++)
            {
                if (!string.IsNullOrWhiteSpace(headers[i]) && !string.IsNullOrWhiteSpace(row[i]))
                    pairs.Add($"{headers[i]} {row[i]}");
            }

            if (pairs.Count > 0)
                return $"The {row[0].Trim()} has {string.Join(", ", pairs)}.";
        }

        // Fallback: "{header}: {value}" pairs (or bare values when no header text exists).
        var kv = new List<string>();
        for (var i = 0; i < row.Count; i++)
        {
            if (string.IsNullOrWhiteSpace(row[i]))
                continue;

            var header = i < headers.Count ? headers[i] : null;
            kv.Add(string.IsNullOrWhiteSpace(header) ? row[i].Trim() : $"{header}: {row[i].Trim()}");
        }

        return kv.Count > 0 ? string.Join("; ", kv) + "." : string.Empty;
    }
}
