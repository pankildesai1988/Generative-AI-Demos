namespace ArNir.Observability.Models;

/// <summary>
/// Holds the scores and reasoning produced by <c>IEvaluationService.EvaluateAsync</c>
/// for a single question-answer-context triple.
/// <para>
/// All scores are normalised to the range <c>[0.0, 1.0]</c>:
/// <list type="bullet">
///   <item><c>0.0</c> — completely irrelevant or unfaithful.</item>
///   <item><c>1.0</c> — perfectly relevant and faithful.</item>
/// </list>
/// </para>
/// </summary>
public sealed class EvaluationResult
{
    /// <summary>
    /// Gets or sets how relevant the answer is to the question, on a scale of <c>0.0</c>–<c>1.0</c>.
    /// A high score means the answer directly addresses what was asked.
    /// </summary>
    public double RelevanceScore { get; set; }

    /// <summary>
    /// Gets or sets how faithful the answer is to the provided context, on a scale of <c>0.0</c>–<c>1.0</c>.
    /// A high score means the answer is grounded in the supplied context and does not hallucinate.
    /// </summary>
    public double FaithfulnessScore { get; set; }

    /// <summary>
    /// Gets or sets a human-readable explanation of how both scores were derived.
    /// Implementations should summarise the key factors that raised or lowered each score.
    /// </summary>
    public string Reasoning { get; set; } = string.Empty;

    /// <summary>Gets or sets the UTC timestamp at which this evaluation was produced.</summary>
    public DateTime EvaluatedAt { get; set; } = DateTime.UtcNow;
}
