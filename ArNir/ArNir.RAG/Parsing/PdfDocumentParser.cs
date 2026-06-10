using System.Text;
using ArNir.Platform.Configuration;
using ArNir.RAG.Interfaces;
using ArNir.RAG.Models;
using Microsoft.Extensions.Options;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;

namespace ArNir.RAG.Parsing;

/// <summary>
/// Parses PDF documents using the PdfPig library, extracting text page by page
/// and populating <see cref="RagDocument.Pages"/> so downstream chunkers can attribute
/// each chunk to its source page number.
/// <para>
/// The <c>LowText</c> threshold defaults to the <c>Rag</c> appsettings section
/// (<see cref="RagSettings.LowTextThreshold"/>), tunable without recompiling.
/// </para>
/// </summary>
public sealed class PdfDocumentParser : IDocumentParser
{
    private readonly RagSettings _settings;

    /// <summary>Initialises the parser with appsettings-bound <see cref="RagSettings"/>.</summary>
    /// <param name="settings">The bound RAG options; when unconfigured, property defaults apply.</param>
    public PdfDocumentParser(IOptions<RagSettings> settings)
        => _settings = settings.Value;

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
                var text = page.Text ?? string.Empty;
                pages.Add(new RagPageContent
                {
                    PageNumber = page.Number, // PdfPig is 1-based
                    Text       = text
                });
                sb.AppendLine(text);
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
}
