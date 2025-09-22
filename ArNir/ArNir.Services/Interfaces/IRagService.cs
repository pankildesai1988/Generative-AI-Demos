using System.Threading.Tasks;
using ArNir.Core.DTOs;
using ArNir.Core.DTOs.RAG;

namespace ArNir.Services.Interfaces
{
    public interface IRagService
    {
        Task<RagResultDto> RunRagAsync(string query, int topK = 5, bool useHybrid = true, string promptStyle = "rag", bool saveAsNew = true);
    }
}
