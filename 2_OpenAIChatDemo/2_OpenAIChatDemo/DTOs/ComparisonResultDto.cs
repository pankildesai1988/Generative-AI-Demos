namespace _2_OpenAIChatDemo.DTOs
{
    public class ComparisonResultDto
    {
        public int ComparisonId { get; set; }
        public string InputText { get; set; }
        public List<ModelOutputDto> Results { get; set; }
    }
}
