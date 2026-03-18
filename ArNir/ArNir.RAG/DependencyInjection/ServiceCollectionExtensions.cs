using ArNir.RAG.Chunking;
using ArNir.RAG.Hosting;
using ArNir.RAG.InProcess;
using ArNir.RAG.Interfaces;
using ArNir.RAG.Parsing;
using ArNir.RAG.Pipeline;
using Microsoft.Extensions.DependencyInjection;

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
    /// and <see cref="SlidingWindowChunker"/> (as <see cref="IDocumentChunker"/>).
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
        // Parsers — Singleton (stateless, safe to share)
        services.AddSingleton<IDocumentParser, PdfDocumentParser>();
        services.AddSingleton<IDocumentParser, DocxDocumentParser>();
        services.AddSingleton<IDocumentParser, PlainTextDocumentParser>();

        // Chunker — Singleton (stateless)
        services.AddSingleton<IDocumentChunker, SlidingWindowChunker>();

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
