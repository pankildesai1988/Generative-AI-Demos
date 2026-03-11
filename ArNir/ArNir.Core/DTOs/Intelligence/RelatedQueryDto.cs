namespace ArNir.Core.DTOs.Intelligence
{
    public class RelatedQueryDto
    {
        public string Prompt { get; set; } = string.Empty;
    }

    public class RelatedInsightDto
    {
        public string Summary { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string? Source { get; set; }
    }
}
