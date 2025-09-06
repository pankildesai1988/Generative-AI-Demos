namespace _2_OpenAIChatDemo.Models
{
    public class PromptTemplate
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;   // Display name
        public string KeyName { get; set; } = string.Empty; // Unique key (e.g., "summary")
        public string TemplateText { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<PromptTemplateParameter> Parameters { get; set; } = new List<PromptTemplateParameter>();
    }

}
