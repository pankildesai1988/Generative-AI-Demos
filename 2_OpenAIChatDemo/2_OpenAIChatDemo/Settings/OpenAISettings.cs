namespace _2_OpenAIChatDemo.Settings
{
    public class OpenAISettings
    {
        public string ApiKey { get; set; } = string.Empty;
        public string DefaultModel { get; set; } = "gpt-4o"; // fallback
    }
}
