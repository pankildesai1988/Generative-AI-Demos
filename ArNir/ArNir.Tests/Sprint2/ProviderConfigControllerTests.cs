using ArNir.Admin.Controllers;
using ArNir.Admin.Models;
using ArNir.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace ArNir.Tests.Sprint2;

public class ProviderConfigControllerTests
{
    private readonly Mock<IPlatformSettingsService> _settingsMock;
    private readonly ProviderConfigController _controller;

    public ProviderConfigControllerTests()
    {
        _settingsMock = new Mock<IPlatformSettingsService>();
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "OpenAI:ApiKey", "sk-test1234abcd" },
                { "OpenAI:EmbeddingModel", "text-embedding-ada-002" },
                { "OpenAI:ChatModel", "gpt-4o-mini" }
            })
            .Build();
        var loggerMock = new Mock<ILogger<ProviderConfigController>>();

        _controller = new ProviderConfigController(_settingsMock.Object, config, loggerMock.Object);

        var httpContext = new DefaultHttpContext();
        _controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
        _controller.TempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());
    }

    [Fact]
    public async Task Index_ReturnsProviderConfigViewModel()
    {
        // Arrange — DB returns null (falls back to config)
        _settingsMock.Setup(s => s.GetAsync("Providers", It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((string?)null);

        // Act
        var result = await _controller.Index();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<ProviderConfigViewModel>(viewResult.Model);
        Assert.True(model.OpenAiKeyIsSet);
        Assert.Equal("text-embedding-ada-002", model.OpenAiEmbeddingModel);
        Assert.Equal("gpt-4o-mini", model.OpenAiChatModel);
    }

    [Fact]
    public async Task Update_SavesSettingAndRedirects()
    {
        // Arrange
        _settingsMock.Setup(s => s.SetAsync("Providers", "OpenAI:ChatModel", "gpt-4", null, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.Update("OpenAI:ChatModel", "gpt-4");

        // Assert
        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirect.ActionName);
        _settingsMock.Verify(s => s.SetAsync("Providers", "OpenAI:ChatModel", "gpt-4", null, It.IsAny<CancellationToken>()), Times.Once);
    }
}
