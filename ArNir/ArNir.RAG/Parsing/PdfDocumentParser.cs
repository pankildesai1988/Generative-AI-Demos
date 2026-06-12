using System.Text;
using ArNir.Platform.Configuration;
using ArNir.RAG.Extraction;
using ArNir.RAG.Interfaces;
using ArNir.RAG.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;

namespace ArNir.RAG.Parsing;

/// <summary>
/// Parses PDF documents using the PdfPig library, extracting text page by page
/// and populating <see cref="RagDocument.Pages"/> so downstream chunkers can attribute
/// each chunk to its source page number.
/// <para>
/// Per page it also runs the heuristic <see cref="TableExtractor"/> and records embedded image
/// references. Detected table words are excluded from the page's text stream (the table content
/// becomes dedicated row-sentence chunks instead, avoiding duplication); pages without detected
/// tables keep PdfPig's <c>page.Text</c> verbatim. The <c>LowText</c> flag is always computed
/// from the original full text so table extraction never changes its semantics.
/// </para>
/// <para>
/// The <c>LowText</c> threshold defaults to the <c>Rag</c> appsettings section
/// (<see cref="RagSettings.LowTextThreshold"/>), tunable without recompiling.
/// </para>
/// </summary>
public sealed class PdfDocumentParser : IDocumentParser
{
    private readonly RagSettings _settings;
    private readonly TableExtractor _tableExtractor;
    private readonly ILogger<PdfDocumentParser> _logger;

    /// <summary>Initialises the parser with appsettings-bound <see cref="RagSettings"/>.</summary>
    /// <param name="settings">The bound RAG options; when unconfigured, property defaults apply.</param>
    /// <param name="tableExtractor">The heuristic table detector; a default instance is created when omitted.</param>
    /// <param name="logger">Logger for diagnostic output; a null logger is used when omitted.</param>
    public PdfDocumentParser(
        IOptions<RagSettings> settings,
        TableExtractor? tableExtractor = null,
        ILogger<PdfDocumentParser>? logger = null)
    {
        _settings       = settings.Value;
        _tableExtractor = tableExtractor ?? new TableExtractor();
        _logger         = logger ?? NullLogger<PdfDocumentParser>.Instance;
    }

    /// <inheritdoc />
    public bool CanParse(string contentType)
        => string.Equals(contentType, "application/pdf", StringComparison.OrdinalIgnoreCase);

    /// <inheritdoc />
    public async Task<RagDocument> ParseAsync(Stream stream, string fileName, string contentType)
    {
        // PdfPig requires a seekable byte buffer.
        using var ms = new MemoryStream();
        await stream.CopyToAsync(ms);
        var bytes = ms.ToArray();

        var pages = new List<RagPageContent>();
        var sb    = new StringBuilder();

        using (var pdf = PdfDocument.Open(bytes))
        {
            foreach (Page page in pdf.GetPages())
            {
                var originalText = page.Text ?? string.Empty;
                var pageContent  = BuildPageContent(page, originalText);

                pages.Add(pageContent);
                // LowText is computed from the original full text (below), so table-word
                // exclusion in pageContent.Text never changes its semantics.
                sb.AppendLine(originalText);
            }
        }

        var content = sb.ToString();

        // Low-text signal: image-only / scanned PDFs (e.g. slide decks) yield almost no
        // extractable text via PdfPig. Flag it so downstream logs/UI can distinguish
        // "bad parse" from "no semantic match".
        var lowText = content.Replace("\n", "").Replace("\r", "").Trim().Length < _settings.LowTextThreshold;

        return new RagDocument
        {
            FileName      = fileName,
            ContentType   = contentType,
            Content       = content,
            Pages         = pages,
            FileSizeBytes = bytes.LongLength,
            ParsedAt      = DateTime.UtcNow,
            Metadata      = new Dictionary<string, string>
            {
                ["Parser"]    = nameof(PdfDocumentParser),
                ["PageCount"] = pages.Count.ToString(),
                ["LowText"]   = lowText ? "true" : "false"
            }
        };
    }

    /// <summary>
    /// Builds one page's content: detects tables over the page's positioned words, rebuilds the
    /// text stream from the non-table words when tables were found, and records image references.
    /// Conservative by design — any detection failure falls back to the original page text.
    /// </summary>
    /// <param name="page">The PdfPig page.</param>
    /// <param name="originalText">PdfPig's verbatim page text.</param>
    private RagPageContent BuildPageContent(Page page, string originalText)
    {
        var pageContent = new RagPageContent
        {
            PageNumber = page.Number, // PdfPig is 1-based
            Text       = originalText
        };

        try
        {
            var imageCount = page.GetImages().Count();
            for (var i = 1; i <= imageCount; i++)
                pageContent.Images.Add(new RagImageRef { Index = i });

            var words = page.GetWords()
                .Select(w => new WordBox(
                    w.Text,
                    w.BoundingBox.Left, w.BoundingBox.Bottom,
                    w.BoundingBox.Right, w.BoundingBox.Top))
                .ToList();

            var tables = _tableExtractor.DetectTables(words);
            if (tables.Count == 0)
                return pageContent;

            pageContent.Tables = tables.Select(t => new RagTable
            {
                Headers = t.Headers.ToList(),
                Rows    = t.Rows.Select(r => r.ToList()).ToList(),
                X1      = (float)t.X1,
                Y1      = (float)t.Y1,
                X2      = (float)t.X2,
                Y2      = (float)t.Y2
            }).ToList();

            // Rebuild the page text from the remaining words (line order, space-joined) so table
            // content isn't duplicated across text chunks and table chunks.
            var consumed  = new HashSet<WordBox>(tables.SelectMany(t => t.ConsumedWords));
            var remaining = words.Where(w => !consumed.Contains(w)).ToList();

            pageContent.Text = string.Join(
                "\n",
                TableExtractor.ClusterLines(remaining)
                    .Select(line => string.Join(" ", line.Select(w => w.Text))));
        }
        catch (Exception ex)
        {
            // Table/image extraction must never break normal text parsing.
            _logger.LogWarning(ex,
                "Table/image extraction failed on page {PageNumber}; falling back to plain text.",
                page.Number);
            pageContent.Tables.Clear();
            pageContent.Text = originalText;
        }

        return pageContent;
    }
}
