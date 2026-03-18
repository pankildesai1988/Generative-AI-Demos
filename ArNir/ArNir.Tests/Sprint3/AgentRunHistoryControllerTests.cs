using ArNir.Admin.Controllers;
using ArNir.Agents.Interfaces;
using ArNir.Agents.Models;
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

/// <summary>Unit tests for the updated <see cref="AgentRunHistoryController"/>.</summary>
public class AgentRunHistoryControllerTests
{
    private static AgentRunHistoryController CreateController(
        DbContextOptions<ArNirDbContext>? sqlOptions = null,
        Mock<IPlannerAgent>? plannerMock = null)
    {
        var resolvedOptions = sqlOptions ?? new DbContextOptionsBuilder<ArNirDbContext>()
            .UseInMemoryDatabase("AgentCtrl_" + Guid.NewGuid())
            .Options;

        var sqlFactoryMock = new Mock<IDbContextFactory<ArNirDbContext>>();
        sqlFactoryMock.Setup(f => f.CreateDbContextAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => new ArNirDbContext(resolvedOptions));

        plannerMock ??= BuildDefaultPlannerMock();
        var logger = new Mock<ILogger<AgentRunHistoryController>>();

        var controller = new AgentRunHistoryController(
            sqlFactoryMock.Object,
            plannerMock.Object,
            logger.Object);

        var httpContext = new DefaultHttpContext();
        controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
        controller.TempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());
        return controller;
    }

    private static Mock<IPlannerAgent> BuildDefaultPlannerMock()
    {
        var mock = new Mock<IPlannerAgent>();
        var plan = new AgentPlan
        {
            SessionId     = "test-session",
            OriginalQuery = "test query",
            Status        = AgentPlanStatus.Completed
        };
        mock.Setup(p => p.CreatePlanAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(plan);
        mock.Setup(p => p.ExecutePlanAsync(It.IsAny<AgentPlan>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((AgentPlan p, CancellationToken _) =>
            {
                p.Status = AgentPlanStatus.Completed;
                return p;
            });
        return mock;
    }

    [Fact]
    public void TriggerRun_Get_ReturnsView()
    {
        // Arrange
        var controller = CreateController();

        // Act
        var result = controller.TriggerRun();

        // Assert
        Assert.IsType<ViewResult>(result);
    }

    [Fact]
    public async Task TriggerRun_Post_WithEmptyQuery_ReturnsViewWithError()
    {
        // Arrange
        var controller = CreateController();

        // Act — empty query
        var result = await controller.TriggerRun("   ", null);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.False(controller.ModelState.IsValid);
        Assert.True(controller.ModelState.ContainsKey("query"));
    }

    [Fact]
    public async Task TriggerRun_Post_WithValidQuery_RedirectsToIndex()
    {
        // Arrange
        var sqlOptions = new DbContextOptionsBuilder<ArNirDbContext>()
            .UseInMemoryDatabase("AgentCtrl_ValidQuery_" + Guid.NewGuid())
            .Options;
        var controller = CreateController(sqlOptions: sqlOptions);

        // Act
        var result = await controller.TriggerRun("What is the capital of France?", null);

        // Assert — must redirect to Index
        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirect.ActionName);
        Assert.Equal("Agent run triggered successfully.", controller.TempData["Success"]?.ToString());

        // Verify log was saved
        using var ctx = new ArNirDbContext(sqlOptions);
        Assert.Equal(1, ctx.AgentRunLogs.Count());
    }

    [Fact]
    public async Task TriggerRun_Post_WithCustomSessionId_UsesProvidedSessionId()
    {
        // Arrange
        var sqlOptions = new DbContextOptionsBuilder<ArNirDbContext>()
            .UseInMemoryDatabase("AgentCtrl_SessionId_" + Guid.NewGuid())
            .Options;
        var controller = CreateController(sqlOptions: sqlOptions);

        // Act
        var result = await controller.TriggerRun("Test query", "my-custom-session-id");

        // Assert
        Assert.IsType<RedirectToActionResult>(result);
        using var ctx = new ArNirDbContext(sqlOptions);
        var log = ctx.AgentRunLogs.Single();
        Assert.Equal("my-custom-session-id", log.SessionId);
    }

    [Fact]
    public async Task Index_ReturnsViewWithLogs()
    {
        // Arrange — seed two logs
        var sqlOptions = new DbContextOptionsBuilder<ArNirDbContext>()
            .UseInMemoryDatabase("AgentCtrl_Index_" + Guid.NewGuid())
            .Options;

        using (var ctx = new ArNirDbContext(sqlOptions))
        {
            ctx.AgentRunLogs.Add(new AgentRunLog
            {
                Id            = Guid.NewGuid(),
                SessionId     = "sess-1",
                OriginalQuery = "Query 1",
                PlanJson      = "{}",
                Status        = "Completed",
                CreatedAt     = DateTime.UtcNow
            });
            ctx.AgentRunLogs.Add(new AgentRunLog
            {
                Id            = Guid.NewGuid(),
                SessionId     = "sess-2",
                OriginalQuery = "Query 2",
                PlanJson      = "{}",
                Status        = "Failed",
                CreatedAt     = DateTime.UtcNow.AddMinutes(-5)
            });
            await ctx.SaveChangesAsync();
        }

        var controller = CreateController(sqlOptions: sqlOptions);

        // Act
        var result = await controller.Index();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsAssignableFrom<IEnumerable<AgentRunLog>>(viewResult.Model);
        Assert.Equal(2, model.Count());
    }
}
