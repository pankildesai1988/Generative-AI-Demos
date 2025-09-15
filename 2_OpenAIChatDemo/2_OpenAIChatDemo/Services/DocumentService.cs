using _2_OpenAIChatDemo.Data;
using _2_OpenAIChatDemo.Models;
using DocumentFormat.OpenXml.InkML;
using DocumentFormat.OpenXml.Packaging;
using Markdig;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UglyToad.PdfPig;

namespace _2_OpenAIChatDemo.Services
{
    public class DocumentService : IDocumentService
    {
        private readonly ChatDbContext _context;
        private readonly ILogger<DocumentService> _logger;

        public DocumentService(ChatDbContext context, ILogger<DocumentService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Document> UploadDocumentAsync(string fileName, string fileType, string uploadedBy, string filePath)
        {
            string content = string.Empty;

            try
            {
                switch (fileType.ToLower())
                {
                    case "pdf":
                        _logger.LogInformation("Extracting text from PDF: {File}", fileName);
                        using (var pdf = PdfDocument.Open(filePath))
                        {
                            foreach (var page in pdf.GetPages())
                            {
                                content += page.Text + "\n";
                            }
                        }
                        break;

                    case "docx":
                        _logger.LogInformation("Extracting text from DOCX: {File}", fileName);
                        using (var doc = WordprocessingDocument.Open(filePath, false))
                        {
                            var body = doc.MainDocumentPart?.Document?.Body;
                            if (body == null)
                            {
                                throw new InvalidOperationException("DOCX file has no body content.");
                            }
                            content = body.InnerText;
                        }
                        break;

                    case "md":
                    case "markdown":
                        _logger.LogInformation("Extracting text from Markdown: {File}", fileName);
                        content = await File.ReadAllTextAsync(filePath);
                        content = Markdown.ToPlainText(content);
                        break;

                    case "txt":
                        _logger.LogInformation("Extracting text from TXT: {File}", fileName);
                        content = await File.ReadAllTextAsync(filePath);
                        break;

                    default:
                        _logger.LogWarning("Unsupported file type: {FileType}", fileType);
                        throw new NotSupportedException($"File type {fileType} is not supported");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to extract text from {File}", fileName);
                throw new ApplicationException($"Failed to extract text from {fileName}: {ex.Message}", ex);
            }

            if (string.IsNullOrWhiteSpace(content))
            {
                _logger.LogWarning("File {File} resulted in empty content", fileName);
                throw new ApplicationException("The file did not contain extractable text.");
            }

            // ✅ Chunking
            var document = new Document
            {
                Name = fileName,
                Type = fileType,
                UploadedBy = uploadedBy,
                UploadedAt = DateTime.UtcNow,
                Chunks = new List<DocumentChunk>()
            };

            int chunkSize = 500;
            int chunkOrder = 0;

            for (int i = 0; i < content.Length; i += chunkSize)
            {
                var chunkText = content.Substring(i, Math.Min(chunkSize, content.Length - i));
                document.Chunks.Add(new DocumentChunk
                {
                    ChunkOrder = chunkOrder++,
                    Text = chunkText
                });
            }

            _context.Documents.Add(document);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Successfully ingested {File} with {ChunkCount} chunks", fileName, document.Chunks.Count);

            return document;
        }


        public async Task<List<Document>> GetDocumentsAsync()
        {
            return await _context.Documents
                .OrderByDescending(d => d.UploadedAt)
                .ToListAsync();
        }

        public async Task<Document?> GetDocumentByIdAsync(int id)
        {
            return await _context.Documents
                .Include(d => d.Chunks)
                .FirstOrDefaultAsync(d => d.Id == id);
        }

        public async Task<List<DocumentChunk>> GetChunksByDocumentIdAsync(int documentId)
        {
            return await _context.DocumentChunks
                .Where(c => c.DocumentId == documentId)
                .OrderBy(c => c.ChunkOrder)
                .ToListAsync();
        }

        private string ParseFile(string filePath, string fileType)
        {
            switch (fileType.ToLower())
            {
                case "pdf":
                    return ParsePdf(filePath);

                case "docx":
                    return ParseDocx(filePath);

                case "md":
                    return ParseMarkdown(filePath);

                case "txt":
                default:
                    return File.ReadAllText(filePath);
            }
        }

        private string ParsePdf(string filePath)
        {
            using var pdf = PdfDocument.Open(filePath);
            var text = new System.Text.StringBuilder();
            foreach (var page in pdf.GetPages())
            {
                text.AppendLine(page.Text);
            }
            return text.ToString();
        }

        private string ParseDocx(string filePath)
        {
            using var doc = WordprocessingDocument.Open(filePath, false);
            var body = doc.MainDocumentPart.Document.Body;
            return body.InnerText;
        }

        private string ParseMarkdown(string filePath)
        {
            var mdContent = File.ReadAllText(filePath);
            return Markdown.ToPlainText(mdContent);
        }
    }
}
