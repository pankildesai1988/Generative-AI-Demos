namespace _2_OpenAIChatDemo.DTOs
{
    public class PromptTemplateDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string KeyName { get; set; }
        public string TemplateText { get; set; }
        public int Version { get; set; }
        public bool IsActive { get; set; }
        public IEnumerable<PromptTemplateParameterDto> Parameters { get; set; }
    }
}
