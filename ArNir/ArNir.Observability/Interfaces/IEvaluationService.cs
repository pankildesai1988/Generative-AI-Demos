using ArNir.Observability.Models;

namespace ArNir.Observability.Interfaces;

/// <summary>
/// Defines the contract for evaluating the quality of an AI-generated answer against
/// a question and supporting context.
/// <para>
/// Evaluation is performed along two independent dimensions:
/// <list type="bullet">
///   <item><term>Relevance</term><description>Does the answer address what was asked?</description></item>
///   <item><term>Faithfulness</term><description>Is the answer grounded in the provided context without hallucination?</description></item>
/// </list>
/// </para>
/// <para>
/// Implementations may use heuristics, LLM-as-judge patterns, or dedicated evaluation
/// frameworks (e.g. RAGAS). The result is captured in an <see cref="EvaluationResult"/>,
/// which can be forwarded to <c>IMetricCollector</c> for trending.
/// </para>
/// </summary>
public interface IEvaluationService
{
    /// <summary>
    /// Evaluates the quality of <paramref name="answer"/> with respect to
    /// <paramref name="question"/> and <paramref name="context"/>.
    /// </summary>
    /// <param name="question">
    /// The original user question or prompt that the answer was generated for.
    /// </param>
    /// <param name="answer">
    /// The AI-generated answer to evaluate.
    /// </param>
    /// <param name="context">
    /// The supporting context (e.g. retrieved RAG chunks) that the answer should be
    /// grounded in. Pass an empty string when no context is available; faithfulness
    /// scoring will reflect the absence of a ground-truth context.
    /// </param>
    /// <param name="ct">A cancellation token.</param>
    /// <returns>
    /// An <see cref="EvaluationResult"/> containing normalised scores in <c>[0.0, 1.0]</c>
    /// and a plain-language <see cref="EvaluationResult.Reasoning"/> summary.
    /// </returns>
    Task<EvaluationResult> EvaluateAsync(
        string question,
        string answer,
        string context,
        CancellationToken ct = default);
}
