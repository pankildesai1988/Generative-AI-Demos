using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace ArNir.Data;

/// <summary>
/// Design-time factory used by EF Core CLI tools (<c>dotnet ef migrations add</c>,
/// <c>dotnet ef database update</c>) to create an <see cref="ArNirDbContext"/> without
/// running the full ASP.NET host.
///
/// <para>
/// <b>Why this class exists — IDesignTimeDbContextFactory&lt;T&gt; explained:</b><br/>
/// EF Core CLI tools need a <see cref="ArNirDbContext"/> instance at design time (before
/// the app is running) to compare the current model against the database schema and
/// generate/apply migrations. Without this factory, tools fail with
/// <c>"Unable to resolve service for DbContextOptions"</c> because there is no host to
/// provide the registered services.
/// </para>
///
/// <para>
/// <b>Why tables appeared in localdb instead of the real database:</b><br/>
/// The original version hard-coded a localdb placeholder connection string. When you ran
/// <c>dotnet ef database update</c>, EF used that placeholder and created tables in
/// <c>(localdb)\mssqllocaldb</c> instead of the real SQL Server instance. This version
/// fixes that by reading <c>ConnectionStrings:DefaultConnection</c> from
/// <c>../ArNir.Admin/appsettings.json</c> at design time, so the migration runs against
/// <c>Server=localhost;Database=ArNir</c>. The localdb string is kept as a fallback only
/// for CI environments where ArNir.Admin/appsettings.json is not present.
/// </para>
/// </summary>
public class ArNirDbContextFactory : IDesignTimeDbContextFactory<ArNirDbContext>
{
    /// <inheritdoc />
    public ArNirDbContext CreateDbContext(string[] args)
    {
        // ── Locate ArNir.Admin/appsettings.json relative to ArNir.Data project ──
        // When running from the ArNir.Data project directory, "..\ArNir.Admin" resolves
        // to the sibling Admin project which holds the real connection string.
        var adminAppsettingsDir = Path.GetFullPath(
            Path.Combine(Directory.GetCurrentDirectory(), "..", "ArNir.Admin"));

        var config = new ConfigurationBuilder()
            .SetBasePath(adminAppsettingsDir)
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        // Prefer real DB connection; fall back to localdb only when appsettings is absent
        var connStr = config.GetConnectionString("DefaultConnection")
            ?? "Server=(localdb)\\mssqllocaldb;Database=ArNirDev;Trusted_Connection=True;TrustServerCertificate=True;";

        var optionsBuilder = new DbContextOptionsBuilder<ArNirDbContext>();
        optionsBuilder.UseSqlServer(connStr);

        return new ArNirDbContext(optionsBuilder.Options);
    }
}
