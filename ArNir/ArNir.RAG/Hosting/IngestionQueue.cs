using System.Collections.Concurrent;
using System.Threading.Channels;

namespace ArNir.RAG.Hosting;

public record IngestionJobRequest(
    ArNir.RAG.Models.IngestionRequest Request,
    string DocumentName,
    DateTime QueuedAt);

public record IngestionJobResult(
    string DocumentName,
    bool Success,
    int ChunksCreated,
    int EmbeddingsCreated,
    string? Error,
    DateTime CompletedAt);

public sealed class IngestionQueue
{
    private readonly Channel<IngestionJobRequest> _channel =
        Channel.CreateBounded<IngestionJobRequest>(new BoundedChannelOptions(100)
        {
            FullMode = BoundedChannelFullMode.Wait
        });

    public ConcurrentQueue<IngestionJobResult> RecentResults { get; } = new();

    public int QueueDepth => _channel.Reader.Count;

    public async Task EnqueueAsync(IngestionJobRequest request, CancellationToken ct = default)
        => await _channel.Writer.WriteAsync(request, ct);

    public async ValueTask<IngestionJobRequest> DequeueAsync(CancellationToken ct)
        => await _channel.Reader.ReadAsync(ct);
}
