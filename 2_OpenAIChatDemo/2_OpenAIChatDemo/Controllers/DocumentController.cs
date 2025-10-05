using _2_OpenAIChatDemo.DTOs;
using _2_OpenAIChatDemo.Models;
using _2_OpenAIChatDemo.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace _2_OpenAIChatDemo.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DocumentController : ControllerBase
    {
        private readonly IDocumentService _documentService;

        public DocumentController(IDocumentService documentService)
        {
            _documentService = documentService;
        }

        /// <summary>
        /// Uploads a new document (expects plain text for now).
        /// Later: extend for PDF/DOCX/Markdown parsing.
        /// </summary>
        [HttpPost("upload")]
        public async Task<IActionResult> UploadDocument([FromForm] string fileName, [FromForm] string fileType, [FromForm] string uploadedBy, [FromForm] string content)
        {
            if (string.IsNullOrWhiteSpace(content))
                return BadRequest("Document content is empty.");

            var document = await _documentService.UploadDocumentAsync(fileName, fileType, uploadedBy, content);
            return Ok(document);
        }


        [HttpPost("upload-file")]
        public async Task<IActionResult> UploadFile([FromForm] FileUploadRequest request)
        {
            if (request.File == null || request.File.Length == 0)
                return BadRequest("File is empty.");

            try
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "Uploads");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                var filePath = Path.Combine(uploadsFolder, request.File.FileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await request.File.CopyToAsync(stream);
                }

                var fileType = Path.GetExtension(request.File.FileName).TrimStart('.').ToLower();

                var document = await _documentService.UploadDocumentAsync(
                    request.File.FileName,
                    fileType,
                    request.UploadedBy,
                    filePath
                );

                return Ok(ToDto(document, includeChunks: true));
            }
            catch (NotSupportedException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (ApplicationException ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Unexpected error", details = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetDocuments()
        {
            var docs = await _documentService.GetDocumentsAsync();
            var dtoList = docs.Select(d => ToDto(d, includeChunks: false)).ToList();
            return Ok(dtoList);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetDocumentById(int id)
        {
            var doc = await _documentService.GetDocumentByIdAsync(id);
            if (doc == null) return NotFound();

            return Ok(ToDto(doc, includeChunks: true));
        }

        [HttpGet("{id}/chunks")]
        public async Task<IActionResult> GetChunks(int id)
        {
            var chunks = await _documentService.GetChunksByDocumentIdAsync(id);
            var dtoList = chunks.Select(c => new DocumentChunkDto
            {
                Id = c.Id,
                ChunkOrder = c.ChunkOrder,
                Text = c.Text
            }).ToList();

            return Ok(dtoList);
        }

        // -----------------------
        // 🔹 Helper Mapper Method
        // -----------------------
        private DocumentResponseDto ToDto(Models.Document doc, bool includeChunks)
        {
            return new DocumentResponseDto
            {
                Id = doc.Id,
                Name = doc.Name,
                Type = doc.Type,
                UploadedBy = doc.UploadedBy,
                UploadedAt = doc.UploadedAt,
                Chunks = includeChunks && doc.Chunks != null
                    ? doc.Chunks.Select(c => new DocumentChunkDto
                    {
                        Id = c.Id,
                        ChunkOrder = c.ChunkOrder,
                        Text = c.Text
                    }).ToList()
                    : new List<DocumentChunkDto>()
            };
        }
    }
}
