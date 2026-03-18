using ArNir.Admin.Controllers;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Configuration;
using Moq;
using System.Security.Claims;
using Xunit;

namespace ArNir.Tests.Sprint1;

public class AccountControllerTests
{
    private readonly AccountController _controller;

    public AccountControllerTests()
    {
        var configData = new Dictionary<string, string?>
        {
            { "Auth:AdminUsername", "admin" },
            { "Auth:AdminPassword", "Admin@123" }
        };
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(configData)
            .Build();

        _controller = new AccountController(config);

        // Setup HttpContext with unauthenticated user and auth service mock
        var authServiceMock = new Mock<IAuthenticationService>();
        authServiceMock
            .Setup(a => a.SignInAsync(
                It.IsAny<HttpContext>(),
                It.IsAny<string>(),
                It.IsAny<ClaimsPrincipal>(),
                It.IsAny<AuthenticationProperties>()))
            .Returns(Task.CompletedTask);

        var serviceProviderMock = new Mock<IServiceProvider>();
        serviceProviderMock
            .Setup(s => s.GetService(typeof(IAuthenticationService)))
            .Returns(authServiceMock.Object);

        var httpContext = new DefaultHttpContext();
        httpContext.User = new ClaimsPrincipal(new ClaimsIdentity());
        httpContext.RequestServices = serviceProviderMock.Object;

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };
        _controller.TempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());
    }

    [Fact]
    public void Login_GET_ReturnsView()
    {
        // Act
        var result = _controller.Login();

        // Assert
        Assert.IsType<ViewResult>(result);
    }

    [Fact]
    public async Task Login_POST_CorrectCredentials_Redirects()
    {
        // Act
        var result = await _controller.Login("admin", "Admin@123", null);

        // Assert
        var redirect = Assert.IsType<RedirectResult>(result);
        Assert.Equal("/", redirect.Url);
    }

    [Fact]
    public async Task Login_POST_WrongCredentials_ReturnsViewWithError()
    {
        // Act
        var result = await _controller.Login("admin", "wrong-password", null);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.False(_controller.ModelState.IsValid);
        Assert.True(_controller.ModelState.ContainsKey(string.Empty));
    }
}
