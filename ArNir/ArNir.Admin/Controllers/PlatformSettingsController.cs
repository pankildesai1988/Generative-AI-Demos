using ArNir.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ArNir.Admin.Controllers;

/// <summary>
/// Admin panel for viewing and updating all <c>PlatformSettings</c> key-value pairs.
/// Settings are grouped by module so operators can see at a glance what each
/// subsystem (RAG, AI, Prompts, Observability) is configured to use.
/// </summary>
[Authorize]
public class PlatformSettingsController : Controller
{
    private static readonly string[] Modules = { "RAG", "AI", "Prompts", "Observability" };

    private readonly IPlatformSettingsService          _settings;
    private readonly ILogger<PlatformSettingsController> _logger;

    /// <summary>Initialises the controller.</summary>
    public PlatformSettingsController(
        IPlatformSettingsService settings,
        ILogger<PlatformSettingsController> logger)
    {
        _settings = settings;
        _logger   = logger;
    }

    // GET /PlatformSettings
    /// <summary>Displays all settings grouped by module.</summary>
    public async Task<IActionResult> Index()
    {
        var grouped = new Dictionary<string, IReadOnlyList<(string Key, string Value)>>();
        foreach (var module in Modules)
            grouped[module] = await _settings.GetModuleSettingsAsync(module);
        return View(grouped);
    }

    // POST /PlatformSettings/Update
    /// <summary>
    /// Bulk-saves changes submitted from the inline edit table.
    /// The form posts a collection of <c>module</c>, <c>key</c>, <c>value</c> triples.
    /// </summary>
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(
        [FromForm] string[] module,
        [FromForm] string[] key,
        [FromForm] string[] value)
    {
        if (module.Length != key.Length || key.Length != value.Length)
            return BadRequest("Mismatched form arrays.");

        for (int i = 0; i < module.Length; i++)
        {
            await _settings.SetAsync(module[i], key[i], value[i]);
            _logger.LogInformation("PlatformSetting updated: [{Module}/{Key}] = '{Value}'.", module[i], key[i], value[i]);
        }

        TempData["Success"] = "Settings saved successfully.";
        return RedirectToAction(nameof(Index));
    }
}
