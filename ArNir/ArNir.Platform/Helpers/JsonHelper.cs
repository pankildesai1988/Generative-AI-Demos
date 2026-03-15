using System.Text.Json;
using System.Text.Json.Serialization;

namespace ArNir.Platform.Helpers;

/// <summary>
/// Provides convenience wrappers around <see cref="System.Text.Json"/> for serialisation
/// and deserialisation throughout the ArNir platform.
/// </summary>
public static class JsonHelper
{
    /// <summary>
    /// Default <see cref="JsonSerializerOptions"/> shared across the platform:
    /// camelCase property names, enum members serialised as strings, and
    /// <see langword="null"/> values omitted from output.
    /// </summary>
    public static readonly JsonSerializerOptions DefaultOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    /// <summary>
    /// Serialises <paramref name="value"/> to a JSON string using <see cref="DefaultOptions"/>.
    /// </summary>
    /// <typeparam name="T">The type of the value to serialise.</typeparam>
    /// <param name="value">The object to serialise.</param>
    /// <returns>A JSON representation of <paramref name="value"/>.</returns>
    public static string Serialize<T>(T value) =>
        JsonSerializer.Serialize(value, DefaultOptions);

    /// <summary>
    /// Deserialises a JSON string to an instance of <typeparamref name="T"/> using
    /// <see cref="DefaultOptions"/>.
    /// </summary>
    /// <typeparam name="T">The target type.</typeparam>
    /// <param name="json">The JSON string to deserialise.</param>
    /// <returns>
    /// The deserialised instance, or <see langword="default"/> if <paramref name="json"/>
    /// is <see langword="null"/> or empty.
    /// </returns>
    public static T? Deserialize<T>(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return default;
        return JsonSerializer.Deserialize<T>(json, DefaultOptions);
    }

    /// <summary>
    /// Attempts to deserialise <paramref name="json"/> into <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The target type.</typeparam>
    /// <param name="json">The JSON string to deserialise.</param>
    /// <param name="result">
    /// When this method returns <see langword="true"/>, contains the deserialised value;
    /// otherwise contains <see langword="default"/>.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if deserialisation succeeded; <see langword="false"/> if
    /// <paramref name="json"/> is null/empty or a <see cref="JsonException"/> was thrown.
    /// </returns>
    public static bool TryDeserialize<T>(string? json, out T? result)
    {
        result = default;
        if (string.IsNullOrWhiteSpace(json)) return false;
        try
        {
            result = JsonSerializer.Deserialize<T>(json, DefaultOptions);
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }

    /// <summary>
    /// Performs a deep clone of <paramref name="value"/> by round-tripping through JSON.
    /// </summary>
    /// <typeparam name="T">The type of the value to clone.</typeparam>
    /// <param name="value">The object to clone.</param>
    /// <returns>A deep copy of <paramref name="value"/>.</returns>
    public static T? DeepClone<T>(T value)
    {
        var json = Serialize(value);
        return Deserialize<T>(json);
    }
}
