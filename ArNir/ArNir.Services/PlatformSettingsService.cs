using ArNir.Core.Entities;
using ArNir.Data;
using ArNir.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace ArNir.Services;

/// <summary>
/// EF Core + IMemoryCache implementation of <see cref="IPlatformSettingsService"/>.
/// Cache entries slide with a 5-minute expiry; a write (<see cref="SetAsync"/>) immediately
/// removes the relevant entry so the next read fetches the fresh DB value.
/// </summary>
public sealed class PlatformSettingsService : IPlatformSettingsService
{
    private readonly IDbContextFactory<ArNirDbContext> _dbFactory;
    private readonly IMemoryCache                      _cache;
    private readonly ILogger<PlatformSettingsService>  _logger;
    private static readonly TimeSpan CacheSliding = TimeSpan.FromMinutes(5);

    /// <summary>Initialises the service with an EF context factory and a memory cache.</summary>
    public PlatformSettingsService(
        IDbContextFactory<ArNirDbContext> dbFactory,
        IMemoryCache cache,
        ILogger<PlatformSettingsService> logger)
    {
        _dbFactory = dbFactory;
        _cache     = cache;
        _logger    = logger;
    }

    /// <inheritdoc />
    public async Task<string?> GetAsync(string module, string key, CancellationToken ct = default)
    {
        var cacheKey = CacheKey(module, key);
        if (_cache.TryGetValue(cacheKey, out string? cached))
            return cached;

        await using var db = await _dbFactory.CreateDbContextAsync(ct);
        var row = await db.PlatformSettings
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Module == module && x.Key == key, ct);

        var value = row?.Value;
        if (value != null)
        {
            _cache.Set(cacheKey, value, new MemoryCacheEntryOptions
            {
                SlidingExpiration = CacheSliding
            });
        }
        return value;
    }

    /// <inheritdoc />
    public async Task<T?> GetAsync<T>(string module, string key, CancellationToken ct = default)
    {
        var raw = await GetAsync(module, key, ct);
        if (raw == null) return default;
        try
        {
            return (T?)Convert.ChangeType(raw, typeof(T));
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "PlatformSettings [{Module}/{Key}] could not be parsed as {Type}.", module, key, typeof(T).Name);
            return default;
        }
    }

    /// <inheritdoc />
    public async Task SetAsync(string module, string key, string value, string? description = null, CancellationToken ct = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(ct);
        var row = await db.PlatformSettings
            .FirstOrDefaultAsync(x => x.Module == module && x.Key == key, ct);

        if (row == null)
        {
            db.PlatformSettings.Add(new PlatformSetting
            {
                Module      = module,
                Key         = key,
                Value       = value,
                Description = description,
                UpdatedAt   = DateTime.UtcNow
            });
        }
        else
        {
            row.Value       = value;
            row.UpdatedAt   = DateTime.UtcNow;
            if (description != null) row.Description = description;
        }

        await db.SaveChangesAsync(ct);
        _cache.Remove(CacheKey(module, key));
        _logger.LogDebug("PlatformSettings [{Module}/{Key}] updated to '{Value}'.", module, key, value);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<(string Key, string Value)>> GetModuleSettingsAsync(string module, CancellationToken ct = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(ct);
        var rows = await db.PlatformSettings
            .AsNoTracking()
            .Where(x => x.Module == module)
            .OrderBy(x => x.Key)
            .Select(x => new { x.Key, x.Value })
            .ToListAsync(ct);

        return rows.Select(r => (r.Key, r.Value)).ToList();
    }

    private static string CacheKey(string module, string key) => $"ps:{module}:{key}";
}
