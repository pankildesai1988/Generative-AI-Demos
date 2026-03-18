using ArNir.Admin.Controllers;
using ArNir.Core.Entities;
using ArNir.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using System.Text;
using System.Text.Json;
using Xunit;

namespace ArNir.Tests.Sprint4;

/// <summary>Unit tests for the Sprint 4 Export/Import actions of <see cref="PromptTemplateController"/>.</summary>
public class PromptTemplateControllerTests
{
    private static PromptTemplateController CreateController(
        DbContextOptions<ArNirDbContext>? sqlOptions = null)
    {
        var resolvedOptions = sqlOptions ?? new DbContextOptionsBuilder<ArNirDbContext>()
            .UseInMemoryDatabase("PromptTemplCtrl_" + Guid.NewGuid())
            .Options;

        var dbFactoryMock = new Mock<IDbContextFactory<ArNirDbContext>>();
        dbFactoryMock.Setup(f => f.CreateDbContextAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => new ArNirDbContext(resolvedOptions));

        var logger = new Mock<ILogger<PromptTemplateController>>();

        var controller = new PromptTemplateController(
            dbFactoryMock.Object,
            logger.Object);

        var httpContext = new DefaultHttpContext();
        controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
        controller.TempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());

        return controller;
    }

    [Fact]
    public async Task ExportJson_ReturnsFileResult_WithJsonContentType()
    {
        // Arrange
        var controller = CreateController();

        // Act
        var result = await controller.ExportJson();

        // Assert
        var fileResult = Assert.IsType<FileContentResult>(result);
        Assert.Equal("application/json", fileResult.ContentType);
        Assert.Equal("prompt-templates.json", fileResult.FileDownloadName);
    }

    [Fact]
    public async Task ExportJson_ContainsAllActiveTemplates()
    {
        // Arrange — seed two templates
        var sqlOptions = new DbContextOptionsBuilder<ArNirDbContext>()
            .UseInMemoryDatabase("PromptTemplCtrl_Export_" + Guid.NewGuid())
            .Options;

        using (var ctx = new ArNirDbContext(sqlOptions))
        {
            ctx.PromptTemplates.AddRange(
                new PromptTemplateEntity
                {
                    Id           = Guid.NewGuid(),
                    Style        = "rag",
                    Name         = "RAG Template v1",
                    TemplateText = "Answer based on {{CONTEXT}}: {{QUERY}}",
                    Version      = 1,
                    IsActive     = true,
                    Source       = "Database",
                    CreatedAt    = DateTime.UtcNow
                },
                new PromptTemplateEntity
                {
                    Id           = Guid.NewGuid(),
                    Style        = "zero-shot",
                    Name         = "Zero-Shot v1",
                    TemplateText = "Answer: {{QUERY}}",
                    Version      = 1,
                    IsActive     = false,
                    Source       = "Database",
                    CreatedAt    = DateTime.UtcNow
                });
            await ctx.SaveChangesAsync();
        }

        var controller = CreateController(sqlOptions);

        // Act
        var result = await controller.ExportJson();

        // Assert — both templates exported (active and inactive)
        var fileResult = Assert.IsType<FileContentResult>(result);
        var json       = Encoding.UTF8.GetString(fileResult.FileContents);
        var templates  = JsonSerializer.Deserialize<List<PromptTemplateEntity>>(json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        Assert.NotNull(templates);
        Assert.Equal(2, templates!.Count);
    }

    [Fact]
    public async Task ImportJson_NullFile_RedirectsWithError()
    {
        // Arrange
        var controller = CreateController();

        // Act — pass null file
        var result = await controller.ImportJson(null);

        // Assert
        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirect.ActionName);
        Assert.NotNull(controller.TempData["Error"]);
    }

    [Fact]
    public async Task ImportJson_ValidJson_InsertsNewTemplates()
    {
        // Arrange — empty DB
        var sqlOptions = new DbContextOptionsBuilder<ArNirDbContext>()
            .UseInMemoryDatabase("PromptTemplCtrl_Import_" + Guid.NewGuid())
            .Options;

        var controller = CreateController(sqlOptions);

        var templates = new List<PromptTemplateEntity>
        {
            new() { Id = Guid.NewGuid(), Style = "rag",       Name = "RAG v1",  TemplateText = "T1", Version = 1, IsActive = true,  Source = "Database", CreatedAt = DateTime.UtcNow },
            new() { Id = Guid.NewGuid(), Style = "zero-shot", Name = "ZS v1",   TemplateText = "T2", Version = 1, IsActive = true,  Source = "Database", CreatedAt = DateTime.UtcNow }
        };

        var jsonBytes = JsonSerializer.SerializeToUtf8Bytes(templates);
        var formFile  = CreateFormFile(jsonBytes, "templates.json", "application/json");

        // Act
        var result = await controller.ImportJson(formFile);

        // Assert
        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirect.ActionName);

        var successMsg = controller.TempData["Success"]?.ToString();
        Assert.NotNull(successMsg);
        Assert.Contains("2", successMsg);   // 2 inserted

        using var verifyCtx = new ArNirDbContext(sqlOptions);
        Assert.Equal(2, verifyCtx.PromptTemplates.Count());
    }

    [Fact]
    public async Task ImportJson_DuplicateStyleVersion_SkipsExisting()
    {
        // Arrange — seed one existing template
        var sqlOptions = new DbContextOptionsBuilder<ArNirDbContext>()
            .UseInMemoryDatabase("PromptTemplCtrl_Skip_" + Guid.NewGuid())
            .Options;

        using (var ctx = new ArNirDbContext(sqlOptions))
        {
            ctx.PromptTemplates.Add(new PromptTemplateEntity
            {
                Id           = Guid.NewGuid(),
                Style        = "rag",
                Name         = "Existing RAG v1",
                TemplateText = "Existing text",
                Version      = 1,
                IsActive     = true,
                Source       = "Database",
                CreatedAt    = DateTime.UtcNow
            });
            await ctx.SaveChangesAsync();
        }

        var controller = CreateController(sqlOptions);

        // Import contains 1 duplicate + 1 new
        var toImport = new List<PromptTemplateEntity>
        {
            new() { Id = Guid.NewGuid(), Style = "rag",       Name = "Dup RAG v1", TemplateText = "Dup", Version = 1, IsActive = true, Source = "Database", CreatedAt = DateTime.UtcNow },
            new() { Id = Guid.NewGuid(), Style = "few-shot",  Name = "FS v1",      TemplateText = "New", Version = 1, IsActive = true, Source = "Database", CreatedAt = DateTime.UtcNow }
        };

        var jsonBytes = JsonSerializer.SerializeToUtf8Bytes(toImport);
        var formFile  = CreateFormFile(jsonBytes, "import.json", "application/json");

        // Act
        var result = await controller.ImportJson(formFile);

        // Assert — 1 skipped, 1 inserted; total = 2 in DB
        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirect.ActionName);

        var successMsg = controller.TempData["Success"]?.ToString();
        Assert.NotNull(successMsg);
        Assert.Contains("1 skipped", successMsg);

        using var verifyCtx = new ArNirDbContext(sqlOptions);
        Assert.Equal(2, verifyCtx.PromptTemplates.Count());
    }

    // ── Helpers ────────────────────────────────────────────────────────────────

    private static IFormFile CreateFormFile(byte[] content, string fileName, string contentType)
    {
        var ms       = new MemoryStream(content);
        var formFile = new FormFile(ms, 0, content.Length, "file", fileName)
        {
            Headers     = new HeaderDictionary(),
            ContentType = contentType
        };
        return formFile;
    }
}
