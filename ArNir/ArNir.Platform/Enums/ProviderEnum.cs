namespace ArNir.Platform.Enums;

/// <summary>
/// Identifies the AI model provider used to fulfil a request.
/// </summary>
public enum ProviderEnum
{
    /// <summary>
    /// OpenAI — provider of the GPT family of large language models.
    /// </summary>
    OpenAI,

    /// <summary>
    /// Google Gemini — Google's multimodal large language model family.
    /// </summary>
    Gemini,

    /// <summary>
    /// Anthropic Claude — Anthropic's large language model family.
    /// </summary>
    Claude
}
