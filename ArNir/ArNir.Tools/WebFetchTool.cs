using ArNir.Agents.Interfaces;
using Microsoft.Extensions.Logging;

namespace ArNir.Tools;

/// <summary>
/// An <see cref="IAgentTool"/> that fetches the text content of a URL via <see cref="HttpClient"/>
/// and returns the first 2 000 characters of the response body.
/// <para>
/// Required <c>parameters</c> key:
/// <list type="bullet">
///   <item><term>url</term><description>The fully-qualified URL to fetch (e.g. <c>https://example.com/page</c>).</description></item>
/// </list>
/// </para>
/// <para>
/// The response is always truncated to 2 000 characters to keep downstream context windows manageable.
/// A trailing notice is appended when the content is truncated.
/// </para>
/// <para>
/// On any <see cref="HttpRequestException"/> or <see cref="TaskCanceledException"/> the tool returns
/// a descriptive error string rather than re-throwing, so that the <c>PlannerAgent</c> can continue
/// executing remaining plan steps.
/// </para>
/// </summary>
public sealed class WebFetchTool : IAgentTool
{
    /// <summary>Maximum number of characters returned from the fetched response body.</summary>
    private const int MaxResponseChars = 2000;

    private readonly HttpClient _httpClient;
    private readonly ILogger<WebFetchTool> _logger;

    /// <inheritdoc />
    public string Name => "WebFetch";

    /// <inheritdoc />
    public string Description =>
        "Fetches the text content of a web page or URL and returns up to 2 000 characters. " +
        "Use this tool to retrieve live web content, API responses, or publicly accessible documents.";

    /// <summary>
    /// Initialises a new instance of <see cref="WebFetchTool"/>.
    /// </summary>
    /// <param name="httpClient">
    /// The <see cref="HttpClient"/> used to perform HTTP GET requests.
    /// Should be provided via <c>IHttpClientFactory</c> or injected directly as a typed client.
    /// </param>
    /// <param name="logger">Logger for diagnostic and error output.</param>
    public WebFetchTool(HttpClient httpClient, ILogger<WebFetchTool> logger)
    {
        _httpClient = httpClient;
        _logger     = logger;
    }

    /// <inheritdoc />
    /// <remarks>
    /// Performs an HTTP GET to <c>parameters["url"]</c>.
    /// The response body is read as a string and truncated to <see cref="MaxResponseChars"/> characters.
    /// Returns an error description string (does not throw) on HTTP or network failure.
    /// </remarks>
    public async Task<string> ExecuteAsync(
        Dictionary<string, string> parameters,
        CancellationToken ct = default)
    {
        if (!parameters.TryGetValue("url", out var url) || string.IsNullOrWhiteSpace(url))
        {
            _logger.LogWarning("WebFetchTool: 'url' parameter is missing or empty.");
            return "[WebFetch error] Required parameter 'url' was not provided.";
        }

        _logger.LogInformation("WebFetchTool: fetching URL '{Url}'.", url);

        try
        {
            var response = await _httpClient
                .GetAsync(url, HttpCompletionOption.ResponseHeadersRead, ct)
                .ConfigureAwait(false);

            response.EnsureSuccessStatusCode();

            var body = await response.Content
                .ReadAsStringAsync(ct)
                .ConfigureAwait(false);

            if (body.Length <= MaxResponseChars)
            {
                _logger.LogDebug(
                    "WebFetchTool: fetched {Length} chars from '{Url}'.", body.Length, url);

                return body;
            }

            var truncated = body[..MaxResponseChars];

            _logger.LogDebug(
                "WebFetchTool: response truncated from {Total} to {Max} chars for '{Url}'.",
                body.Length, MaxResponseChars, url);

            return truncated +
                   $"\n\n[WebFetch] Response truncated to {MaxResponseChars} characters " +
                   $"(original length: {body.Length} chars).";
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "WebFetchTool: HTTP request failed for '{Url}'.", url);
            return $"[WebFetch error] HTTP request failed for '{url}': {ex.Message}";
        }
        catch (TaskCanceledException ex) when (!ct.IsCancellationRequested)
        {
            _logger.LogError(ex, "WebFetchTool: request to '{Url}' timed out.", url);
            return $"[WebFetch error] Request to '{url}' timed out.";
        }
    }
}
