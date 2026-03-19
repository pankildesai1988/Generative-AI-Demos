namespace ArNir.Core.DTOs.Evaluation;

/// <summary>Output DTO representing a persisted evaluation result.</summary>
public class EvaluationResultDto
{
    /// <summary>Evaluation record identifier.</summary>
    public int Id { get; set; }

    /// <summary>The question that was evaluated.</summary>
    public string Question { get; set; } = string.Empty;

    /// <summary>The answer that was evaluated.</summary>
    public string Answer { get; set; } = string.Empty;

    /// <summary>Relevance score [0.0–1.0].</summary>
    public double RelevanceScore { get; set; }

    /// <summary>Faithfulness score [0.0–1.0].</summary>
    public double FaithfulnessScore { get; set; }

    /// <summary>Human-readable reasoning for the scores.</summary>
    public string Reasoning { get; set; } = string.Empty;

    /// <summary>UTC timestamp when the evaluation was produced.</summary>
    public DateTime EvaluatedAt { get; set; }

    /// <summary>Optional FK to the related RAG comparison history.</summary>
    public int? RelatedHistoryId { get; set; }
}
