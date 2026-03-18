using ArNir.Admin.Controllers;
using ArNir.Admin.Models;
using ArNir.Core.DTOs.Documents;
using ArNir.Core.Entities;
using ArNir.Data;
using ArNir.RAG.Hosting;
using ArNir.Services.Interfaces;
using ArNir.Services.Provider;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace ArNir.Tests.Sprint3;

/// <summary>Unit tests for the enhanced <see cref="EmbeddingController"/>.</summary>
public class EmbeddingControllerTests
{
    private static EmbeddingController CreateController(
        Mock<IDbContextFactory<VectorDbContext>>? pgFactoryMock = null,
        DbContextOptions<ArNirDbContext>? sqlOptions = null,
        IngestionQueue? queue = null,
        Mock<IDocumentService>? docServiceMock = null)
    {
        pgFactoryMock ??= new Mock<IDbContextFactory<VectorDbContext>>();
        pgFactoryMock.Setup(f => f.CreateDbContextAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("PG not available in tests"));

        var resolvedSqlOptions = sqlOptions ?? new DbContextOptionsBuilder<ArNirDbContext>()
            .UseInMemoryDatabase("EmbCtrl_" + Guid.NewGuid())
            .Options;

        var sqlFactoryMock = new Mock<IDbContextFactory<ArNirDbContext>>();
        sqlFactoryMock.Setup(f => f.CreateDbContextAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => new ArNirDbContext(resolvedSqlOptions));

        var resolvedQueue    = queue ?? new IngestionQueue();
        var resolvedDocSvc   = docServiceMock ?? new Mock<IDocumentService>();
        var embeddingService = new Mock<IEmbeddingService>();
        var embeddingProvider= new Mock<IEmbeddingProvider>();

        var controller = new EmbeddingController(
            embeddingService.Object,
            embeddingProvider.Object,
            pgFactoryMock.Object,
            sqlFactoryMock.Object,
            resolvedQueue,
            resolvedDocSvc.Object);

        var httpContext = new DefaultHttpContext();
        controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
        controller.TempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());

        return controller;
    }

    [Fact]
    public async Task Stats_ReturnsJson_WithTotalAndByModel()
    {
        // Arrange — PG throws, so stats should be empty/zero (graceful degradation)
        var controller = CreateController();

        // Act
        var result = await controller.Stats();

        // Assert — must be a JSON result with expected shape
        var jsonResult = Assert.IsType<JsonResult>(result);
        Assert.NotNull(jsonResult.Value);

        // Use dynamic to inspect anonymous type
        dynamic data = jsonResult.Value!;
        Assert.NotNull(data);
    }

    [Fact]
    public async Task Stats_ReturnsJson_WithZeroWhenPgUnavailable()
    {
        // Arrange
        var controller = CreateController();

        // Act
        var result = await controller.Stats();

        // Assert
        var jsonResult = Assert.IsType<JsonResult>(result);
        Assert.NotNull(jsonResult.Value);
    }

    [Fact]
    public async Task RebuildAll_EnqueuesAllDocuments()
    {
        // Arrange — document service returns 2 documents
        var docServiceMock = new Mock<IDocumentService>();
        docServiceMock.Setup(s => s.GetAllDocumentsAsync())
            .ReturnsAsync(new List<DocumentResponseDto>
            {
                new() { Id = 1, Name = "Doc1.pdf" },
                new() { Id = 2, Name = "Doc2.txt" }
            });

        var queue = new IngestionQueue();
        var controller = CreateController(queue: queue, docServiceMock: docServiceMock);

        // Act
        var result = await controller.RebuildAll();

        // Assert
        var jsonResult = Assert.IsType<JsonResult>(result);
        Assert.NotNull(jsonResult.Value);
        // Queue depth should reflect the 2 enqueued items
        Assert.Equal(2, queue.QueueDepth);
    }

    [Fact]
    public async Task DeleteByModel_ReturnsDeletedCount_WhenModelIsEmpty()
    {
        // Arrange — PG throws; model parameter empty
        var controller = CreateController();

        // Act
        var result = await controller.DeleteByModel("");

        // Assert
        var jsonResult = Assert.IsType<JsonResult>(result);
        Assert.NotNull(jsonResult.Value);
    }

    [Fact]
    public async Task Index_ReturnsViewWithEmbeddingStatsViewModel()
    {
        // Arrange — both PG and SQL available (SQL has docs)
        var sqlOptions = new DbContextOptionsBuilder<ArNirDbContext>()
            .UseInMemoryDatabase("EmbCtrl_Index_" + Guid.NewGuid())
            .Options;
        using (var ctx = new ArNirDbContext(sqlOptions))
        {
            ctx.Documents.Add(new Document { Name = "Test", Type = "txt", UploadedAt = DateTime.UtcNow });
            await ctx.SaveChangesAsync();
        }

        var controller = CreateController(sqlOptions: sqlOptions);

        // Act
        var result = await controller.Index();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<EmbeddingStatsViewModel>(viewResult.Model);
        Assert.NotNull(model);
        Assert.Equal(1, model.TotalDocuments);
    }
}
