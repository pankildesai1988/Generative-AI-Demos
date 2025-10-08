using ArNir.Core.DTOs.Embeddings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArNir.Services.Interfaces
{
    public interface IEmbeddingService
    {
        Task<List<EmbeddingResultDto>> GenerateForDocumentAsync(EmbeddingRequestDto request);
        Task<float[]> GenerateForQueryAsync(string text, string model = "text-embedding-ada-002");
        Task DeleteEmbeddingsForDocumentAsync(int documentId);
        Task<List<EmbeddingResultDto>> RebuildEmbeddingsForDocumentAsync(int documentId, string model = "text-embedding-ada-002");
    }
}
