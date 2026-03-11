using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArNir.Services.AI.Interfaces
{
    public interface IContextMemoryService
    {
        Task SaveContextAsync(string sessionId, string message);
        Task<string?> RetrieveContextAsync(string sessionId);
    }
}
