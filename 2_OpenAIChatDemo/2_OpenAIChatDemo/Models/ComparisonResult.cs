namespace _2_OpenAIChatDemo.Models
{
    public class ComparisonResult
    {
        public int Id { get; set; }
        public int SessionComparisonId { get; set; }

        public string Provider { get; set; }   // "OpenAI", "Claude-3", "Gemini-1.5"
        public string ModelName { get; set; }  // "gpt-4o-mini", "opus", "pro"

        public string? ResponseText { get; set; }  // ✅ nullable
        public double? LatencyMs { get; set; }
        public string? RawResponse { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // ✅ Error fields
        public string ErrorCode { get; set; }
        public string ErrorMessage { get; set; }

        public SessionComparison SessionComparison { get; set; }
    }

}
