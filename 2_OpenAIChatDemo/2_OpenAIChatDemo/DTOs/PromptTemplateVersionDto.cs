namespace _2_OpenAIChatDemo.DTOs
{
    public class PromptTemplateVersionDto
    {
        public int Id { get; set; }
        public int Version { get; set; }
        public string Name { get; set; }
        public string KeyName { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
