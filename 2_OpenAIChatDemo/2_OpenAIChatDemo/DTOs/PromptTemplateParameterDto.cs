namespace _2_OpenAIChatDemo.DTOs
{
    public class PromptTemplateParameterDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string KeyName { get; set; }
        public string Type { get; set; } = "text";   // new
        public string Options { get; set; }
        public string DefaultValue { get; set; }
        public bool IsRequired { get; set; }         // new
        public string RegexPattern { get; set; }     // new
    }
}
