using ArNir.Core.Entities;
using ArNir.Data;
using ArNir.PromptEngine.Interfaces;
using ArNir.PromptEngine.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ArNir.Services;

/// <summary>
/// EF Core–backed implementation of <see cref="IPromptVersionStore"/> (Layer 1 of the
/// 3-layer prompt resolution chain: Database → Config → Code).
/// <para>
/// Queries the <c>PromptTemplates</c> table; the result with the highest
/// <see cref="PromptTemplateEntity.Version"/> for the requested style is returned.
/// </para>
/// </summary>
public sealed class DbPromptVersionStore : IPromptVersionStore
{
    private readonly IDbContextFactory<ArNirDbContext>  _dbFactory;
    private readonly ILogger<DbPromptVersionStore>      _logger;

    /// <summary>Initialises the store with an EF context factory.</summary>
    public DbPromptVersionStore(
        IDbContextFactory<ArNirDbContext> dbFactory,
        ILogger<DbPromptVersionStore> logger)
    {
        _dbFactory = dbFactory;
        _logger    = logger;
    }

    /// <inheritdoc />
    public async Task<PromptTemplate?> GetByStyleAsync(string style, CancellationToken ct = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(ct);
        var entity = await db.PromptTemplates
            .AsNoTracking()
            .Where(x => x.Style == style && x.IsActive)
            .OrderByDescending(x => x.Version)
            .FirstOrDefaultAsync(ct);

        if (entity == null)
        {
            _logger.LogDebug("DbPromptVersionStore: no active template found for style '{Style}'.", style);
            return null;
        }

        return Map(entity);
    }

    /// <inheritdoc />
    public async Task SaveAsync(PromptTemplate template, CancellationToken ct = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(ct);

        // Determine the next version number for this style
        var maxVersion = await db.PromptTemplates
            .Where(x => x.Style == template.Style)
            .Select(x => (int?)x.Version)
            .MaxAsync(ct) ?? 0;

        db.PromptTemplates.Add(new PromptTemplateEntity
        {
            Id           = template.Id == Guid.Empty ? Guid.NewGuid() : template.Id,
            Style        = template.Style,
            Name         = template.Name,
            TemplateText = template.TemplateText,
            Version      = maxVersion + 1,
            IsActive     = template.IsActive,
            Source       = template.Source.ToString(),
            CreatedAt    = DateTime.UtcNow
        });

        await db.SaveChangesAsync(ct);
        _logger.LogInformation("DbPromptVersionStore: saved template '{Name}' (style={Style}, v{Version}).",
            template.Name, template.Style, maxVersion + 1);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<PromptTemplate>> GetHistoryAsync(string style, CancellationToken ct = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(ct);
        var entities = await db.PromptTemplates
            .AsNoTracking()
            .Where(x => x.Style == style)
            .OrderBy(x => x.Version)
            .ToListAsync(ct);

        return entities.Select(Map).ToList();
    }

    // ── helpers ──────────────────────────────────────────────────────────────

    private static PromptTemplate Map(PromptTemplateEntity e) => new()
    {
        Id           = e.Id,
        Name         = e.Name,
        Style        = e.Style,
        TemplateText = e.TemplateText,
        Version      = e.Version,
        IsActive     = e.IsActive,
        Source       = Enum.TryParse<PromptSource>(e.Source, out var src) ? src : PromptSource.Database,
        CreatedAt    = e.CreatedAt
    };
}
