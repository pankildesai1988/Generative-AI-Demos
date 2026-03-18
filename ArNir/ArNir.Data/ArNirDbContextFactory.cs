using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ArNir.Data;

/// <summary>
/// Design-time factory used by EF Core CLI tools (<c>dotnet ef migrations add</c>, <c>database update</c>)
/// to create an <see cref="ArNirDbContext"/> without a running ASP.NET host.
/// </summary>
public class ArNirDbContextFactory : IDesignTimeDbContextFactory<ArNirDbContext>
{
    /// <inheritdoc />
    public ArNirDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ArNirDbContext>();

        // Use a placeholder connection string for migrations scaffolding only.
        // Actual runtime connection string is supplied via appsettings / environment variables.
        optionsBuilder.UseSqlServer(
            "Server=(localdb)\\mssqllocaldb;Database=ArNirDev;Trusted_Connection=True;");

        return new ArNirDbContext(optionsBuilder.Options);
    }
}
