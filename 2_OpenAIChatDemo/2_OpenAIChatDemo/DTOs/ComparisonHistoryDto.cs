namespace _2_OpenAIChatDemo.DTOs
{
    public class ComparisonHistoryDto
    {
        public int Id { get; set; }
        public string InputText { get; set; }
        public DateTime CreatedAt { get; set; }

        public List<ComparisonHistoryResultDto> Results { get; set; }
    }

    public class ComparisonHistoryResultDto
    {
        public string Provider { get; set; }
        public string ModelName { get; set; }
        public bool IsError { get; set; }
        public string? ErrorCode { get; set; }
        public string? ErrorMessage { get; set; }
        public string? ResponseText { get; set; }   // ✅ add this
    }
}
