namespace _2_OpenAIChatDemo.DTOs
{
    public class PromptTemplateCreateDto
    {
        public string Name { get; set; }
        public string KeyName { get; set; }
        public string TemplateText { get; set; }
        public IEnumerable<PromptTemplateParameterDto> Parameters { get; set; }
    }
}
