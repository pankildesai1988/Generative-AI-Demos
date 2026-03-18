using ArNir.RAG.Interfaces;
using Microsoft.Extensions.Logging;

namespace ArNir.RAG.InProcess;

/// <summary>
/// No-op dev stub for <see cref="IDocumentEmbedder"/>.
/// Returns zero-length vectors for all inputs.
/// <para>
/// <b>Warning:</b> Replace with a real embedding provider (e.g., OpenAI text-embedding-ada-002)
/// before deploying to production. Using this stub means all stored vectors will be zero
/// and similarity search will return meaningless results.
/// </para>
/// </summary>
public sealed class NullDocumentEmbedder : IDocumentEmbedder
{
    private readonly ILogger<NullDocumentEmbedder> _logger;

    /// <summary>Initialises a new <see cref="NullDocumentEmbedder"/>.</summary>
    public NullDocumentEmbedder(ILogger<NullDocumentEmbedder> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc />
    /// <remarks>Returns an empty float array — no-op dev stub.</remarks>
    public Task<float[]> GenerateAsync(string text, string model)
    {
        _logger.LogDebug("[NullDocumentEmbedder] GenerateAsync called — no-op stub, returning empty vector.");
        return Task.FromResult(Array.Empty<float>());
    }

    /// <inheritdoc />
    /// <remarks>Returns one empty float array per input — no-op dev stub.</remarks>
    public Task<IReadOnlyList<float[]>> GenerateBatchAsync(IEnumerable<string> texts, string model)
    {
        _logger.LogDebug("[NullDocumentEmbedder] GenerateBatchAsync called — no-op stub, returning empty vectors.");
        IReadOnlyList<float[]> result = texts.Select(_ => Array.Empty<float>()).ToList();
        return Task.FromResult(result);
    }
}
