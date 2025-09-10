namespace _2_OpenAIChatDemo.DTOs
{
    public class PromptPreviewDto
    {
        public string TemplateText { get; set; }
        public Dictionary<string, string> Parameters { get; set; } = new();
    }
}
