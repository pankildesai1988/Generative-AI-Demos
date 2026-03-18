using ArNir.RAG.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ArNir.RAG.Hosting;

public sealed class IngestionWorker : BackgroundService
{
    private readonly IngestionQueue _queue;
    private readonly IServiceProvider _services;
    private readonly ILogger<IngestionWorker> _logger;

    public IngestionWorker(IngestionQueue queue, IServiceProvider services, ILogger<IngestionWorker> logger)
    {
        _queue = queue; _services = services; _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("IngestionWorker started.");
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var job = await _queue.DequeueAsync(stoppingToken);
                _logger.LogInformation("Processing ingestion job for '{Doc}'.", job.DocumentName);

                using var scope = _services.CreateScope();
                var pipeline = scope.ServiceProvider.GetRequiredService<IIngestionPipeline>();

                var result = await pipeline.IngestAsync(job.Request);

                var jobResult = new IngestionJobResult(
                    job.DocumentName, true,
                    result.ChunksCreated, result.EmbeddingsCreated,
                    null, DateTime.UtcNow);
                _queue.RecentResults.Enqueue(jobResult);
                while (_queue.RecentResults.Count > 100)
                    _queue.RecentResults.TryDequeue(out _);

                _logger.LogInformation("Ingestion completed for '{Doc}': {Chunks} chunks, {Emb} embeddings.",
                    job.DocumentName, result.ChunksCreated, result.EmbeddingsCreated);
            }
            catch (OperationCanceledException) { break; }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ingestion job failed.");
                var jobResult = new IngestionJobResult("Unknown", false, 0, 0, ex.Message, DateTime.UtcNow);
                _queue.RecentResults.Enqueue(jobResult);
                while (_queue.RecentResults.Count > 100)
                    _queue.RecentResults.TryDequeue(out _);
            }
        }
    }
}
