using ArNir.Core.Interfaces;
using ArNir.RAG.Interfaces;
using Microsoft.Extensions.Logging;

namespace ArNir.RAG.Pgvector;

/// <summary>
/// Real implementation of <see cref="IDocumentEmbedder"/> that calls the OpenAI embeddings API
/// via <see cref="IEmbeddingProvider"/> and returns dense float vectors.
/// <para>
/// Replaces the dev-only <c>NullDocumentEmbedder</c> stub that was registered by
/// <c>AddArNirRAG()</c>. This implementation is registered as <b>Scoped</b> in
/// <c>ArNir.Admin/Program.cs</c> after <c>AddArNirRAG()</c> so it takes precedence.
/// </para>
/// </summary>
public sealed class PgvectorDocumentEmbedder : IDocumentEmbedder
{
    private readonly IEmbeddingProvider          _provider;
    private readonly ILogger<PgvectorDocumentEmbedder> _logger;

    /// <summary>
    /// Initialises a new instance of <see cref="PgvectorDocumentEmbedder"/>.
    /// </summary>
    /// <param name="provider">The embedding provider that calls the OpenAI API.</param>
    /// <param name="logger">Logger for diagnostic output.</param>
    public PgvectorDocumentEmbedder(
        IEmbeddingProvider provider,
        ILogger<PgvectorDocumentEmbedder> logger)
    {
        _provider = provider;
        _logger   = logger;
    }

    /// <inheritdoc />
    public async Task<float[]> GenerateAsync(string text, string model)
    {
        var results = await GenerateBatchAsync(new[] { text }, model);
        return results[0];
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<float[]>> GenerateBatchAsync(IEnumerable<string> texts, string model)
    {
        var textList = texts.ToList();
        var results  = new List<float[]>(textList.Count);

        _logger.LogInformation(
            "Generating {Count} embeddings using model '{Model}' via OpenAI.",
            textList.Count, model);

        foreach (var text in textList)
        {
            var vector = await _provider.GenerateEmbeddingAsync(text, model);
            results.Add(vector);
        }

        _logger.LogInformation(
            "Successfully generated {Count} embeddings (dim={Dim}).",
            results.Count, results.Count > 0 ? results[0].Length : 0);

        return results;
    }
}
