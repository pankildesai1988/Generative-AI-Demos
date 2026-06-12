namespace ArNir.Core.Interfaces;

/// <summary>
/// The effective chunking parameters for one extraction call.
/// </summary>
/// <param name="ChunkSize">Target chunk size in characters.</param>
/// <param name="ChunkOverlap">Overlap budget in characters between consecutive chunks.</param>
/// <param name="Strategy">Chunking strategy name (<c>"sentence"</c> or <c>"sliding"</c>).</param>
public sealed record ChunkingOptions(int ChunkSize, int ChunkOverlap, string Strategy);

/// <summary>
/// Resolves the chunking parameters at extraction time, realising the 3-layer config precedence
/// (PlatformSettings DB &gt; appsettings <c>Rag</c> section &gt; code constants). Implemented in
/// <c>ArNir.Services</c> over <c>IPlatformSettingsService</c>; the unified chunk extractor in
/// <c>ArNir.RAG</c> consumes it through this Core interface so the DB layer applies per call —
/// including the strategy choice, which would otherwise be frozen by the singleton DI selection.
/// </summary>
public interface IChunkingOptionsResolver
{
    /// <summary>Returns the effective chunk size, overlap, and strategy for the current call.</summary>
    /// <param name="ct">Cancellation token.</param>
    Task<ChunkingOptions> ResolveAsync(CancellationToken ct = default);
}
