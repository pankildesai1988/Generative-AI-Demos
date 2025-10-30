using System.Threading.Tasks;

namespace ArNir.Services.Interfaces
{
    public interface IChatInsightService
    {
        Task<string> GenerateInsightAsync(string userPrompt);
    }
}
