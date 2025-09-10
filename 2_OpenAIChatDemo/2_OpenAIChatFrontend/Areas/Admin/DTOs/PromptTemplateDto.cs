namespace _2_OpenAIChatFrontend.Areas.Admin.DTOs
{
    public class PromptTemplateDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string KeyName { get; set; }
        public string TemplateText { get; set; }
        public int Version { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public IEnumerable<PromptTemplateParameterDto> Parameters { get; set; }
    }
}
