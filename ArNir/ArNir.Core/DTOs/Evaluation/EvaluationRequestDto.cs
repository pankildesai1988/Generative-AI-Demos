namespace ArNir.Core.DTOs.Evaluation;

/// <summary>Input DTO for manually triggering an evaluation via the API.</summary>
public class EvaluationRequestDto
{
    /// <summary>The question to evaluate.</summary>
    public string Question { get; set; } = string.Empty;

    /// <summary>The AI-generated answer to evaluate.</summary>
    public string Answer { get; set; } = string.Empty;

    /// <summary>The supporting context (e.g. retrieved RAG chunks).</summary>
    public string Context { get; set; } = string.Empty;
}
