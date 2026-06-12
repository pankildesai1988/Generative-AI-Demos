using ArNir.Core.Interfaces;
using ArNir.Platform.Configuration;
using ArNir.Platform.Constants;
using ArNir.RAG.Chunking;
using ArNir.RAG.Extraction;
using ArNir.RAG.Hosting;
using ArNir.RAG.InProcess;
using ArNir.RAG.Interfaces;
using ArNir.RAG.Parsing;
using ArNir.RAG.Pipeline;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace ArNir.RAG.DependencyInjection;

/// <summary>
/// Provides extension methods for registering ArNir.RAG services into an
/// <see cref="IServiceCollection"/>.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers all ArNir.RAG pipeline services into the dependency-injection container.
    /// <para>
    /// Registered as <b>Singleton</b>: <see cref="PdfDocumentParser"/>,
    /// <see cref="DocxDocumentParser"/>, <see cref="PlainTextDocumentParser"/> (all as <see cref="IDocumentParser"/>),
    /// and both chunkers (<see cref="SlidingWindowChunker"/>, <see cref="SentenceAwareChunker"/>) —
    /// the active <see cref="IDocumentChunker"/> is selected from <c>Rag:ChunkingStrategy</c>.
    /// </para>
    /// <para>
    /// Registered as <b>Scoped</b>: <see cref="IngestionPipeline"/> (as <see cref="IIngestionPipeline"/>).
    /// </para>
    /// <para>
    /// <b>NOT registered</b>: <see cref="IDocumentEmbedder"/> and <see cref="IDocumentVectorStore"/> —
    /// these are infrastructure concerns provided by the consuming application.
    /// </para>
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <returns>The same <see cref="IServiceCollection"/> instance for chaining.</returns>
    public static IServiceCollection AddArNirRAG(this IServiceCollection services)
    {
        // Table detector — Singleton (stateless heuristic over page word boxes)
        services.AddSingleton<TableExtractor>();

        // Parsers — Singleton (stateless, safe to share)
        services.AddSingleton<IDocumentParser, PdfDocumentParser>();
        services.AddSingleton<IDocumentParser, DocxDocumentParser>();
        services.AddSingleton<IDocumentParser, PlainTextDocumentParser>();

        // Chunkers — Singleton (stateless). The active IDocumentChunker is selected at startup
        // from Rag:ChunkingStrategy ("sliding" | "sentence"); unknown values fall back to sliding.
        services.AddSingleton<SlidingWindowChunker>();
        services.AddSingleton<SentenceAwareChunker>();
        services.AddSingleton<IDocumentChunker>(sp =>
        {
            var strategy = sp.GetRequiredService<IOptions<RagSettings>>().Value.ChunkingStrategy;

            return string.Equals(strategy, ApplicationConstants.ChunkingStrategySentence, StringComparison.OrdinalIgnoreCase)
                ? sp.GetRequiredService<SentenceAwareChunker>()
                : sp.GetRequiredService<SlidingWindowChunker>();
        });

        // Unified chunk extractor — Scoped so the optional IChunkingOptionsResolver (a scoped
        // DB-backed service registered by the composition roots) can be injected per request.
        // Both ingestion paths (DocumentService SQL save + IngestionPipeline embedding) must use
        // this one component so their chunk sequences stay identical.
        services.AddScoped<IUnifiedChunkExtractor, UnifiedChunkExtractor>();

        // Embedder & vector store — no-op dev stubs (Singleton).
        // Replace NullDocumentEmbedder / NullDocumentVectorStore with real implementations before production.
        services.AddSingleton<IDocumentEmbedder, NullDocumentEmbedder>();
        services.AddSingleton<IDocumentVectorStore, NullDocumentVectorStore>();

        // Pipeline — Scoped (may depend on scoped infrastructure services)
        services.AddScoped<IIngestionPipeline, IngestionPipeline>();

        return services;
    }

    /// <summary>
    /// Registers the background ingestion queue and worker.
    /// </summary>
    public static IServiceCollection AddArNirRAGBackgroundIngestion(this IServiceCollection services)
    {
        services.AddSingleton<IngestionQueue>();
        services.AddHostedService<IngestionWorker>();
        return services;
    }
}
