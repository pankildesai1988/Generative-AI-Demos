using ArNir.Core.Config;
using ArNir.Core.DTOs.Documents;
using ArNir.Core.Entities;
using ArNir.Data;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace ArNir.Service
{
    public class DocumentService : IDocumentService
    {
        private readonly ArNirDbContext _context;
        private readonly IMapper _mapper;
        private readonly FileUploadSettings _fileSettings;

        public DocumentService(
            ArNirDbContext context,
            IMapper mapper,
            IOptions<FileUploadSettings> fileSettings)
        {
            _context = context;
            _mapper = mapper;
            _fileSettings = fileSettings.Value;
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

        public async Task<DocumentResponseDto> UploadDocumentAsync(DocumentUploadDto dto)
        {
            ValidateFile(dto.File);

            using var reader = new StreamReader(dto.File.OpenReadStream());
            var content = await reader.ReadToEndAsync();

            var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
            if (!Directory.Exists(uploadPath))
                Directory.CreateDirectory(uploadPath);

            var filePath = Path.Combine(uploadPath, dto.File.FileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await dto.File.CopyToAsync(stream);
            }

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

            return _mapper.Map<DocumentResponseDto>(document);
        }

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

                using var reader = new StreamReader(dto.NewFile.OpenReadStream());
                var content = await reader.ReadToEndAsync();

                _context.DocumentChunks.RemoveRange(doc.Chunks);
                doc.Chunks.Clear();

                doc.Chunks = ChunkText(content);
                doc.Name = dto.NewFile.FileName;
                doc.Type = dto.NewFile.ContentType;
            }

            await _context.SaveChangesAsync();
            return _mapper.Map<DocumentResponseDto>(doc);
        }

        public async Task<bool> DeleteDocumentAsync(int id)
        {
            var doc = await _context.Documents
                .Include(d => d.Chunks)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (doc == null) return false;

            _context.DocumentChunks.RemoveRange(doc.Chunks);
            _context.Documents.Remove(doc);

            await _context.SaveChangesAsync();
            return true;
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
                    Text = content.Substring(i, Math.Min(chunkSize, content.Length - i))
                });
            }

            return chunks;
        }
    }
}
