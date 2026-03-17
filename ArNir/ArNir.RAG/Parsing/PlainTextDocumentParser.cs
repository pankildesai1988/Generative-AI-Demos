using ArNir.RAG.Interfaces;
using ArNir.RAG.Models;

namespace ArNir.RAG.Parsing;

/// <summary>
/// Parses plain-text documents (plain text, Markdown, CSV) by reading the stream as UTF-8.
/// </summary>
public sealed class PlainTextDocumentParser : IDocumentParser
{
    private static readonly HashSet<string> SupportedTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "text/plain",
        "text/markdown",
        "text/csv"
    };

    /// <inheritdoc />
    public bool CanParse(string contentType)
        => SupportedTypes.Contains(contentType);

    /// <inheritdoc />
    public async Task<RagDocument> ParseAsync(Stream stream, string fileName, string contentType)
    {
        using var reader = new StreamReader(stream, leaveOpen: true);
        var content = await reader.ReadToEndAsync();

        return new RagDocument
        {
            FileName      = fileName,
            ContentType   = contentType,
            Content       = content,
            FileSizeBytes = stream.CanSeek ? stream.Length : content.Length * sizeof(char),
            ParsedAt      = DateTime.UtcNow,
            Metadata      = new Dictionary<string, string>
            {
                ["Parser"] = nameof(PlainTextDocumentParser)
            }
        };
    }
}
