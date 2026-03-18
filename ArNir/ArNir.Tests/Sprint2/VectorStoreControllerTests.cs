using ArNir.Admin.Controllers;
using ArNir.Admin.Models;
using ArNir.Core.Entities;
using ArNir.Data;
using ArNir.RAG.Interfaces;
using ArNir.RAG.Models;
using ArNir.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace ArNir.Tests.Sprint2;

public class VectorStoreControllerTests
{
    private VectorStoreController CreateController(
        DbContextOptions<ArNirDbContext> sqlOptions,
        Mock<IDbContextFactory<VectorDbContext>> pgFactoryMock)
    {
        var sqlFactoryMock = new Mock<IDbContextFactory<ArNirDbContext>>();
        sqlFactoryMock.Setup(f => f.CreateDbContextAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => new ArNirDbContext(sqlOptions));

        var pipelineMock = new Mock<IIngestionPipeline>();
        pipelineMock.Setup(p => p.IngestAsync(It.IsAny<IngestionRequest>()))
            .ReturnsAsync(new IngestionResult { Success = true, ChunksCreated = 3, EmbeddingsCreated = 3 });

        var docServiceMock = new Mock<IDocumentService>();
        var loggerMock = new Mock<ILogger<VectorStoreController>>();

        var controller = new VectorStoreController(
            sqlFactoryMock.Object, pgFactoryMock.Object, pipelineMock.Object,
            docServiceMock.Object, loggerMock.Object);

        var httpContext = new DefaultHttpContext();
        controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
        controller.TempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());

        return controller;
    }

    [Fact]
    public async Task Index_ReturnsVectorStoreViewModel()
    {
        // Arrange — PG factory throws (simulates no PG)
        var sqlOptions = new DbContextOptionsBuilder<ArNirDbContext>()
            .UseInMemoryDatabase("VectorStoreCtrl_" + Guid.NewGuid())
            .Options;

        var pgFactoryMock = new Mock<IDbContextFactory<VectorDbContext>>();
        pgFactoryMock.Setup(f => f.CreateDbContextAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("No PG"));

        var controller = CreateController(sqlOptions, pgFactoryMock);

        // Act
        var result = await controller.Index();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<VectorStoreViewModel>(viewResult.Model);
        Assert.NotNull(model);
    }

    [Fact]
    public async Task RebuildForDocument_NonExistentDocument_RedirectsToIndex()
    {
        // Arrange
        var sqlOptions = new DbContextOptionsBuilder<ArNirDbContext>()
            .UseInMemoryDatabase("VectorStoreCtrl_Rebuild_" + Guid.NewGuid())
            .Options;

        var pgFactoryMock = new Mock<IDbContextFactory<VectorDbContext>>();

        var controller = CreateController(sqlOptions, pgFactoryMock);

        // Act
        var result = await controller.RebuildForDocument(999);

        // Assert
        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirect.ActionName);
    }
}
