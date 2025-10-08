using SharpToken;
using System.Collections.Generic;
using System.Linq;

namespace ArNir.Core.Utils
{
    public static class TokenizerUtil
    {
        public static int CountTokens(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return 0;

            var encoding = GptEncoding.GetEncoding("cl100k_base");
            return encoding.Encode(text).Count;
        }

        public static int CountTokens(IEnumerable<string> texts)
        {
            if (texts == null) return 0;
            var encoding = GptEncoding.GetEncoding("cl100k_base");
            return texts.Sum(t => encoding.Encode(t ?? string.Empty).Count);
        }
    }
}
