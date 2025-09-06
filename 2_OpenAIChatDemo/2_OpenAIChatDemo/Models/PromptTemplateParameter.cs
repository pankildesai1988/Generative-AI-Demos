namespace _2_OpenAIChatDemo.Models
{
    public class PromptTemplateParameter
    {
        public int Id { get; set; }
        public int TemplateId { get; set; }
        public PromptTemplate Template { get; set; }
        public string Name { get; set; } = string.Empty;     // Display name (Tone)
        public string KeyName { get; set; } = string.Empty;  // Used in template {tone}
        public string? Options { get; set; }                 // Comma-separated values
        public string? DefaultValue { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

}
