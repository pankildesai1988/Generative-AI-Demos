using System.Text;
using ArNir.RAG.Interfaces;
using ArNir.RAG.Models;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;

namespace ArNir.RAG.Parsing;

/// <summary>
/// Parses PDF documents using the PdfPig library, extracting text page by page
/// and populating <see cref="RagDocument.Pages"/> so downstream chunkers can attribute
/// each chunk to its source page number.
/// </summary>
public sealed class PdfDocumentParser : IDocumentParser
{
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

        return new RagDocument
        {
            FileName      = fileName,
            ContentType   = contentType,
            Content       = sb.ToString(),
            Pages         = pages,
            FileSizeBytes = bytes.LongLength,
            ParsedAt      = DateTime.UtcNow,
            Metadata      = new Dictionary<string, string>
            {
                ["Parser"]    = nameof(PdfDocumentParser),
                ["PageCount"] = pages.Count.ToString()
            }
        };
    }
}
