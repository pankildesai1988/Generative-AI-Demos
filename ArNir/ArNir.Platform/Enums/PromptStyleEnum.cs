namespace ArNir.Platform.Enums;

/// <summary>
/// Defines the prompt engineering styles supported by the platform.
/// </summary>
public enum PromptStyleEnum
{
    /// <summary>
    /// Zero-shot prompting: the model receives no examples and is expected to respond based solely on the instruction.
    /// </summary>
    ZeroShot,

    /// <summary>
    /// Few-shot prompting: a small number of input-output examples are provided within the prompt to guide the model.
    /// </summary>
    FewShot,

    /// <summary>
    /// Role-based prompting: the model is assigned a specific persona or role to shape the tone and content of its response.
    /// </summary>
    Role,

    /// <summary>
    /// Retrieval-Augmented Generation (RAG) prompting: relevant document chunks are injected into the prompt as context
    /// before the model generates its answer.
    /// </summary>
    Rag,

    /// <summary>
    /// Hybrid prompting: combines two or more prompt styles (e.g. RAG + Role) to maximise response quality.
    /// </summary>
    Hybrid
}
