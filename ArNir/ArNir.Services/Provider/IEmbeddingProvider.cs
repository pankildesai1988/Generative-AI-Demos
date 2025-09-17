using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArNir.Services.Provider
{
    public interface IEmbeddingProvider
    {
        Task<float[]> GenerateEmbeddingAsync(string text, string model);
    }
}
