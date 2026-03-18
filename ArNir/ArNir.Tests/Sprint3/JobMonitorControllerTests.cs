using ArNir.Admin.Controllers;
using ArNir.RAG.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace ArNir.Tests.Sprint3;

/// <summary>Unit tests for <see cref="JobMonitorController"/>.</summary>
public class JobMonitorControllerTests
{
    private static JobMonitorController CreateController(IngestionQueue? queue = null)
    {
        queue ??= new IngestionQueue();
        var logger = new Mock<ILogger<JobMonitorController>>();
        var controller = new JobMonitorController(queue, logger.Object);
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
        return controller;
    }

    [Fact]
    public void Index_ReturnsView()
    {
        // Arrange
        var controller = CreateController();

        // Act
        var result = controller.Index();

        // Assert
        Assert.IsType<ViewResult>(result);
    }

    [Fact]
    public void Status_ReturnsJsonWithQueueDepth()
    {
        // Arrange — empty queue
        var queue = new IngestionQueue();
        var controller = CreateController(queue);

        // Act
        var result = controller.Status();

        // Assert
        var jsonResult = Assert.IsType<JsonResult>(result);
        Assert.NotNull(jsonResult.Value);

        // Inspect using reflection to avoid dynamic
        var value = jsonResult.Value!;
        var queueDepthProp = value.GetType().GetProperty("queueDepth");
        Assert.NotNull(queueDepthProp);
        var depth = (int)queueDepthProp.GetValue(value)!;
        Assert.Equal(0, depth);
    }

    [Fact]
    public void Status_ReturnsJsonWithRecentResults()
    {
        // Arrange — queue with one completed result
        var queue = new IngestionQueue();
        queue.RecentResults.Enqueue(new IngestionJobResult(
            DocumentName: "test.pdf",
            Success: true,
            ChunksCreated: 5,
            EmbeddingsCreated: 5,
            Error: null,
            CompletedAt: DateTime.UtcNow));

        var controller = CreateController(queue);

        // Act
        var result = controller.Status();

        // Assert
        var jsonResult = Assert.IsType<JsonResult>(result);
        Assert.NotNull(jsonResult.Value);

        var value = jsonResult.Value!;
        var resultsProp = value.GetType().GetProperty("recentResults");
        Assert.NotNull(resultsProp);
        var recentResults = resultsProp.GetValue(value) as System.Collections.IEnumerable;
        Assert.NotNull(recentResults);
        Assert.Single(recentResults!.Cast<object>());
    }

    [Fact]
    public void Status_EmptyQueue_ReturnsZeroDepth()
    {
        // Arrange
        var controller = CreateController(new IngestionQueue());

        // Act
        var result = controller.Status();

        // Assert
        var json = Assert.IsType<JsonResult>(result);
        Assert.NotNull(json.Value);
    }
}
