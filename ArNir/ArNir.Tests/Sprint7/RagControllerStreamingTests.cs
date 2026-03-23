using ArNir.Api.Controllers;
using ArNir.Core.DTOs.RAG;
using ArNir.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Text;
using Xunit;

namespace ArNir.Tests.Sprint7;

public class RagControllerStreamingTests
{
    [Fact]
    public async Task Stream_ValidRequest_WritesTokenMetadataAndCompleteEvents()
    {
        var ragServiceMock = new Mock<IRagService>();
        ragServiceMock
            .Setup(service => service.RunRagAsync(
                "Explain revenue",
                4,
                true,
                "rag",
                true,
                "OpenAI",
                "gpt-4o-mini",
                It.IsAny<IEnumerable<int>>()))
            .ReturnsAsync(new RagResultDto
            {
                RagAnswer = "Revenue increased steadily.",
                HistoryId = 42,
                RetrievedChunks = new List<RagChunkDto>
                {
                    new() { DocumentId = 1, DocumentTitle = "Annual Report", ChunkText = "Revenue increased steadily." }
                }
            });

        var controller = new RagController(ragServiceMock.Object);
        var responseStream = new MemoryStream();
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                Response = { Body = responseStream },
            },
        };

        await controller.Stream(
            new RagRequestDto
            {
                Query = "Explain revenue",
                TopK = 4,
                UseHybrid = true,
                PromptStyle = "rag",
                SaveAsNew = true,
                Provider = "OpenAI",
                Model = "gpt-4o-mini",
            },
            CancellationToken.None);

        responseStream.Position = 0;
        var body = Encoding.UTF8.GetString(responseStream.ToArray());

        Assert.Equal("text/event-stream", controller.Response.ContentType);
        Assert.Contains("event: token", body);
        Assert.Contains("event: metadata", body);
        Assert.Contains("\"historyId\":42", body);
        Assert.Contains("event: complete", body);
    }

    [Fact]
    public async Task Stream_RagServiceThrows_WritesErrorEvent()
    {
        var ragServiceMock = new Mock<IRagService>();
        ragServiceMock
            .Setup(service => service.RunRagAsync(
                It.IsAny<string>(),
                It.IsAny<int>(),
                It.IsAny<bool>(),
                It.IsAny<string>(),
                It.IsAny<bool>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<IEnumerable<int>>()))
            .ThrowsAsync(new InvalidOperationException("Streaming failed."));

        var controller = new RagController(ragServiceMock.Object);
        var responseStream = new MemoryStream();
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                Response = { Body = responseStream },
            },
        };

        await controller.Stream(new RagRequestDto { Query = "Explain revenue" }, CancellationToken.None);

        responseStream.Position = 0;
        var body = Encoding.UTF8.GetString(responseStream.ToArray());

        Assert.Contains("event: error", body);
        Assert.Contains("Streaming failed.", body);
    }
}
