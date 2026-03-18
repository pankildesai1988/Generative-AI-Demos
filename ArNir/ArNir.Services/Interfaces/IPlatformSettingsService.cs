namespace ArNir.Services.Interfaces;

/// <summary>
/// Provides cached read/write access to the <c>PlatformSettings</c> DB table.
/// All module-level configuration (chunk sizes, AI defaults, SLA thresholds, etc.)
/// flows through this service so operators can change values at runtime via ArNir.Admin
/// without redeployment.
/// </summary>
public interface IPlatformSettingsService
{
    /// <summary>Returns the raw string value for <paramref name="module"/>/<paramref name="key"/>, or <c>null</c> if not found.</summary>
    Task<string?> GetAsync(string module, string key, CancellationToken ct = default);

    /// <summary>
    /// Returns the value for <paramref name="module"/>/<paramref name="key"/> parsed to
    /// <typeparamref name="T"/> via <see cref="Convert.ChangeType(object, Type)"/>.
    /// Returns <c>default(T)</c> when the key is missing or the value cannot be parsed.
    /// </summary>
    Task<T?> GetAsync<T>(string module, string key, CancellationToken ct = default);

    /// <summary>
    /// Upserts the value for <paramref name="module"/>/<paramref name="key"/> and invalidates
    /// the cache entry so the next <see cref="GetAsync"/> call returns the fresh value.
    /// </summary>
    Task SetAsync(string module, string key, string value, string? description = null, CancellationToken ct = default);

    /// <summary>Returns all key/value pairs that belong to <paramref name="module"/>.</summary>
    Task<IReadOnlyList<(string Key, string Value)>> GetModuleSettingsAsync(string module, CancellationToken ct = default);
}
