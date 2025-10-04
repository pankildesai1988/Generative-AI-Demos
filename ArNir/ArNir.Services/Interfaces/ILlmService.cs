using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArNir.Services.Interfaces
{
    public interface ILlmService
    {
        Task<string> GetCompletionAsync(string prompt, string model);
    }
}
