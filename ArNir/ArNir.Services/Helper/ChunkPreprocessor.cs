using System.Text;
using System.Text.RegularExpressions;

namespace ArNir.Services.Helpers
{
    public static class ChunkPreprocessor
    {
        public static string CleanText(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return string.Empty;

            // Normalize whitespace (collapse multiple spaces/newlines)
            string cleaned = Regex.Replace(input, @"\s+", " ");

            // Normalize special characters (optional: remove or replace)
            cleaned = cleaned.Replace("–", "-")   // En dash → hyphen
                             .Replace("—", "-")   // Em dash → hyphen
                             .Replace("“", "\"")  // Smart quotes → "
                             .Replace("”", "\"")
                             .Replace("’", "'");

            // Trim final output
            return cleaned.Trim();
        }
    }
}
