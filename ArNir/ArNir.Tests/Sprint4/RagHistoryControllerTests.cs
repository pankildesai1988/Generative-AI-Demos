using ArNir.Admin.Controllers;
using ArNir.Core.Entities;
using ArNir.Data;
using ArNir.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using System.Text.Json;
using Xunit;

namespace ArNir.Tests.Sprint4;

/// <summary>Unit tests for the Sprint 4 <see cref="RagHistoryController.SubmitFeedback"/> action.</summary>
public class RagHistoryControllerTests
{
    private static RagHistoryController CreateController(
        DbContextOptions<ArNirDbContext>? sqlOptions = null,
        Mock<IRagHistoryService>? ragServiceMock = null)
    {
        var resolvedOptions = sqlOptions ?? new DbContextOptionsBuilder<ArNirDbContext>()
            .UseInMemoryDatabase("RagHistCtrl_" + Guid.NewGuid())
            .Options;

        var dbFactoryMock = new Mock<IDbContextFactory<ArNirDbContext>>();
        dbFactoryMock.Setup(f => f.CreateDbContextAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => new ArNirDbContext(resolvedOptions));

        ragServiceMock ??= new Mock<IRagHistoryService>();

        var controller = new RagHistoryController(
            ragServiceMock.Object,
            dbFactoryMock.Object);

        var httpContext = new DefaultHttpContext();
        controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
        controller.TempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());

        return controller;
    }

    [Fact]
    public async Task SubmitFeedback_ValidRating_ReturnsJson_Success()
    {
        // Arrange
        var sqlOptions = new DbContextOptionsBuilder<ArNirDbContext>()
            .UseInMemoryDatabase("RagHistCtrl_Valid_" + Guid.NewGuid())
            .Options;

        var controller = CreateController(sqlOptions);

        // Act
        var result = await controller.SubmitFeedback(1, 4, "Great retrieval!");

        // Assert
        var jsonResult = Assert.IsType<JsonResult>(result);
        Assert.NotNull(jsonResult.Value);
        var json   = JsonSerializer.Serialize(jsonResult.Value);
        var parsed = JsonDocument.Parse(json);
        Assert.True(parsed.RootElement.GetProperty("success").GetBoolean());
        Assert.Equal("Feedback saved.", parsed.RootElement.GetProperty("message").GetString());
    }

    [Theory]
    [InlineData(0)]
    [InlineData(6)]
    [InlineData(-1)]
    public async Task SubmitFeedback_InvalidRating_ReturnsBadRequest(int invalidRating)
    {
        // Arrange
        var controller = CreateController();

        // Act
        var result = await controller.SubmitFeedback(1, invalidRating, null);

        // Assert
        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.NotNull(badRequest.Value);
        Assert.Contains("1 and 5", badRequest.Value!.ToString());
    }

    [Fact]
    public async Task SubmitFeedback_ExistingFeedback_Updates_NotCreatesNew()
    {
        // Arrange — seed an existing feedback row for historyId=10
        var sqlOptions = new DbContextOptionsBuilder<ArNirDbContext>()
            .UseInMemoryDatabase("RagHistCtrl_Update_" + Guid.NewGuid())
            .Options;

        using (var ctx = new ArNirDbContext(sqlOptions))
        {
            ctx.Feedbacks.Add(new Feedback
            {
                HistoryId = 10,
                Rating    = 2,
                Comments  = "Old comment",
                CreatedAt = DateTime.UtcNow.AddMinutes(-5)
            });
            await ctx.SaveChangesAsync();
        }

        var controller = CreateController(sqlOptions);

        // Act — submit again for same historyId
        var result = await controller.SubmitFeedback(10, 5, "Updated comment");

        // Assert — still only 1 row
        var jsonResult = Assert.IsType<JsonResult>(result);
        var json   = JsonSerializer.Serialize(jsonResult.Value);
        var parsed = JsonDocument.Parse(json);
        Assert.True(parsed.RootElement.GetProperty("success").GetBoolean());

        using var verifyCtx = new ArNirDbContext(sqlOptions);
        var feedbacks = verifyCtx.Feedbacks.Where(f => f.HistoryId == 10).ToList();
        Assert.Single(feedbacks);
        Assert.Equal(5, feedbacks[0].Rating);
        Assert.Equal("Updated comment", feedbacks[0].Comments);
    }

    [Fact]
    public async Task SubmitFeedback_NewFeedback_CreatesRow()
    {
        // Arrange — empty database
        var sqlOptions = new DbContextOptionsBuilder<ArNirDbContext>()
            .UseInMemoryDatabase("RagHistCtrl_New_" + Guid.NewGuid())
            .Options;

        var controller = CreateController(sqlOptions);

        // Act
        var result = await controller.SubmitFeedback(99, 3, "Average");

        // Assert
        var jsonResult = Assert.IsType<JsonResult>(result);
        var json   = JsonSerializer.Serialize(jsonResult.Value);
        var parsed = JsonDocument.Parse(json);
        Assert.True(parsed.RootElement.GetProperty("success").GetBoolean());

        using var verifyCtx = new ArNirDbContext(sqlOptions);
        Assert.Equal(1, verifyCtx.Feedbacks.Count());
        var fb = verifyCtx.Feedbacks.Single();
        Assert.Equal(99, fb.HistoryId);
        Assert.Equal(3, fb.Rating);
        Assert.Equal("Average", fb.Comments);
    }
}
