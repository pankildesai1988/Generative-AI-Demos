using ArNir.RAG.Interfaces;
using ArNir.RAG.Pgvector;
using Microsoft.Extensions.DependencyInjection;

namespace ArNir.RAG.Pgvector.DependencyInjection;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers pgvector-backed implementations of IDocumentEmbedder and IDocumentVectorStore.
    /// Call AFTER AddArNirRAG() so these Scoped registrations override the Singleton null stubs.
    /// </summary>
    public static IServiceCollection AddArNirRagPgvector(this IServiceCollection services)
    {
        services.AddScoped<IDocumentEmbedder, PgvectorDocumentEmbedder>();
        services.AddScoped<IDocumentVectorStore, PgvectorDocumentVectorStore>();
        return services;
    }
}
