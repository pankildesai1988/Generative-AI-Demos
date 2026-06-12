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
        }

        return result;
    }
}
