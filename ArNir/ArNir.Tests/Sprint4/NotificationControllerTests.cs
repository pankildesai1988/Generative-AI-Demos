using ArNir.Admin.Controllers;
using ArNir.Core.Entities;
using ArNir.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using System.Text.Json;
using Xunit;

namespace ArNir.Tests.Sprint4;

/// <summary>Unit tests for the Sprint 4 <see cref="NotificationController"/>.</summary>
public class NotificationControllerTests
{
    private static NotificationController CreateController(
        DbContextOptions<ArNirDbContext>? sqlOptions = null,
        bool throwOnFactory = false)
    {
        var resolvedOptions = sqlOptions ?? new DbContextOptionsBuilder<ArNirDbContext>()
            .UseInMemoryDatabase("NotifCtrl_" + Guid.NewGuid())
            .Options;

        var dbFactoryMock = new Mock<IDbContextFactory<ArNirDbContext>>();

        if (throwOnFactory)
        {
            dbFactoryMock.Setup(f => f.CreateDbContextAsync(It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("DB unavailable in test"));
        }
        else
        {
            dbFactoryMock.Setup(f => f.CreateDbContextAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(() => new ArNirDbContext(resolvedOptions));
        }

        var logger = new Mock<ILogger<NotificationController>>();

        var controller = new NotificationController(
            dbFactoryMock.Object,
            logger.Object);

        var httpContext = new DefaultHttpContext();
        controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
        controller.TempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());

        return controller;
    }

    /// <summary>Serialize the JsonResult value and return it as a JsonDocument.</summary>
    private static JsonDocument ToDoc(JsonResult result)
    {
        var json = JsonSerializer.Serialize(result.Value);
        return JsonDocument.Parse(json);
    }

    [Fact]
    public async Task GetUnread_NoBreaches_ReturnsCountZero()
    {
        // Arrange — DB has metric events but all within SLA
        var sqlOptions = new DbContextOptionsBuilder<ArNirDbContext>()
            .UseInMemoryDatabase("NotifCtrl_NoBreaches_" + Guid.NewGuid())
            .Options;

        using (var ctx = new ArNirDbContext(sqlOptions))
        {
            ctx.MetricEvents.Add(new MetricEventEntity
            {
                EventType    = "LlmCall",
                Provider     = "OpenAI",
                Model        = "gpt-4o",
                LatencyMs    = 200,
                IsWithinSla  = true,
                TokensUsed   = 100,
                OccurredAt   = DateTime.UtcNow.AddMinutes(-10)
            });
            await ctx.SaveChangesAsync();
        }

        var controller = CreateController(sqlOptions);

        // Act
        var result = await controller.GetUnread();

        // Assert
        var jsonResult = Assert.IsType<JsonResult>(result);
        using var doc  = ToDoc(jsonResult);
        Assert.Equal(0, doc.RootElement.GetProperty("count").GetInt32());
    }

    [Fact]
    public async Task GetUnread_WithBreaches_ReturnsCorrectCount()
    {
        // Arrange — seed 3 SLA breaches in the last hour
        var sqlOptions = new DbContextOptionsBuilder<ArNirDbContext>()
            .UseInMemoryDatabase("NotifCtrl_WithBreaches_" + Guid.NewGuid())
            .Options;

        using (var ctx = new ArNirDbContext(sqlOptions))
        {
            for (int i = 0; i < 3; i++)
            {
                ctx.MetricEvents.Add(new MetricEventEntity
                {
                    EventType   = "LlmCall",
                    Provider    = "OpenAI",
                    Model       = "gpt-4o",
                    LatencyMs   = 5000 + i * 100,
                    IsWithinSla = false,
                    TokensUsed  = 200,
                    OccurredAt  = DateTime.UtcNow.AddMinutes(-(i + 1))
                });
            }
            // Old breach (> 1 hour) — must not be counted
            ctx.MetricEvents.Add(new MetricEventEntity
            {
                EventType   = "LlmCall",
                Provider    = "Gemini",
                Model       = "gemini-1.5-pro",
                LatencyMs   = 9000,
                IsWithinSla = false,
                TokensUsed  = 150,
                OccurredAt  = DateTime.UtcNow.AddHours(-2)
            });
            await ctx.SaveChangesAsync();
        }

        var controller = CreateController(sqlOptions);

        // Act
        var result = await controller.GetUnread();

        // Assert
        var jsonResult = Assert.IsType<JsonResult>(result);
        using var doc  = ToDoc(jsonResult);
        Assert.Equal(3, doc.RootElement.GetProperty("count").GetInt32());
    }

    [Fact]
    public async Task GetUnread_ReturnsOnly5Alerts_WhenMoreExist()
    {
        // Arrange — seed 8 SLA breaches
        var sqlOptions = new DbContextOptionsBuilder<ArNirDbContext>()
            .UseInMemoryDatabase("NotifCtrl_Max5_" + Guid.NewGuid())
            .Options;

        using (var ctx = new ArNirDbContext(sqlOptions))
        {
            for (int i = 0; i < 8; i++)
            {
                ctx.MetricEvents.Add(new MetricEventEntity
                {
                    EventType   = "LlmCall",
                    Provider    = "OpenAI",
                    Model       = "gpt-4o-mini",
                    LatencyMs   = 6000 + i * 50,
                    IsWithinSla = false,
                    TokensUsed  = 100,
                    OccurredAt  = DateTime.UtcNow.AddMinutes(-(i + 1))
                });
            }
            await ctx.SaveChangesAsync();
        }

        var controller = CreateController(sqlOptions);

        // Act
        var result = await controller.GetUnread();

        // Assert — count = 8, but alerts array = 5
        var jsonResult = Assert.IsType<JsonResult>(result);
        using var doc  = ToDoc(jsonResult);
        Assert.Equal(8, doc.RootElement.GetProperty("count").GetInt32());
        Assert.Equal(5, doc.RootElement.GetProperty("alerts").GetArrayLength());
    }

    [Fact]
    public async Task GetUnread_DbFailure_ReturnsZeroGracefully()
    {
        // Arrange — factory is configured to throw
        var controller = CreateController(throwOnFactory: true);

        // Act — should NOT throw; error is swallowed
        var result = await controller.GetUnread();

        // Assert
        var jsonResult = Assert.IsType<JsonResult>(result);
        using var doc  = ToDoc(jsonResult);
        Assert.Equal(0, doc.RootElement.GetProperty("count").GetInt32());
    }
}
