namespace _2_OpenAIChatDemo.DTOs
{
    public class ModelOutputDto
    {
        public string Provider { get; set; }   // "OpenAI", "Claude-3", "Gemini-1.5"
        public string ModelName { get; set; }  // "gpt-4o-mini", "opus", "pro"

        // ✅ Nullable because error cases don’t return content
        public string? ResponseText { get; set; }
        public double? LatencyMs { get; set; }
        public string? RawResponse { get; set; }

        // ✅ New fields for structured errors
        public bool IsError { get; set; }
        public string ErrorCode { get; set; }   // e.g. "Unauthorized", "RateLimited", "BadRequest"
        public string ErrorMessage { get; set; }
    }
}
