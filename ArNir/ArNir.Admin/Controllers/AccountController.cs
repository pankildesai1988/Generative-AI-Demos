using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ArNir.Admin.Controllers;

/// <summary>
/// Handles cookie-based authentication for the ArNir Admin panel.
/// <para>
/// Credentials are configured in <c>appsettings.json</c> under the <c>Auth</c> section:
/// <code>
/// "Auth": {
///   "AdminUsername": "admin",
///   "AdminPassword": "Admin@123"
/// }
/// </code>
/// For production, replace plain-text passwords with a hashed credential store or an
/// identity provider (Azure AD, Auth0, etc.).
/// </para>
/// </summary>
public class AccountController : Controller
{
    private readonly IConfiguration _config;

    /// <summary>Initialises a new instance of <see cref="AccountController"/>.</summary>
    public AccountController(IConfiguration config)
    {
        _config = config;
    }

    // GET /Account/Login
    /// <summary>Displays the login form.</summary>
    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
            return Redirect(returnUrl ?? "/");

        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    // POST /Account/Login
    /// <summary>Validates credentials and issues the authentication cookie.</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(string username, string password, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;

        var expectedUser = _config["Auth:AdminUsername"] ?? "admin";
        var expectedPass = _config["Auth:AdminPassword"] ?? "Admin@123";

        if (!string.Equals(username, expectedUser, StringComparison.OrdinalIgnoreCase) ||
            !string.Equals(password, expectedPass, StringComparison.Ordinal))
        {
            ModelState.AddModelError(string.Empty, "Invalid username or password.");
            return View();
        }

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name,  username),
            new Claim(ClaimTypes.Role,  "Admin")
        };

        var identity  = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc   = DateTimeOffset.UtcNow.AddHours(8)
            });

        return Redirect(returnUrl ?? "/");
    }

    // POST /Account/Logout
    /// <summary>Signs the user out and redirects to the login page.</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction(nameof(Login));
    }

    // GET /Account/AccessDenied
    /// <summary>Shown when an authenticated user lacks the required role.</summary>
    [HttpGet]
    public IActionResult AccessDenied() => View();
}
