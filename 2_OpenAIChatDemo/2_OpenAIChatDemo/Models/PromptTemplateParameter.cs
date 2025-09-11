namespace _2_OpenAIChatDemo.Models
{
    public class PromptTemplateParameter
    {
        public int Id { get; set; }
        public int TemplateId { get; set; }
        public string Name { get; set; }
        public string KeyName { get; set; }
        public string Type { get; set; } = "text";   // new
        public string Options { get; set; }
        public string DefaultValue { get; set; }
        public bool IsRequired { get; set; }         // new
        public string RegexPattern { get; set; }     // new
        public PromptTemplate Template { get; set; }
    }

}
