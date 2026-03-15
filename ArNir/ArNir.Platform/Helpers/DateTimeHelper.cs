namespace ArNir.Platform.Helpers;

/// <summary>
/// Provides date and time utility methods used throughout the ArNir platform.
/// All methods operate in UTC unless explicitly stated otherwise.
/// </summary>
public static class DateTimeHelper
{
    /// <summary>
    /// Returns the current UTC timestamp as a <see cref="DateTimeOffset"/>.
    /// Prefer this over <see cref="DateTime.UtcNow"/> to retain timezone offset information.
    /// </summary>
    public static DateTimeOffset UtcNow => DateTimeOffset.UtcNow;

    /// <summary>
    /// Returns the current UTC timestamp as a Unix epoch timestamp in milliseconds.
    /// </summary>
    public static long UtcNowUnixMs => DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

    /// <summary>
    /// Converts a <see cref="DateTimeOffset"/> to a Unix epoch timestamp in milliseconds.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    public static long ToUnixMs(DateTimeOffset value) => value.ToUnixTimeMilliseconds();

    /// <summary>
    /// Converts a Unix epoch timestamp in milliseconds back to a UTC <see cref="DateTimeOffset"/>.
    /// </summary>
    /// <param name="unixMs">Milliseconds elapsed since 1970-01-01T00:00:00Z.</param>
    public static DateTimeOffset FromUnixMs(long unixMs) =>
        DateTimeOffset.FromUnixTimeMilliseconds(unixMs);

    /// <summary>
    /// Returns a human-readable relative description of how long ago <paramref name="past"/>
    /// occurred compared to now (e.g. <c>"3 minutes ago"</c>, <c>"2 days ago"</c>).
    /// </summary>
    /// <param name="past">The past point in time (UTC).</param>
    /// <returns>A relative time string.</returns>
    public static string ToRelativeTime(DateTimeOffset past)
    {
        var diff = DateTimeOffset.UtcNow - past;

        return diff.TotalSeconds switch
        {
            < 60      => "just now",
            < 3600    => $"{(int)diff.TotalMinutes} minute{Plural((int)diff.TotalMinutes)} ago",
            < 86400   => $"{(int)diff.TotalHours} hour{Plural((int)diff.TotalHours)} ago",
            < 2592000 => $"{(int)diff.TotalDays} day{Plural((int)diff.TotalDays)} ago",
            < 31536000 => $"{(int)(diff.TotalDays / 30)} month{Plural((int)(diff.TotalDays / 30))} ago",
            _         => $"{(int)(diff.TotalDays / 365)} year{Plural((int)(diff.TotalDays / 365))} ago"
        };
    }

    /// <summary>
    /// Calculates the elapsed milliseconds between <paramref name="start"/> and
    /// <paramref name="end"/>.
    /// </summary>
    /// <param name="start">The start time.</param>
    /// <param name="end">The end time.</param>
    /// <returns>Elapsed time in milliseconds (always non-negative).</returns>
    public static long ElapsedMs(DateTimeOffset start, DateTimeOffset end) =>
        Math.Abs((long)(end - start).TotalMilliseconds);

    /// <summary>
    /// Returns <see langword="true"/> if <paramref name="value"/> falls within a business-hours
    /// window (Monday–Friday, 09:00–17:00 UTC).
    /// </summary>
    /// <param name="value">The UTC timestamp to evaluate.</param>
    public static bool IsBusinessHours(DateTimeOffset value)
    {
        var utc = value.ToUniversalTime();
        return utc.DayOfWeek is not (DayOfWeek.Saturday or DayOfWeek.Sunday)
               && utc.Hour >= 9
               && utc.Hour < 17;
    }

    private static string Plural(int count) => count == 1 ? string.Empty : "s";
}
