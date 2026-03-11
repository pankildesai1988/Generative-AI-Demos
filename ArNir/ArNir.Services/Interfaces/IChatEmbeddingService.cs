using ArNir.Core.Entities;
using System.Threading.Tasks;

namespace ArNir.Services.Interfaces
{
    public interface IChatEmbeddingService
    {
        Task<Guid?> GenerateEmbeddingForMessageAsync(int chatMemoryId, string text, string model = "text-embedding-3-small");
        Task<ChatEmbedding?> FindSimilarAsync(float[] queryVector, int limit = 1);
    }
}