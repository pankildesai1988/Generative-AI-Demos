namespace _2_OpenAIChatDemo.DTOs
{
    public class ComparisonRequestDto
    {
        public int OriginalSessionId { get; set; }
        public string InputText { get; set; }
        public List<string> ModelNames { get; set; } // ["GPT-4", "Claude-3"]
    }
}
