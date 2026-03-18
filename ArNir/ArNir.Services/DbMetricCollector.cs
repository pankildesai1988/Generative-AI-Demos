using ArNir.Core.Entities;
using ArNir.Data;
using ArNir.Observability.Interfaces;
using ArNir.Observability.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace ArNir.Services;

/// <summary>
/// EF Core–backed implementation of <see cref="IMetricCollector"/> that persists every
/// <see cref="MetricEvent"/> to the <c>MetricEvents</c> SQL Server table.
/// <para>
/// Provides time-range and provider-scoped queries for the
/// <c>ObservabilityDashboardController</c> in ArNir.Admin.
/// </para>
/// </summary>
public sealed class DbMetricCollector : IMetricCollector
{
    private readonly IDbContextFactory<ArNirDbContext> _dbFactory;
    private readonly ILogger<DbMetricCollector>        _logger;

    /// <summary>Initialises the collector with an EF context factory.</summary>
    public DbMetricCollector(
        IDbContextFactory<ArNirDbContext> dbFactory,
        ILogger<DbMetricCollector> logger)
    {
        _dbFactory = dbFactory;
        _logger    = logger;
    }

    /// <inheritdoc />
    public async Task RecordAsync(MetricEvent metricEvent, CancellationToken ct = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(ct);
        db.MetricEvents.Add(new MetricEventEntity
        {
            EventType   = metricEvent.EventType,
            Provider    = metricEvent.Provider,
            Model       = metricEvent.Model,
            LatencyMs   = metricEvent.LatencyMs,
            IsWithinSla = metricEvent.IsWithinSla,
            TokensUsed  = metricEvent.TokensUsed,
            OccurredAt  = metricEvent.OccurredAt,
            TagsJson    = metricEvent.Tags.Count > 0
                ? JsonSerializer.Serialize(metricEvent.Tags)
                : null
        });
        await db.SaveChangesAsync(ct);
        _logger.LogDebug("MetricEvent recorded: {EventType} | {Provider}/{Model} | {LatencyMs}ms | SLA={IsWithinSla}.",
            metricEvent.EventType, metricEvent.Provider, metricEvent.Model,
            metricEvent.LatencyMs, metricEvent.IsWithinSla);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<MetricEvent>> QueryAsync(
        string?   provider = null,
        DateTime? start    = null,
        DateTime? end      = null,
        CancellationToken ct = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(ct);
        var query = db.MetricEvents.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(provider))
            query = query.Where(x => x.Provider == provider);
        if (start.HasValue)
            query = query.Where(x => x.OccurredAt >= start.Value);
        if (end.HasValue)
            query = query.Where(x => x.OccurredAt <= end.Value);

        var rows = await query.OrderBy(x => x.OccurredAt).ToListAsync(ct);

        return rows.Select(r => new MetricEvent
        {
            EventType   = r.EventType,
            Provider    = r.Provider,
            Model       = r.Model,
            LatencyMs   = r.LatencyMs,
            IsWithinSla = r.IsWithinSla,
            TokensUsed  = r.TokensUsed,
            OccurredAt  = r.OccurredAt,
            Tags        = r.TagsJson != null
                ? JsonSerializer.Deserialize<Dictionary<string, string>>(r.TagsJson) ?? new()
                : new()
        }).ToList();
    }
}
