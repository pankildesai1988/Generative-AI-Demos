namespace _2_OpenAIChatDemo.Models
{
    public class SessionComparison
    {
        public int Id { get; set; }
        public int OriginalSessionId { get; set; }
        public string InputText { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<ComparisonResult> Results { get; set; }
    }

}
