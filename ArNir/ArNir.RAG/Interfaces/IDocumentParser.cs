using ArNir.RAG.Models;

namespace ArNir.RAG.Interfaces;

/// <summary>
/// Defines a strategy for parsing a raw document stream into a <see cref="RagDocument"/>.
/// Multiple implementations are registered and selected based on content type.
/// </summary>
public interface IDocumentParser
{
    /// <summary>
    /// Determines whether this parser can handle the given MIME content type.
    /// </summary>
    /// <param name="contentType">The MIME type of the document (e.g. "application/pdf").</param>
    /// <returns><c>true</c> if this parser supports the content type; otherwise <c>false</c>.</returns>
    bool CanParse(string contentType);

    /// <summary>
    /// Parses the document stream and returns a populated <see cref="RagDocument"/>.
    /// </summary>
    /// <param name="stream">A readable stream containing the raw document bytes.</param>
    /// <param name="fileName">The original file name, including extension.</param>
    /// <param name="contentType">The MIME content type of the document.</param>
    /// <returns>A <see cref="RagDocument"/> with extracted text content and metadata.</returns>
    Task<RagDocument> ParseAsync(Stream stream, string fileName, string contentType);
}
