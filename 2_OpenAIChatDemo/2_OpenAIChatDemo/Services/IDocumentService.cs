using System.Collections.Generic;
using System.Threading.Tasks;
using _2_OpenAIChatDemo.Models;

namespace _2_OpenAIChatDemo.Services
{
    public interface IDocumentService
    {
        Task<Document> UploadDocumentAsync(string fileName, string fileType, string uploadedBy, string textContent);
        Task<List<Document>> GetDocumentsAsync();
        Task<Document?> GetDocumentByIdAsync(int id);
        Task<List<DocumentChunk>> GetChunksByDocumentIdAsync(int documentId);
    }
}
