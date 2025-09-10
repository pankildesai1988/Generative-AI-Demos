namespace _2_OpenAIChatDemo.Models
{
    public class PromptTemplate
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string KeyName { get; set; }
        public string TemplateText { get; set; }
        public int Version { get; set; } = 1;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        public ICollection<PromptTemplateParameter> Parameters { get; set; }
    }

}
