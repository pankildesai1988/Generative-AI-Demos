using System.Collections.Concurrent;
using ArNir.Agents.Interfaces;
using Microsoft.Extensions.Logging;

namespace ArNir.Agents.Registry;

/// <summary>
/// A thread-safe, <see cref="ConcurrentDictionary{TKey,TValue}"/>-backed implementation of
/// <see cref="IToolRegistry"/>.
/// <para>
/// Tools are keyed by <see cref="IAgentTool.Name"/> using <see cref="StringComparer.OrdinalIgnoreCase"/>
/// so lookups are case-insensitive. Registered as Singleton in the DI container, allowing tools
/// to be registered at startup and shared safely across concurrent requests.
/// </para>
/// </summary>
public sealed class ToolRegistry : IToolRegistry
{
    private readonly ConcurrentDictionary<string, IAgentTool> _tools =
        new(StringComparer.OrdinalIgnoreCase);

    private readonly ILogger<ToolRegistry> _logger;

    /// <summary>
    /// Initialises a new instance of <see cref="ToolRegistry"/>.
    /// </summary>
    /// <param name="logger">Logger for diagnostic output.</param>
    public ToolRegistry(ILogger<ToolRegistry> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc />
    /// <remarks>
    /// If a tool with the same name is already registered it is silently replaced.
    /// A debug message is logged on every successful registration.
    /// </remarks>
    public void Register(IAgentTool tool)
    {
        _tools[tool.Name] = tool;

        _logger.LogDebug(
            "ToolRegistry: registered tool '{ToolName}' ({ToolType}).",
            tool.Name, tool.GetType().Name);
    }

    /// <inheritdoc />
    /// <remarks>
    /// Performs a case-insensitive lookup. Logs a warning when the tool is not found
    /// so callers can detect misconfigured step plans at runtime.
    /// </remarks>
    public IAgentTool? Get(string name)
    {
        if (_tools.TryGetValue(name, out var tool))
        {
            return tool;
        }

        _logger.LogWarning(
            "ToolRegistry: tool '{ToolName}' was requested but is not registered.", name);

        return null;
    }

    /// <inheritdoc />
    public IReadOnlyList<IAgentTool> GetAll() =>
        _tools.Values.ToList().AsReadOnly();
}
