namespace ArNir.Admin.Models;

/// <summary>Summary row for a grouped chat-memory session.</summary>
public class MemorySessionViewModel
{
    /// <summary>Unique session identifier.</summary>
    public string SessionId { get; set; } = "";

    /// <summary>Number of messages (user + assistant turns) in this session.</summary>
    public int MessageCount { get; set; }

    /// <summary>UTC timestamp of the most recent message in this session.</summary>
    public DateTime LastActivity { get; set; }
}
