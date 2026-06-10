namespace ArNir.Admin.Models;

public class ProviderConfigViewModel
{
    public string OpenAiApiKeyMasked { get; set; } = "";
    public string OpenAiEmbeddingModel { get; set; } = ArNir.Core.EmbeddingModels.Default;
    public string OpenAiChatModel { get; set; } = "gpt-4o-mini";
    public bool OpenAiKeyIsSet { get; set; }
}
