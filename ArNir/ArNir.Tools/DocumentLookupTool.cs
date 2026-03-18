using ArNir.Agents.Interfaces;
using Microsoft.Extensions.Logging;

namespace ArNir.Tools;

/// <summary>
/// An <see cref="IAgentTool"/> that performs document look-up operations within the ArNir platform.
/// <para>
/// <b>Phase 7 — placeholder implementation.</b> The current implementation logs the look-up request
/// and returns a descriptive stub result. A full implementation will delegate to the RAG retrieval
/// pipeline (e.g. via <c>IRetrievalService</c> or <c>IDocumentVectorStore</c>) in a later phase.
/// </para>
/// <para>
/// Expected <c>parameters</c> keys (reserved for the full implementation):
/// <list type="bullet">
///   <item><term>query</term><description>The natural-language question or keyword to search for in stored documents.</description></item>
///   <item><term>topK</term><description>Optional. Maximum number of document chunks to return. Defaults to <c>5</c> when omitted.</description></item>
/// </list>
/// </para>
/// </summary>
public sealed class DocumentLookupTool : IAgentTool
{
    private readonly ILogger<DocumentLookupTool> _logger;

    /// <inheritdoc />
    public string Name => "DocumentLookup";

    /// <inheritdoc />
    public string Description =>
        "Searches stored documents and returns relevant text chunks that match a query. " +
        "Use this tool to look up facts, policies, or context from the document knowledge base.";

    /// <summary>
    /// Initialises a new instance of <see cref="DocumentLookupTool"/>.
    /// </summary>
    /// <param name="logger">Logger for diagnostic output.</param>
    public DocumentLookupTool(ILogger<DocumentLookupTool> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc />
    /// <remarks>
    /// <b>Phase 7 stub:</b> logs the query and returns a placeholder result.
    /// The full implementation will call the RAG retrieval pipeline.
    /// </remarks>
    public Task<string> ExecuteAsync(
        Dictionary<string, string> parameters,
        CancellationToken ct = default)
    {
        parameters.TryGetValue("query", out var query);
        parameters.TryGetValue("topK", out var topKRaw);
        var topK = int.TryParse(topKRaw, out var k) ? k : 5;

        _logger.LogInformation(
            "DocumentLookupTool: look-up requested — query='{Query}', topK={TopK}.",
            query ?? "(none)", topK);

        // Phase 7 placeholder — full RAG delegation deferred to a later phase
        var result =
            $"[DocumentLookup — placeholder] Query: '{query ?? string.Empty}'. " +
            $"No documents are wired in Phase 7. " +
            $"Connect IRetrievalService or IDocumentVectorStore to return real chunks.";

        _logger.LogDebug("DocumentLookupTool: returning stub result ({Length} chars).", result.Length);

        return Task.FromResult(result);
    }
}
