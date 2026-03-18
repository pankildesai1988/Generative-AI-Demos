namespace ArNir.Core.Interfaces;

/// <summary>
/// Generates a dense embedding vector for a single text string.
/// Placed in ArNir.Core so both ArNir.Services and ArNir.RAG.Pgvector can
/// reference it without a circular dependency.
/// </summary>
public interface IEmbeddingProvider
{
    Task<float[]> GenerateEmbeddingAsync(string text, string model);
}
