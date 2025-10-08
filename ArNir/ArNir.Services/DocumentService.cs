using ArNir.Core.Config;
using ArNir.Core.DTOs.Documents;
using ArNir.Core.Entities;
using ArNir.Data;
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

namespace ArNir.Services
{
    public class DocumentService : IDocumentService
    {
        private readonly ArNirDbContext _context;
        private readonly IMapper _mapper;
        private readonly FileUploadSettings _fileSettings;
        private readonly IEmbeddingService _embeddingService;

        public DocumentService(
            ArNirDbContext context,
            IMapper mapper,
            IOptions<FileUploadSettings> fileSettings,
            IEmbeddingService embeddingService)
        {
            _context = context;
            _mapper = mapper;
            _fileSettings = fileSettings.Value;
            _embeddingService = embeddingService;
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

        public async Task RebuildDocumentEmbeddingsAsync(int documentId, string model = "text-embedding-ada-002")
        {
            await _embeddingService.RebuildEmbeddingsForDocumentAsync(documentId, model);
        }

        // -------------------------------
        // Upload
        // -------------------------------
        public async Task<DocumentResponseDto> UploadDocumentAsync(DocumentUploadDto dto)
        {
            ValidateFile(dto.File);

            // ✅ Extract clean text
            string content = await ExtractTextAsync(dto.File);

            // ✅ Save document + chunks
            var document = new Document
            {
                Name = dto.File.FileName,
                Type = dto.File.ContentType,
                UploadedBy = dto.UploadedBy ?? "System",
                UploadedAt = DateTime.UtcNow,
                Chunks = ChunkText(content)
            };

            _context.Documents.Add(document);
            await _context.SaveChangesAsync();

            // ✅ Immediately generate embeddings
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

                // ✅ Extract clean text
                string content = await ExtractTextAsync(dto.NewFile);

                // Replace old chunks
                _context.DocumentChunks.RemoveRange(doc.Chunks);
                doc.Chunks.Clear();

                doc.Chunks = ChunkText(content);
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


        private List<DocumentChunk> ChunkText(string content)
        {
            const int chunkSize = 500;
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
