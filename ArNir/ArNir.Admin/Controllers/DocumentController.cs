using ArNir.Core.DTOs;
using ArNir.Service;
using Microsoft.AspNetCore.Mvc;

namespace ArNir.Admin.Controllers
{
    public class DocumentController : Controller
    {
        private readonly IDocumentService _documentService;

        public DocumentController(IDocumentService documentService)
        {
            _documentService = documentService;
        }

        public async Task<IActionResult> Index()
        {
            var docs = await _documentService.GetAllDocumentsAsync();
            return View(docs);
        }

        public IActionResult Upload() => View();

        [HttpPost]
        public async Task<IActionResult> Upload(DocumentUploadDto dto)
        {
            if (!ModelState.IsValid) return View(dto);

            try
            {
                await _documentService.UploadDocumentAsync(dto);
                return RedirectToAction(nameof(Index));
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError("File", ex.Message);
                return View(dto);
            }
        }

        public async Task<IActionResult> Edit(int id)
        {
            var doc = await _documentService.GetDocumentByIdAsync(id);
            if (doc == null) return NotFound();

            var updateDto = new DocumentUpdateDto
            {
                Name = doc.Name,
                UploadedBy = doc.UploadedBy,
                Type = doc.Type
            };
            return View(updateDto);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, DocumentUpdateDto dto)
        {
            if (!ModelState.IsValid) return View(dto);

            try
            {
                await _documentService.UpdateDocumentAsync(id, dto);
                return RedirectToAction(nameof(Index));
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError("NewFile", ex.Message);
                return View(dto);
            }
        }

        public async Task<IActionResult> Details(int id)
        {
            var doc = await _documentService.GetDocumentByIdAsync(id);
            if (doc == null) return NotFound();

            return View(doc);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            await _documentService.DeleteDocumentAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
