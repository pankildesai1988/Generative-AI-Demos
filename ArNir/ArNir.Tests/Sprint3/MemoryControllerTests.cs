using ArNir.Admin.Controllers;
using ArNir.Admin.Models;
using ArNir.Core.Entities;
using ArNir.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace ArNir.Tests.Sprint3;

/// <summary>Unit tests for <see cref="MemoryController"/>.</summary>
public class MemoryControllerTests
{
    private static MemoryController CreateController(DbContextOptions<ArNirDbContext> sqlOptions)
    {
        var sqlFactoryMock = new Mock<IDbContextFactory<ArNirDbContext>>();
        sqlFactoryMock.Setup(f => f.CreateDbContextAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => new ArNirDbContext(sqlOptions));

        var logger = new Mock<ILogger<MemoryController>>();
        var controller = new MemoryController(sqlFactoryMock.Object, logger.Object);

        var httpContext = new DefaultHttpContext();
        controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
        controller.TempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());
        return controller;
    }

    private static async Task<DbContextOptions<ArNirDbContext>> SeedMemoriesAsync(
        params (string SessionId, string UserMsg, string? AsstMsg, DateTime CreatedAt)[] rows)
    {
        var options = new DbContextOptionsBuilder<ArNirDbContext>()
            .UseInMemoryDatabase("MemCtrl_" + Guid.NewGuid())
            .Options;
        using var ctx = new ArNirDbContext(options);
        foreach (var (sid, userMsg, asstMsg, createdAt) in rows)
        {
            ctx.ChatMemories.Add(new ChatMemory
            {
                SessionId        = sid,
                UserMessage      = userMsg,
                AssistantMessage = asstMsg,
                CreatedAt        = createdAt
            });
        }
        await ctx.SaveChangesAsync();
        return options;
    }

    [Fact]
    public async Task Index_ReturnsViewWithSessions()
    {
        // Arrange — 3 rows across 2 sessions
        var options = await SeedMemoriesAsync(
            ("session-a", "Hello",   "Hi!",  DateTime.UtcNow.AddHours(-2)),
            ("session-a", "Another", null,   DateTime.UtcNow.AddHours(-1)),
            ("session-b", "Test",    "Resp", DateTime.UtcNow));

        var controller = CreateController(options);

        // Act
        var result = await controller.Index();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsAssignableFrom<IEnumerable<MemorySessionViewModel>>(viewResult.Model);
        Assert.Equal(2, model.Count());
        Assert.Contains(model, s => s.SessionId == "session-a" && s.MessageCount == 2);
        Assert.Contains(model, s => s.SessionId == "session-b" && s.MessageCount == 1);
    }

    [Fact]
    public async Task DeleteSession_RemovesRows_AndRedirects()
    {
        // Arrange
        var options = await SeedMemoriesAsync(
            ("sess-del", "Msg1", null,  DateTime.UtcNow),
            ("sess-del", "Msg2", "OK",  DateTime.UtcNow),
            ("sess-keep", "Stay", null, DateTime.UtcNow));

        var controller = CreateController(options);

        // Act
        var result = await controller.DeleteSession("sess-del");

        // Assert
        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirect.ActionName);

        // Verify rows were deleted
        using var ctx = new ArNirDbContext(options);
        Assert.Equal(0, ctx.ChatMemories.Count(m => m.SessionId == "sess-del"));
        Assert.Equal(1, ctx.ChatMemories.Count(m => m.SessionId == "sess-keep"));
    }

    [Fact]
    public async Task PurgeOld_RemovesOldSessions_AndRedirects()
    {
        // Arrange — one old session (35 days ago), one recent
        var options = await SeedMemoriesAsync(
            ("old-sess",    "Old",    null, DateTime.UtcNow.AddDays(-35)),
            ("recent-sess", "Recent", null, DateTime.UtcNow.AddDays(-1)));

        var controller = CreateController(options);

        // Act — purge sessions older than 30 days
        var result = await controller.PurgeOld(30);

        // Assert
        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirect.ActionName);

        using var ctx = new ArNirDbContext(options);
        Assert.Equal(0, ctx.ChatMemories.Count(m => m.SessionId == "old-sess"));
        Assert.Equal(1, ctx.ChatMemories.Count(m => m.SessionId == "recent-sess"));
    }

    [Fact]
    public async Task PurgeOld_NoOldSessions_SetsTempDataSuccess()
    {
        // Arrange — only recent session
        var options = await SeedMemoriesAsync(
            ("new-sess", "Recent", null, DateTime.UtcNow));
        var controller = CreateController(options);

        // Act
        var result = await controller.PurgeOld(30);

        // Assert
        Assert.IsType<RedirectToActionResult>(result);
        Assert.Contains("No sessions", controller.TempData["Success"]?.ToString() ?? "");
    }

    [Fact]
    public async Task Session_ReturnsNotFound_WhenSessionMissing()
    {
        // Arrange — empty DB
        var options = new DbContextOptionsBuilder<ArNirDbContext>()
            .UseInMemoryDatabase("MemCtrl_NotFound_" + Guid.NewGuid())
            .Options;
        var controller = CreateController(options);

        // Act
        var result = await controller.Session("nonexistent-session");

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }
}
