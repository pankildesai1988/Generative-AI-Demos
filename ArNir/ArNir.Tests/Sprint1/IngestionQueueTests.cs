using ArNir.RAG.Hosting;
using ArNir.RAG.Models;
using Xunit;

namespace ArNir.Tests.Sprint1;

public class IngestionQueueTests
{
    [Fact]
    public async Task EnqueueAndDequeue_RoundTripsCorrectly()
    {
        // Arrange
        var queue = new IngestionQueue();
        var request = new IngestionJobRequest(
            new IngestionRequest { FileName = "test.pdf" },
            "test.pdf",
            DateTime.UtcNow);

        // Act
        await queue.EnqueueAsync(request);
        var dequeued = await queue.DequeueAsync(CancellationToken.None);

        // Assert
        Assert.Equal("test.pdf", dequeued.DocumentName);
        Assert.Equal("test.pdf", dequeued.Request.FileName);
    }

    [Fact]
    public async Task QueueDepth_ReflectsEnqueuedItems()
    {
        // Arrange
        var queue = new IngestionQueue();
        Assert.Equal(0, queue.QueueDepth);

        // Act
        await queue.EnqueueAsync(new IngestionJobRequest(
            new IngestionRequest { FileName = "a.pdf" }, "a.pdf", DateTime.UtcNow));
        await queue.EnqueueAsync(new IngestionJobRequest(
            new IngestionRequest { FileName = "b.pdf" }, "b.pdf", DateTime.UtcNow));

        // Assert
        Assert.Equal(2, queue.QueueDepth);

        // Dequeue one
        await queue.DequeueAsync(CancellationToken.None);
        Assert.Equal(1, queue.QueueDepth);
    }

    [Fact]
    public void RecentResults_EnqueueAndDequeue()
    {
        // Arrange
        var queue = new IngestionQueue();
        var result = new IngestionJobResult("doc.pdf", true, 5, 5, null, DateTime.UtcNow);

        // Act
        queue.RecentResults.Enqueue(result);

        // Assert
        Assert.Single(queue.RecentResults);
        Assert.True(queue.RecentResults.TryDequeue(out var dequeued));
        Assert.Equal("doc.pdf", dequeued.DocumentName);
        Assert.True(dequeued.Success);
        Assert.Equal(5, dequeued.ChunksCreated);
    }
}
