using ArNir.Core.Config;
using ArNir.Core.DTOs.Documents;
using ArNir.Core.Entities;
using ArNir.Data;
using ArNir.Platform.Configuration;
using ArNir.Services.Helpers;
using ArNir.Services.Interfaces;
using AutoMapper;
using DocumentFormat.OpenXml.Drawing;
using DocumentFormat.OpenXml.Packaging;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Text;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;

namespace ArNir.Services
{
    public class DocumentService : IDocumentService
    {
        private readonly ArNirDbContext _context;
        private readonly IMapper _mapper;
        private readonly FileUploadSettings _fileSettings;
        private readonly IEmbeddingService _embeddingService;
        private readonly RagSettings _ragSettings;

        public DocumentService(
            ArNirDbContext context,
            IMapper mapper,
            IOptions<FileUploadSettings> fileSettings,
            IEmbeddingService embeddingService,
            IOptions<RagSettings>? ragOptions = null)
        {
            _context = context;
            _mapper = mapper;
            _fileSettings = fileSettings.Value;
            _embeddingService = embeddingService;
            _ragSettings = ragOptions?.Value ?? new RagSettings();
        }

        public async Task<IEnumerable<DocumentResponseDto>> GetAllDocumentsAsync()
        {
            var docs = await _context.Documents
                .Include(d => d.Chunks)
                .ToListAsync();

            return _mapper.Map<IEnumerable<DocumentResponseDto>>(docs);
        }

        public async Task<DocumentResponseDto?> GetDocumentByIdAsync(int id)
        {
            var doc = await _context.Documents
                .Include(d => d.Chunks)
                .FirstOrDefaultAsync(d => d.Id == id);

            return doc == null ? null : _mapper.Map<DocumentResponseDto>(doc);
        }

