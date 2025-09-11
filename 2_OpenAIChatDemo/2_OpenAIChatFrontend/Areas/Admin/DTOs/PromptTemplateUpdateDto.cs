namespace _2_OpenAIChatFrontend.Areas.Admin.DTOs
{
    public class PromptTemplateUpdateDto
    {
        public string Name { get; set; }
        public string TemplateText { get; set; }
        public IEnumerable<PromptTemplateParameterDto> Parameters { get; set; }
    }
}
