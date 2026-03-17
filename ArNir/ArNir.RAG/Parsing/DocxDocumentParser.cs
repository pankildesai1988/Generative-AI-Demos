using System.Text;
using ArNir.RAG.Interfaces;
using ArNir.RAG.Models;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

namespace ArNir.RAG.Parsing;

/// <summary>
/// Parses DOCX documents using the DocumentFormat.OpenXml library, extracting paragraph text.
/// </summary>
public sealed class DocxDocumentParser : IDocumentParser
{
    private const string DocxMimeType =
        "application/vnd.openxmlformats-officedocument.wordprocessingml.document";

    /// <inheritdoc />
    public bool CanParse(string contentType)
        => string.Equals(contentType, DocxMimeType, StringComparison.OrdinalIgnoreCase);

    /// <inheritdoc />
    public async Task<RagDocument> ParseAsync(Stream stream, string fileName, string contentType)
    {
        // Buffer to memory so OpenXml can seek.
        var ms = new MemoryStream();
        await stream.CopyToAsync(ms);
        ms.Position = 0;

        var sb = new StringBuilder();

        using (var wordDoc = WordprocessingDocument.Open(ms, isEditable: false))
        {
            var body = wordDoc.MainDocumentPart?.Document?.Body;
            if (body is not null)
            {
                foreach (var paragraph in body.Descendants<Paragraph>())
                {
                    var text = paragraph.InnerText;
                    if (!string.IsNullOrWhiteSpace(text))
                    {
                        sb.AppendLine(text);
                    }
                }
            }
        }

        return new RagDocument
        {
            FileName      = fileName,
            ContentType   = contentType,
            Content       = sb.ToString(),
            FileSizeBytes = ms.Length,
            ParsedAt      = DateTime.UtcNow,
            Metadata      = new Dictionary<string, string>
            {
                ["Parser"] = nameof(DocxDocumentParser)
            }
        };
    }
}
