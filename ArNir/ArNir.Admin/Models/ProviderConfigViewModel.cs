namespace ArNir.Admin.Models;

public class ProviderConfigViewModel
{
    public string OpenAiApiKeyMasked { get; set; } = "";
    public string OpenAiEmbeddingModel { get; set; } = "text-embedding-ada-002";
    public string OpenAiChatModel { get; set; } = "gpt-4o-mini";
    public bool OpenAiKeyIsSet { get; set; }
}
