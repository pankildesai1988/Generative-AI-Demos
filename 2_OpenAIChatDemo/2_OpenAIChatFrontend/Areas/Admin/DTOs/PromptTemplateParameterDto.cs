namespace _2_OpenAIChatFrontend.Areas.Admin.DTOs
{
    public class PromptTemplateParameterDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string KeyName { get; set; }
        public string Options { get; set; }
        public string DefaultValue { get; set; }
    }
}
