using System.Text;
using System.Text.RegularExpressions;

namespace ArNir.Platform.Extensions;

/// <summary>
/// Extension methods for <see cref="string"/> that provide common text-processing utilities
/// used throughout the ArNir platform.
/// </summary>
public static class StringExtensions
{
    /// <summary>
    /// Returns <see langword="true"/> if the string is <see langword="null"/>, empty, or consists
    /// only of white-space characters.
    /// </summary>
    /// <param name="value">The string to test.</param>
    public static bool IsNullOrWhiteSpace(this string? value) =>
        string.IsNullOrWhiteSpace(value);

    /// <summary>
    /// Truncates the string to at most <paramref name="maxLength"/> characters, appending
    /// <paramref name="suffix"/> when truncation occurs.
    /// </summary>
    /// <param name="value">The source string.</param>
    /// <param name="maxLength">Maximum number of characters to retain (inclusive of suffix).</param>
    /// <param name="suffix">String appended when truncation occurs. Defaults to <c>"..."</c>.</param>
    /// <returns>The original string if its length is within <paramref name="maxLength"/>;
    /// otherwise a truncated copy with <paramref name="suffix"/> appended.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="maxLength"/> is less than the length of <paramref name="suffix"/>.
    /// </exception>
    public static string Truncate(this string value, int maxLength, string suffix = "...")
    {
        if (string.IsNullOrEmpty(value)) return value;
        if (maxLength < suffix.Length)
            throw new ArgumentOutOfRangeException(nameof(maxLength),
                $"maxLength ({maxLength}) must be >= suffix length ({suffix.Length}).");

        return value.Length <= maxLength
            ? value
            : string.Concat(value.AsSpan(0, maxLength - suffix.Length), suffix);
    }

    /// <summary>
    /// Converts a string to Title Case using the current culture
    /// (e.g. <c>"hello world"</c> → <c>"Hello World"</c>).
    /// </summary>
    /// <param name="value">The source string.</param>
    public static string ToTitleCase(this string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return value;
        var sb = new StringBuilder(value.Length);
        bool capitalise = true;
        foreach (char c in value)
        {
            sb.Append(capitalise ? char.ToUpperInvariant(c) : c);
            capitalise = char.IsWhiteSpace(c);
        }
        return sb.ToString();
    }

    /// <summary>
    /// Removes all excess white-space from a string, collapsing interior runs of whitespace
    /// to a single space and trimming leading/trailing whitespace.
    /// </summary>
    /// <param name="value">The source string.</param>
    public static string NormaliseWhiteSpace(this string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return string.Empty;
        return Regex.Replace(value.Trim(), @"\s+", " ");
    }

    /// <summary>
    /// Splits the string into chunks of at most <paramref name="chunkSize"/> characters.
    /// The last chunk may be shorter than <paramref name="chunkSize"/>.
    /// </summary>
    /// <param name="value">The source string.</param>
    /// <param name="chunkSize">Maximum character length of each chunk. Must be greater than zero.</param>
    /// <returns>An enumerable sequence of string chunks.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="chunkSize"/> is less than or equal to zero.
    /// </exception>
    public static IEnumerable<string> ToChunks(this string value, int chunkSize)
    {
        if (chunkSize <= 0)
            throw new ArgumentOutOfRangeException(nameof(chunkSize), "chunkSize must be greater than zero.");

        if (string.IsNullOrEmpty(value)) yield break;

        for (int i = 0; i < value.Length; i += chunkSize)
            yield return value.Substring(i, Math.Min(chunkSize, value.Length - i));
    }

    /// <summary>
    /// Converts the string to a URL-friendly slug by lower-casing, replacing whitespace with
    /// hyphens, and removing any character that is not a letter, digit, or hyphen.
    /// </summary>
    /// <param name="value">The source string.</param>
    public static string ToSlug(this string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return string.Empty;
        var slug = value.Trim().ToLowerInvariant();
        slug = Regex.Replace(slug, @"\s+", "-");
        slug = Regex.Replace(slug, @"[^a-z0-9\-]", string.Empty);
        slug = Regex.Replace(slug, @"-{2,}", "-").Trim('-');
        return slug;
    }
}
