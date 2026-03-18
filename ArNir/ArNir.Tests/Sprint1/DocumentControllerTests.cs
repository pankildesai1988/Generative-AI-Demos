using ArNir.Admin.Controllers;
using ArNir.Core.Config;
using ArNir.Core.DTOs.Documents;
using ArNir.RAG.Hosting;
using ArNir.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace ArNir.Tests.Sprint1;

public class DocumentControllerTests
{
    private readonly DocumentController _controller;
    private readonly Mock<IDocumentService> _docServiceMock;
    private readonly FileUploadSettings _settings;

    public DocumentControllerTests()
    {
        _docServiceMock = new Mock<IDocumentService>();
        var queueMock = new IngestionQueue();
        var loggerMock = new Mock<ILogger<DocumentController>>();
        _settings = new FileUploadSettings
        {
            MaxFileSize = 5_242_880, // 5 MB
            AllowedTypes = new[] { "application/pdf", "text/plain" }
        };
        var optionsMock = Options.Create(_settings);

        _controller = new DocumentController(
            _docServiceMock.Object, queueMock, loggerMock.Object, optionsMock);

        var httpContext = new DefaultHttpContext();
        _controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
        _controller.TempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());
    }

    [Fact]
    public async Task Upload_POST_NullFile_ReturnsViewWithError()
    {
        // Arrange
        var dto = new DocumentUploadDto { File = null! };

        // Act
        var result = await _controller.Upload(dto);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.False(_controller.ModelState.IsValid);
        Assert.True(_controller.ModelState.ContainsKey("File"));
    }

    [Fact]
    public async Task Upload_POST_OversizedFile_ReturnsViewWithError()
    {
        // Arrange - create a file mock larger than max
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.Length).Returns(10_000_000); // 10 MB
        fileMock.Setup(f => f.ContentType).Returns("application/pdf");

        var dto = new DocumentUploadDto { File = fileMock.Object };

        // Act
        var result = await _controller.Upload(dto);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.False(_controller.ModelState.IsValid);
        Assert.True(_controller.ModelState.ContainsKey("File"));
    }

    [Fact]
    public async Task Upload_POST_InvalidContentType_ReturnsViewWithError()
    {
        // Arrange
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.Length).Returns(1000);
        fileMock.Setup(f => f.ContentType).Returns("image/png"); // not allowed

        var dto = new DocumentUploadDto { File = fileMock.Object };

        // Act
        var result = await _controller.Upload(dto);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.False(_controller.ModelState.IsValid);
        Assert.True(_controller.ModelState.ContainsKey("File"));
    }
}
