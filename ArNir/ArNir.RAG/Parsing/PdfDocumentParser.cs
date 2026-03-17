using System.Text;
using ArNir.RAG.Interfaces;
using ArNir.RAG.Models;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;

namespace ArNir.RAG.Parsing;

/// <summary>
/// Parses PDF documents using the PdfPig library, extracting text page by page.
/// </summary>
public sealed class PdfDocumentParser : IDocumentParser
{
    /// <inheritdoc />
    public bool CanParse(string contentType)
        => string.Equals(contentType, "application/pdf", StringComparison.OrdinalIgnoreCase);

    /// <inheritdoc />
    public async Task<RagDocument> ParseAsync(Stream stream, string fileName, string contentType)
    {
        // PdfPig requires a seekable stream; buffer to memory if necessary.
        byte[] bytes;
        if (stream.CanSeek)
        {
            using var ms = new MemoryStream();
            await stream.CopyToAsync(ms);
            bytes = ms.ToArray();
        }
        else
        {
            using var ms = new MemoryStream();
            await stream.CopyToAsync(ms);
            bytes = ms.ToArray();
        }

        var sb = new StringBuilder();

        using (var pdf = PdfDocument.Open(bytes))
        {
            foreach (Page page in pdf.GetPages())
            {
                sb.AppendLine(page.Text);
            }
        }

        return new RagDocument
        {
            FileName    = fileName,
            ContentType = contentType,
            Content     = sb.ToString(),
            FileSizeBytes = bytes.LongLength,
            ParsedAt    = DateTime.UtcNow,
            Metadata    = new Dictionary<string, string>
            {
                ["Parser"] = nameof(PdfDocumentParser)
            }
        };
    }
}
