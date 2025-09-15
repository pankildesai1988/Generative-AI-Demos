using System;
using System.Collections.Generic;
using System.Text;

namespace _2_OpenAIChatDemo.Services
{
    public class ChunkingService : IChunkingService
    {
        public List<string> SplitIntoChunks(string text, int maxChunkSize = 500, int overlap = 50)
        {
            var chunks = new List<string>();
            if (string.IsNullOrWhiteSpace(text))
                return chunks;

            int start = 0;
            while (start < text.Length)
            {
                int length = Math.Min(maxChunkSize, text.Length - start);
                string chunk = text.Substring(start, length);

                chunks.Add(chunk.Trim());

                // Move pointer forward with overlap
                start += (maxChunkSize - overlap);
                if (start < 0) break;
            }

            return chunks;
        }
    }
}
