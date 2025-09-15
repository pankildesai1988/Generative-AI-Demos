using System.Collections.Generic;

namespace _2_OpenAIChatDemo.Services
{
    public interface IChunkingService
    {
        List<string> SplitIntoChunks(string text, int maxChunkSize = 500, int overlap = 50);
    }
}
