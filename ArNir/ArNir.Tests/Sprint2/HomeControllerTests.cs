using ArNir.Admin.Controllers;
using ArNir.Admin.Models;
using ArNir.Core.Entities;
using ArNir.Data;
using ArNir.RAG.InProcess;
using ArNir.RAG.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace ArNir.Tests.Sprint2;

public class HomeControllerTests
{
    [Fact]
    public async Task Index_ReturnsPlatformHealthViewModelWithCorrectDocumentCount()
    {
        // Arrange — InMemory SQL context
        var sqlOptions = new DbContextOptionsBuilder<ArNirDbContext>()
            .UseInMemoryDatabase("HomeCtrl_Sql_" + Guid.NewGuid())
            .Options;

        // Seed data
        using (var ctx = new ArNirDbContext(sqlOptions))
        {
            ctx.Documents.Add(new Document { Name = "Doc1", Type = "pdf", UploadedAt = DateTime.UtcNow });
            ctx.Documents.Add(new Document { Name = "Doc2", Type = "txt", UploadedAt = DateTime.UtcNow });
            await ctx.SaveChangesAsync();
        }

        var sqlFactoryMock = new Mock<IDbContextFactory<ArNirDbContext>>();
        sqlFactoryMock.Setup(f => f.CreateDbContextAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => new ArNirDbContext(sqlOptions));

        // PG factory mock — throw to simulate no PG connection
        var pgFactoryMock = new Mock<IDbContextFactory<VectorDbContext>>();
        pgFactoryMock.Setup(f => f.CreateDbContextAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("No PG"));

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "OpenAI:ApiKey", "sk-your-openai-key" }
            })
            .Build();

        // Service provider returning NullDocumentEmbedder
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<IDocumentEmbedder, NullDocumentEmbedder>();
        var sp = services.BuildServiceProvider();

        var logger = new Mock<ILogger<HomeController>>();

        var controller = new HomeController(
            logger.Object, sqlFactoryMock.Object, pgFactoryMock.Object, config, sp);

        var httpContext = new DefaultHttpContext();
        controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
        controller.TempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());

        // Act
        var result = await controller.Index();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<PlatformHealthViewModel>(viewResult.Model);
        Assert.Equal(2, model.TotalDocuments);
        Assert.False(model.PostgresConnected);
        Assert.False(model.OpenAiKeyConfigured); // "sk-your-openai-key" is the placeholder
        Assert.False(model.EmbedderIsReal); // NullDocumentEmbedder
    }
}
