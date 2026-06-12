using ArNir.Core.Interfaces;
using ArNir.Platform.Configuration;
using ArNir.Services.Interfaces;
using Microsoft.Extensions.Options;

namespace ArNir.Services;

/// <summary>
/// Resolves chunking options through the 3-layer config precedence: PlatformSettings DB row
/// (module <c>RAG</c>, keys <c>ChunkSize</c> / <c>ChunkOverlap</c> / <c>ChunkingStrategy</c>)
/// &gt; appsettings <c>Rag</c> section (<see cref="RagSettings"/>) &gt; code constants
/// (baked into the <see cref="RagSettings"/> property defaults).
/// Registered in the composition roots (Admin/API) so the unified chunk extractor in
/// <c>ArNir.RAG</c> picks up DB values per call without referencing <c>ArNir.Services</c>.
/// </summary>
public sealed class PlatformChunkingOptionsResolver : IChunkingOptionsResolver
{
    private readonly IPlatformSettingsService _settings;
    private readonly RagSettings _ragSettings;

    /// <summary>
    /// Initialises a new instance of <see cref="PlatformChunkingOptionsResolver"/>.
    /// </summary>
    /// <param name="settings">The cached PlatformSettings DB accessor.</param>
    /// <param name="ragOptions">Appsettings-bound RAG options used as the fallback layer.</param>
    public PlatformChunkingOptionsResolver(
        IPlatformSettingsService settings,
        IOptions<RagSettings>? ragOptions = null)
    {
        _settings    = settings;
        _ragSettings = ragOptions?.Value ?? new RagSettings();
    }

    /// <inheritdoc />
    public async Task<ChunkingOptions> ResolveAsync(CancellationToken ct = default)
        => new(
            await _settings.GetOrDefaultAsync("RAG", "ChunkSize",        _ragSettings.ChunkSize, ct),
            await _settings.GetOrDefaultAsync("RAG", "ChunkOverlap",     _ragSettings.ChunkOverlap, ct),
            await _settings.GetOrDefaultAsync("RAG", "ChunkingStrategy", _ragSettings.ChunkingStrategy, ct));
}
