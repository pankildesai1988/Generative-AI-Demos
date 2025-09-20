using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArNir.Services.Interfaces
{
    public interface IOpenAiService
    {
        Task<string> GetCompletionAsync(string prompt, string model = "gpt-4o");
    }
}
