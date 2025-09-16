using ArNir.Core.DTOs;

namespace ArNir.Service
{
    public interface IDocumentService
    {
        /// <summary>
        /// Get all uploaded documents with metadata + chunks.
        /// </summary>
        Task<IEnumerable<DocumentResponseDto>> GetAllDocumentsAsync();

        /// <summary>
        /// Get a single document by its Id (with chunks).
        /// </summary>
        Task<DocumentResponseDto?> GetDocumentByIdAsync(int id);

        /// <summary>
        /// Upload a new document and split it into chunks.
        /// </summary>
        Task<DocumentResponseDto> UploadDocumentAsync(DocumentUploadDto dto);

        /// <summary>
        /// Update metadata or replace file (deletes old chunks if new file uploaded).
        /// </summary>
        Task<DocumentResponseDto?> UpdateDocumentAsync(int id, DocumentUpdateDto dto);

        /// <summary>
        /// Delete a document and all of its chunks.
        /// </summary>
        Task<bool> DeleteDocumentAsync(int id);
    }
}
