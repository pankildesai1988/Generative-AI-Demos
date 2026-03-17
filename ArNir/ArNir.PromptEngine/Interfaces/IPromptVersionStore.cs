using ArNir.PromptEngine.Models;

namespace ArNir.PromptEngine.Interfaces;

/// <summary>
/// Defines the contract for persisting and retrieving versioned <see cref="PromptTemplate"/> records.
/// <para>
/// This interface represents <b>Layer 1 (Database)</b> of the 3-layer prompt resolution chain
/// (Database → Config → Code). Implementations are responsible for durable storage and retrieval
/// of prompt templates, enabling runtime updates without redeployment.
/// </para>
/// <para>
/// <b>NOT registered by default</b> in the DI container — this is an infrastructure concern.
/// The consuming application must provide a concrete implementation (e.g. EF Core, Dapper, Redis)
/// and register it against this interface in its service configuration.
/// </para>
/// </summary>
public interface IPromptVersionStore
{
    /// <summary>
    /// Retrieves the most recent active <see cref="PromptTemplate"/> for the given <paramref name="style"/>.
    /// </summary>
    /// <param name="style">The prompt style key to look up (e.g. <c>rag</c>, <c>few-shot</c>).</param>
    /// <param name="ct">A cancellation token.</param>
    /// <returns>
    /// The latest active <see cref="PromptTemplate"/> for the style, or <c>null</c> if none exists.
    /// </returns>
    Task<PromptTemplate?> GetByStyleAsync(string style, CancellationToken ct = default);

    /// <summary>
    /// Persists a new <see cref="PromptTemplate"/> to the store.
    /// If a template with the same <see cref="PromptTemplate.Style"/> already exists, the store
    /// should increment the <see cref="PromptTemplate.Version"/> and retain the previous record for history.
    /// </summary>
    /// <param name="template">The template to save.</param>
    /// <param name="ct">A cancellation token.</param>
    Task SaveAsync(PromptTemplate template, CancellationToken ct = default);

    /// <summary>
    /// Returns the full version history of templates for the given <paramref name="style"/>,
    /// ordered from oldest to newest.
    /// </summary>
    /// <param name="style">The prompt style key whose history to retrieve.</param>
    /// <param name="ct">A cancellation token.</param>
    /// <returns>
    /// A read-only list of all <see cref="PromptTemplate"/> versions for the style,
    /// ordered by <see cref="PromptTemplate.Version"/> ascending.
    /// </returns>
    Task<IReadOnlyList<PromptTemplate>> GetHistoryAsync(string style, CancellationToken ct = default);
}
