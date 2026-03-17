namespace ArNir.RAG.Interfaces;

/// <summary>
/// Defines a contract for generating dense vector embeddings from text.
/// <para>
/// CRITICAL: This interface is named <c>IDocumentEmbedder</c> — NOT <c>IEmbeddingService</c>.
/// <c>IEmbeddingService</c> already exists in <c>ArNir.Services</c> with different method signatures;
/// renaming or merging these would cause interface conflicts.
/// </para>
/// </summary>
public interface IDocumentEmbedder
{
    /// <summary>
    /// Generates a single embedding vector for the given text.
    /// </summary>
    /// <param name="text">The input text to embed.</param>
    /// <param name="model">The embedding model identifier (e.g. "text-embedding-ada-002").</param>
    /// <returns>A float array representing the dense embedding vector.</returns>
    Task<float[]> GenerateAsync(string text, string model);

    /// <summary>
    /// Generates embedding vectors for a batch of text strings in a single API call.
    /// </summary>
    /// <param name="texts">The collection of input texts to embed.</param>
    /// <param name="model">The embedding model identifier.</param>
    /// <returns>A read-only list of float arrays, one per input text, in the same order.</returns>
    Task<IReadOnlyList<float[]>> GenerateBatchAsync(IEnumerable<string> texts, string model);
}
