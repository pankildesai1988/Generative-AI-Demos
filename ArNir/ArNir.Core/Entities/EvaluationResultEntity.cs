using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ArNir.Core.Entities;

/// <summary>
/// Persisted evaluation result produced by <c>IEvaluationService</c> for RAG quality scoring.
/// Each row captures Relevance and Faithfulness scores (0.0–1.0) plus a reasoning summary.
/// </summary>
[Table("EvaluationResults")]
public class EvaluationResultEntity
{
    /// <summary>Auto-incremented surrogate key.</summary>
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    /// <summary>The original user question that was evaluated.</summary>
    [MaxLength(2000)]
    public string Question { get; set; } = string.Empty;

    /// <summary>The AI-generated answer that was evaluated.</summary>
    public string Answer { get; set; } = string.Empty;

    /// <summary>The RAG context (retrieved chunks) used to ground the answer.</summary>
    public string Context { get; set; } = string.Empty;

    /// <summary>Relevance score [0.0–1.0]: does the answer address what was asked?</summary>
    public double RelevanceScore { get; set; }

    /// <summary>Faithfulness score [0.0–1.0]: is the answer grounded in the context?</summary>
    public double FaithfulnessScore { get; set; }

    /// <summary>Human-readable explanation of how both scores were derived.</summary>
    public string Reasoning { get; set; } = string.Empty;

    /// <summary>UTC timestamp when this evaluation was produced.</summary>
    public DateTime EvaluatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Optional FK to the RAG comparison history entry that triggered this evaluation.</summary>
    public int? RelatedHistoryId { get; set; }

    /// <summary>Navigation property to the related RAG comparison history.</summary>
    [ForeignKey(nameof(RelatedHistoryId))]
    public RagComparisonHistory? RelatedHistory { get; set; }
}
