namespace ArNir.Platform.Configuration;

/// <summary>
/// Strongly-typed configuration settings for the OpenAI provider.
/// Bind this class to the <c>OpenAI</c> section of <c>appsettings.json</c>.
/// </summary>
public sealed class OpenAiSettings
{
    /// <summary>
    /// Configuration section key used when binding from <see cref="Microsoft.Extensions.Configuration.IConfiguration"/>.
    /// </summary>
    public const string SectionName = "OpenAI";

    /// <summary>
    /// Gets or sets the OpenAI API key used to authenticate requests.
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the default model identifier (e.g. <c>gpt-4o</c>).
    /// </summary>
    public string Model { get; set; } = "gpt-4o";

    /// <summary>
    /// Gets or sets the maximum number of tokens the model may generate in a single response.
    /// </summary>
    public int MaxTokens { get; set; } = 2048;

    /// <summary>
    /// Gets or sets the sampling temperature. Values closer to <c>0</c> produce more deterministic
    /// output; values closer to <c>2</c> produce more varied output.
    /// </summary>
    public double Temperature { get; set; } = 0.7;

    /// <summary>
    /// Gets or sets the base URL for the OpenAI API endpoint.
    /// Override this to point at Azure OpenAI or a compatible proxy.
    /// </summary>
    public string BaseUrl { get; set; } = "https://api.openai.com/v1";
}
