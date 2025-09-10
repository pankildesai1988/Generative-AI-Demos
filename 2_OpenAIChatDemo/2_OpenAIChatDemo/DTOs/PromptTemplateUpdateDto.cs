namespace _2_OpenAIChatDemo.DTOs
{
    public class PromptTemplateUpdateDto
    {
        public string Name { get; set; }
        public string TemplateText { get; set; }
        public IEnumerable<PromptTemplateParameterDto> Parameters { get; set; }
    }
}
