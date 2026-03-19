using ArNir.Admin.Controllers;
using ArNir.Core.Entities;
using ArNir.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace ArNir.Tests.Sprint8;

/// <summary>
/// Unit tests for Sprint 8 — Prompt Versioning features:
/// edit-creates-version, history, rollback, and compare.
/// </summary>
public class PromptVersioningTests
{
    private static (PromptTemplateController ctrl, string dbName) CreateController()
    {
        var dbName = Guid.NewGuid().ToString();
        var options = new DbContextOptionsBuilder<ArNirDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;

        var factoryMock = new Mock<IDbContextFactory<ArNirDbContext>>();
        factoryMock.Setup(f => f.CreateDbContextAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => new ArNirDbContext(options));
        factoryMock.Setup(f => f.CreateDbContext())
            .Returns(() => new ArNirDbContext(options));

        var loggerMock = new Mock<ILogger<PromptTemplateController>>();
        var ctrl = new PromptTemplateController(factoryMock.Object, loggerMock.Object);

        ctrl.TempData = new TempDataDictionary(
            new DefaultHttpContext(), Mock.Of<ITempDataProvider>());

        return (ctrl, dbName);
    }

    private static ArNirDbContext OpenDb(string dbName) =>
        new(new DbContextOptionsBuilder<ArNirDbContext>().UseInMemoryDatabase(dbName).Options);

    private static async Task SeedTemplates(string dbName)
    {
        using var db = OpenDb(dbName);
        db.PromptTemplates.AddRange(
            new PromptTemplateEntity { Id = Guid.NewGuid(), Style = "rag", Name = "RAG v1", TemplateText = "Original template v1", Version = 1, IsActive = false, Source = "Database", CreatedAt = DateTime.UtcNow.AddDays(-2) },
            new PromptTemplateEntity { Id = Guid.NewGuid(), Style = "rag", Name = "RAG v2", TemplateText = "Updated template v2", Version = 2, IsActive = true, Source = "Database", CreatedAt = DateTime.UtcNow.AddDays(-1) },
            new PromptTemplateEntity { Id = Guid.NewGuid(), Style = "zero-shot", Name = "ZS v1", TemplateText = "Zero shot template", Version = 1, IsActive = true, Source = "Database", CreatedAt = DateTime.UtcNow }
        );
        await db.SaveChangesAsync();
    }

    [Fact]
    public async Task Edit_CreatesNewVersion_DeactivatesOld()
    {
        var (ctrl, dbName) = CreateController();
        await SeedTemplates(dbName);

        Guid originalId;
        using (var db = OpenDb(dbName))
        {
            var original = await db.PromptTemplates.FirstAsync(x => x.Style == "rag" && x.Version == 2);
            originalId = original.Id;
        }

        var model = new PromptTemplateEntity
        {
            Id = originalId,
            Style = "rag",
            Name = "RAG v3 edited",
            TemplateText = "Completely new text",
            IsActive = true
        };

        var result = await ctrl.Edit(originalId, model);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirect.ActionName);

        // Verify via a fresh context
        using var verifyDb = OpenDb(dbName);
        var allRag = await verifyDb.PromptTemplates.Where(x => x.Style == "rag").ToListAsync();
        Assert.Equal(3, allRag.Count);

        var v3 = allRag.FirstOrDefault(x => x.Version == 3);
        Assert.NotNull(v3);
        Assert.True(v3!.IsActive);
        Assert.Equal("Completely new text", v3.TemplateText);

        var v2 = allRag.First(x => x.Version == 2);
        Assert.False(v2.IsActive);
    }

    [Fact]
    public async Task History_ReturnsVersionsForStyle()
    {
        var (ctrl, dbName) = CreateController();
        await SeedTemplates(dbName);

        var result = await ctrl.History("rag");

        var viewResult = Assert.IsType<ViewResult>(result);
        var versions = Assert.IsAssignableFrom<IEnumerable<PromptTemplateEntity>>(viewResult.Model);
        var list = versions.ToList();
        Assert.Equal(2, list.Count);
        Assert.Equal("rag", list.First().Style);
        Assert.True(list[0].Version > list[1].Version);
    }

    [Fact]
    public async Task History_EmptyStyle_RedirectsToIndex()
    {
        var (ctrl, _) = CreateController();

        var result = await ctrl.History("");

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirect.ActionName);
    }

    [Fact]
    public async Task Rollback_CreatesNewVersionFromOld_DeactivatesAll()
    {
        var (ctrl, dbName) = CreateController();
        await SeedTemplates(dbName);

        Guid v1Id;
        using (var db = OpenDb(dbName))
        {
            v1Id = (await db.PromptTemplates.FirstAsync(x => x.Style == "rag" && x.Version == 1)).Id;
        }

        var result = await ctrl.Rollback(v1Id);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("History", redirect.ActionName);

        using var verifyDb = OpenDb(dbName);
        var allRag = await verifyDb.PromptTemplates
            .Where(x => x.Style == "rag")
            .OrderBy(x => x.Version)
            .ToListAsync();
        Assert.Equal(3, allRag.Count);

        var newest = allRag.Last();
        Assert.Equal(3, newest.Version);
        Assert.True(newest.IsActive);
        Assert.Contains("Original template v1", newest.TemplateText);
        Assert.Contains("rollback from v1", newest.Name);

        // All others should be inactive
        Assert.All(allRag.Where(x => x.Version != newest.Version), x => Assert.False(x.IsActive));
    }

    [Fact]
    public async Task Rollback_NotFound_Returns404()
    {
        var (ctrl, _) = CreateController();

        var result = await ctrl.Rollback(Guid.NewGuid());

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Compare_ReturnsBothVersions()
    {
        var (ctrl, dbName) = CreateController();
        await SeedTemplates(dbName);

        Guid id1, id2;
        using (var db = OpenDb(dbName))
        {
            id1 = (await db.PromptTemplates.FirstAsync(x => x.Style == "rag" && x.Version == 1)).Id;
            id2 = (await db.PromptTemplates.FirstAsync(x => x.Style == "rag" && x.Version == 2)).Id;
        }

        var result = await ctrl.Compare(id1, id2);

        var viewResult = Assert.IsType<ViewResult>(result);
        var left = viewResult.ViewData["Left"] as PromptTemplateEntity;
        var right = viewResult.ViewData["Right"] as PromptTemplateEntity;
        Assert.NotNull(left);
        Assert.NotNull(right);
        Assert.Equal(1, left!.Version);
        Assert.Equal(2, right!.Version);
    }

    [Fact]
    public async Task Compare_MissingVersion_Returns404()
    {
        var (ctrl, dbName) = CreateController();
        await SeedTemplates(dbName);

        Guid id1;
        using (var db = OpenDb(dbName))
        {
            id1 = (await db.PromptTemplates.FirstAsync(x => x.Style == "rag" && x.Version == 1)).Id;
        }

        var result = await ctrl.Compare(id1, Guid.NewGuid());

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Edit_NotFound_Returns404()
    {
        var (ctrl, _) = CreateController();
        var fakeId = Guid.NewGuid();
        var model = new PromptTemplateEntity { Id = fakeId };

        var result = await ctrl.Edit(fakeId, model);

        Assert.IsType<NotFoundResult>(result);
    }
}
