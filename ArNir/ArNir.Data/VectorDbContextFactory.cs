using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace ArNir.Data;

/// <summary>
/// Design-time factory used by EF Core CLI tools (<c>dotnet ef migrations add</c>,
/// <c>dotnet ef database update --context VectorDbContext</c>) to create a
/// <see cref="VectorDbContext"/> without running the full ASP.NET host.
///
/// <para>
/// <b>Why this class exists:</b> EF Core CLI tools need a <see cref="VectorDbContext"/>
/// instance at design time to apply Postgres/pgvector migrations (e.g. the HNSW index).
/// Without it the tools fail with
/// <c>"Unable to resolve service for type 'DbContextOptions&lt;VectorDbContext&gt;'"</c>
/// because there is no running host to supply the registered options.
/// </para>
///
/// <para>
/// Mirrors <see cref="ArNirDbContextFactory"/>: reads
/// <c>ConnectionStrings:Postgres</c> from <c>../ArNir.Admin/appsettings.json</c> and
/// configures Npgsql with pgvector (<c>UseVector()</c>) so the <c>vector(1536)</c> column
/// and the <c>vector_cosine_ops</c> HNSW index resolve correctly. A localhost fallback is
/// kept for CI environments where ArNir.Admin/appsettings.json is absent.
/// </para>
/// </summary>
public class VectorDbContextFactory : IDesignTimeDbContextFactory<VectorDbContext>
{
    /// <inheritdoc />
    public VectorDbContext CreateDbContext(string[] args)
    {
        var adminAppsettingsDir = Path.GetFullPath(
            Path.Combine(Directory.GetCurrentDirectory(), "..", "ArNir.Admin"));

        var config = new ConfigurationBuilder()
            .SetBasePath(adminAppsettingsDir)
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connStr = config.GetConnectionString("Postgres")
            ?? "Host=localhost:5432;Database=arnir;Username=postgres;Password=postgres;";

        var optionsBuilder = new DbContextOptionsBuilder<VectorDbContext>();
        optionsBuilder.UseNpgsql(connStr,
            npgsqlOptions => npgsqlOptions
                .MigrationsAssembly("ArNir.Data")
                .UseVector());

        return new VectorDbContext(optionsBuilder.Options);
    }
}
