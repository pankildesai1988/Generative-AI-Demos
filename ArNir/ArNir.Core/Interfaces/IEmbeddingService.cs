using ArNir.Core.DTOs.Embeddings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArNir.Core.Interfaces
{
    public interface IEmbeddingService
    {
        Task<List<EmbeddingResultDto>> GenerateForDocumentAsync(EmbeddingRequestDto request);
    }
}
