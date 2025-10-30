using System.Threading.Tasks;

namespace ArNir.Services.Interfaces
{
    public interface INaturalLanguageCommandService
    {
        Task<string?> TryParseCommandAsync(string userInput);
    }
}