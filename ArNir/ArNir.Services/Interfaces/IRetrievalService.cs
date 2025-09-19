using ArNir.Core.DTOs.Documents;
using ArNir.Core.DTOs.Embeddings;

namespace ArNir.Services.Interfaces
{
    public interface IRetrievalService
    {
        Task<List<ChunkResultDto>> SearchAsync(string query, int topK = 5, bool useHybrid = false);
    }
}
