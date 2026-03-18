using ArNir.Api.Controllers;
using ArNir.Core.DTOs.Documents;
using ArNir.RAG.Hosting;
using ArNir.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace ArNir.Tests.Sprint5;

/// <summary>
/// Unit tests for <see cref="DocumentIngestController"/> (ArNir.API).
/// Validates the dual-path ingestion: SQL save via IDocumentService +
/// background RAG queue via IngestionQueue.
/// </summary>
public class DocumentIngestControllerApiTests
{
    private readonly DocumentIngestController _controller;
    private readonly Mock<IDocumentService> _docServiceMock;
    private readonly IngestionQueue _ingestionQueue;

    public DocumentIngestControllerApiTests()
    {
        _docServiceMock = new Mock<IDocumentService>();
        _ingestionQueue = new IngestionQueue();
        var loggerMock = new Mock<ILogger<DocumentIngestController>>();

        _controller = new DocumentIngestController(
            _docServiceMock.Object, _ingestionQueue, loggerMock.Object);

        var httpContext = new DefaultHttpContext();
        _controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
    }

    [Fact]
    public async Task Ingest_NoFile_ReturnsBadRequest()
    {
        // Act
        var result = await _controller.Ingest(null!);

        // Assert
        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("No file uploaded.", badRequest.Value);
    }

    [Fact]
    public async Task Ingest_EmptyFile_ReturnsBadRequest()
    {
        // Arrange
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.Length).Returns(0);

        // Act
        var result = await _controller.Ingest(fileMock.Object);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task Ingest_ValidFile_SavesDocumentToSql()
    {
        // Arrange
        var fileMock = CreateValidFileMock();
        _docServiceMock
            .Setup(s => s.UploadDocumentAsync(It.IsAny<DocumentUploadDto>()))
            .ReturnsAsync(new DocumentResponseDto { Id = 42, Name = "test.pdf" });

        // Act
        await _controller.Ingest(fileMock.Object);

        // Assert — IDocumentService.UploadDocumentAsync was called exactly once
        _docServiceMock.Verify(
            s => s.UploadDocumentAsync(It.IsAny<DocumentUploadDto>()),
            Times.Once);
    }

    [Fact]
    public async Task Ingest_ValidFile_EnqueuesToQueue_Returns202()
    {
        // Arrange
        var fileMock = CreateValidFileMock();
        _docServiceMock
            .Setup(s => s.UploadDocumentAsync(It.IsAny<DocumentUploadDto>()))
            .ReturnsAsync(new DocumentResponseDto { Id = 7, Name = "report.docx" });

        // Act
        var result = await _controller.Ingest(fileMock.Object);

        // Assert — 202 Accepted
        var accepted = Assert.IsType<AcceptedResult>(result);
        Assert.NotNull(accepted.Value);

        // Assert — job was enqueued (queue depth should be 1)
        Assert.Equal(1, _ingestionQueue.QueueDepth);
    }

    [Fact]
    public async Task Ingest_ValidFile_EnqueuedJobHasCorrectSqlDocId()
    {
        // Arrange
        var fileMock = CreateValidFileMock();
        _docServiceMock
            .Setup(s => s.UploadDocumentAsync(It.IsAny<DocumentUploadDto>()))
            .ReturnsAsync(new DocumentResponseDto { Id = 99, Name = "data.txt" });

        // Act
        await _controller.Ingest(fileMock.Object);

        // Assert — dequeue and verify the LegacySqlDocumentId
        var job = await _ingestionQueue.DequeueAsync(CancellationToken.None);
        Assert.Equal(99, job.Request.LegacySqlDocumentId);
        Assert.Equal("data.txt", job.DocumentName);
    }

    /// <summary>Creates a valid IFormFile mock with 100 bytes of content.</summary>
    private static Mock<IFormFile> CreateValidFileMock()
    {
        var content = new byte[100];
        var stream = new MemoryStream(content);
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.Length).Returns(100);
        fileMock.Setup(f => f.FileName).Returns("data.txt");
        fileMock.Setup(f => f.ContentType).Returns("text/plain");
        fileMock.Setup(f => f.OpenReadStream()).Returns(stream);
        fileMock.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
            .Returns<Stream, CancellationToken>((target, _) =>
            {
                stream.Position = 0;
                return stream.CopyToAsync(target);
            });
        return fileMock;
    }
}