        public async Task<bool> DeleteDocumentAsync(int id)
        {

            // Delete embeddings first
            await _embeddingService.DeleteEmbeddingsForDocumentAsync(id);

            // Delete chunks from SQL Server
            var chunks = _context.DocumentChunks.Where(c => c.DocumentId == id);
            _context.DocumentChunks.RemoveRange(chunks);

            // Delete document
            var doc = await _context.Documents.FindAsync(id);
            if (doc != null)
                _context.Documents.Remove(doc);

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task RebuildDocumentEmbeddingsAsync(int documentId, string model = ArNir.Core.EmbeddingModels.Default)
        {
            await _embeddingService.RebuildEmbeddingsForDocumentAsync(documentId, model);
        }

        // -------------------------------
        // Upload
        // -------------------------------
        public async Task<DocumentResponseDto> UploadDocumentAsync(DocumentUploadDto dto)
        {
            ValidateFile(dto.File);

            List<DocumentChunk> chunks;
            byte[]? fileContent = null;

            if (dto.File.ContentType == "application/pdf")
            {
                using var ms = new MemoryStream();
                await dto.File.OpenReadStream().CopyToAsync(ms);
                fileContent = ms.ToArray();
                chunks = ExtractPdfChunks(fileContent);
            }
            else
            {
                string content = await ExtractTextAsync(dto.File);
                chunks = ChunkText(content);
            }

            var document = new Document
            {
                Name = dto.File.FileName,
                Type = dto.File.ContentType,
                UploadedBy = dto.UploadedBy ?? "System",
                UploadedAt = DateTime.UtcNow,
                FileContent = fileContent,
                Chunks = chunks
            };

            _context.Documents.Add(document);
            await _context.SaveChangesAsync();

            await _embeddingService.RebuildEmbeddingsForDocumentAsync(document.Id);

            return _mapper.Map<DocumentResponseDto>(document);
        }

        // -------------------------------
        // Update
        // -------------------------------
        public async Task<DocumentResponseDto?> UpdateDocumentAsync(int id, DocumentUpdateDto dto)
        {
            var doc = await _context.Documents
                .Include(d => d.Chunks)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (doc == null) return null;

            if (!string.IsNullOrWhiteSpace(dto.Name))
                doc.Name = dto.Name;

            if (!string.IsNullOrWhiteSpace(dto.UploadedBy))
                doc.UploadedBy = dto.UploadedBy;

            if (!string.IsNullOrWhiteSpace(dto.Type))
                doc.Type = dto.Type;

            if (dto.NewFile != null && dto.NewFile.Length > 0)
            {
                ValidateFile(dto.NewFile);

                _context.DocumentChunks.RemoveRange(doc.Chunks);
                doc.Chunks.Clear();

                if (dto.NewFile.ContentType == "application/pdf")
                {
                    using var ms = new MemoryStream();
                    await dto.NewFile.OpenReadStream().CopyToAsync(ms);
                    doc.FileContent = ms.ToArray();
                    doc.Chunks = ExtractPdfChunks(doc.FileContent);
                }
                else
                {
                    doc.FileContent = null;
                    string content = await ExtractTextAsync(dto.NewFile);
                    doc.Chunks = ChunkText(content);
                }

                doc.Name = dto.NewFile.FileName;
                doc.Type = dto.NewFile.ContentType;
            }

            await _context.SaveChangesAsync();

            // ✅ Refresh embeddings after update
            await _embeddingService.RebuildEmbeddingsForDocumentAsync(doc.Id);

            return _mapper.Map<DocumentResponseDto>(doc);
        }

        // -------------------------------
        // Helpers
        // -------------------------------
        private void ValidateFile(IFormFile file)
        {
            if (!_fileSettings.AllowedTypes.Contains(file.ContentType))
                throw new InvalidOperationException(
                    $"Unsupported file type '{file.ContentType}'. Allowed: {string.Join(", ", _fileSettings.AllowedTypes)}");

            if (file.Length > _fileSettings.MaxFileSize)
                throw new InvalidOperationException(
                    $"File size exceeds {_fileSettings.MaxFileSize / 1024 / 1024} MB limit.");
        }

        // ✅ Smart extractor based on file type
        private async Task<string> ExtractTextAsync(IFormFile file)
        {
            if (file.ContentType == "application/pdf")
            {
                using var pdf = PdfDocument.Open(file.OpenReadStream());
                var sb = new StringBuilder();
                foreach (var page in pdf.GetPages())
                    sb.AppendLine(page.Text);
                return sb.ToString();
            }
            else if (file.ContentType == "application/vnd.openxmlformats-officedocument.wordprocessingml.document")
            {
                return ExtractTextFromDocx(file);
            }
            else if (file.ContentType == "text/plain")
            {
                using var reader = new StreamReader(file.OpenReadStream());
                return await reader.ReadToEndAsync();
            }
            else
            {
                throw new InvalidOperationException($"Unsupported file type: {file.ContentType}");
            }
        }

        private string ExtractTextFromDocx(IFormFile file)
        {
            using var stream = file.OpenReadStream();
            using var wordDoc = WordprocessingDocument.Open(stream, false);

            var sb = new StringBuilder();

            // Main body text
            var body = wordDoc.MainDocumentPart?.Document.Body;
            if (body != null)
            {
                foreach (var para in body.Descendants<DocumentFormat.OpenXml.Wordprocessing.Paragraph>())
                {
                    sb.AppendLine(para.InnerText);
                }
            }

            // Headers & Footers
            foreach (var headerPart in wordDoc.MainDocumentPart.HeaderParts)
            {
                foreach (var para in headerPart.RootElement.Descendants<DocumentFormat.OpenXml.Wordprocessing.Paragraph>())
                {
                    sb.AppendLine(para.InnerText);
                }
            }

            foreach (var footerPart in wordDoc.MainDocumentPart.FooterParts)
            {
                foreach (var para in footerPart.RootElement.Descendants<DocumentFormat.OpenXml.Wordprocessing.Paragraph>())
                {
                    sb.AppendLine(para.InnerText);
                }
            }

            return sb.ToString();
        }


        private List<DocumentChunk> ExtractPdfChunks(byte[] pdfBytes)
        {
            int chunkSize = _ragSettings.ChunkSize; // appsettings Rag:ChunkSize → const floor
            var chunks = new List<DocumentChunk>();
            int chunkOrder = 0;

            using var pdf = PdfDocument.Open(pdfBytes);
            foreach (Page page in pdf.GetPages())
            {
                var words = page.GetWords().ToList();
                if (words.Count == 0)
                    continue;

                var pageText = string.Join(" ", words.Select(w => w.Text));
                var cleanedText = ChunkPreprocessor.CleanText(pageText);
                if (string.IsNullOrWhiteSpace(cleanedText))
                    continue;

                // Compute bounding box as union of all word bboxes on this page
                double x1 = words.Min(w => w.BoundingBox.Left);
                double y1 = words.Min(w => w.BoundingBox.Bottom);
                double x2 = words.Max(w => w.BoundingBox.Right);
                double y2 = words.Max(w => w.BoundingBox.Top);

                bool hasImages = page.GetImages().Any();
                string chunkType = hasImages ? "image" : "text";

                // Split long page text into 500-char sub-chunks, all attributed to same page
                for (int i = 0; i < cleanedText.Length; i += chunkSize)
                {
                    var subText = cleanedText.Substring(i, Math.Min(chunkSize, cleanedText.Length - i));
                    chunks.Add(new DocumentChunk
                    {
                        ChunkOrder = chunkOrder++,
                        Text = subText,
                        PageNumber = page.Number,
                        BboxX1 = (float)x1,
                        BboxY1 = (float)y1,
                        BboxX2 = (float)x2,
                        BboxY2 = (float)y2,
                        ChunkType = chunkType
                    });
                }
            }

            return chunks;
        }

        private List<DocumentChunk> ChunkText(string content)
        {
            int chunkSize = _ragSettings.ChunkSize; // appsettings Rag:ChunkSize → const floor
            var chunks = new List<DocumentChunk>();
            int chunkOrder = 0;

            for (int i = 0; i < content.Length; i += chunkSize)
            {
                chunks.Add(new DocumentChunk
                {
                    ChunkOrder = chunkOrder++,
                    Text = ChunkPreprocessor.CleanText(content.Substring(i, Math.Min(chunkSize, content.Length - i)))
                });
            }

            return chunks;
        }
    }
}
