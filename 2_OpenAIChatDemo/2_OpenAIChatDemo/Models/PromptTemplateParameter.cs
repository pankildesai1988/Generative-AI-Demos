namespace _2_OpenAIChatDemo.Models
{
    public class PromptTemplateParameter
    {
        public int Id { get; set; }
        public int TemplateId { get; set; }
        public string Name { get; set; }
        public string KeyName { get; set; }
        public string Options { get; set; }
        public string DefaultValue { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public PromptTemplate Template { get; set; }
    }

}
