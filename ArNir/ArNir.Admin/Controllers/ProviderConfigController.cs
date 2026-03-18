using ArNir.Admin.Models;
using ArNir.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ArNir.Admin.Controllers;

[Authorize]
public class ProviderConfigController : Controller
{
    private readonly IPlatformSettingsService _settings;
    private readonly IConfiguration _config;
    private readonly ILogger<ProviderConfigController> _logger;

    private const string Module = "Providers";

    public ProviderConfigController(
        IPlatformSettingsService settings,
        IConfiguration config,
        ILogger<ProviderConfigController> logger)
    {
        _settings = settings;
        _config = config;
        _logger = logger;
    }

    // GET /ProviderConfig
    public async Task<IActionResult> Index()
    {
        // Read from DB, fall back to config
        var apiKey = await _settings.GetAsync(Module, "OpenAI:ApiKey")
                     ?? _config["OpenAI:ApiKey"]
                     ?? "";

        var embeddingModel = await _settings.GetAsync(Module, "OpenAI:EmbeddingModel")
                             ?? _config["OpenAI:EmbeddingModel"]
                             ?? "text-embedding-ada-002";

        var chatModel = await _settings.GetAsync(Module, "OpenAI:ChatModel")
                        ?? _config["OpenAI:ChatModel"]
                        ?? "gpt-4o-mini";

        var keyIsSet = !string.IsNullOrEmpty(apiKey) && apiKey != "sk-your-openai-key";

        // Mask the API key: show first 8 chars + ****
        var masked = keyIsSet && apiKey.Length >= 8
            ? apiKey[..8] + "****"
            : keyIsSet ? "sk-****" : "(not configured)";

        var vm = new ProviderConfigViewModel
        {
            OpenAiApiKeyMasked  = masked,
            OpenAiEmbeddingModel = embeddingModel,
            OpenAiChatModel     = chatModel,
            OpenAiKeyIsSet      = keyIsSet
        };

        return View(vm);
    }

    // POST /ProviderConfig/Update
    [HttpPost]
    public async Task<IActionResult> Update(string key, string value)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            TempData["Error"] = "Setting key cannot be empty.";
            return RedirectToAction(nameof(Index));
        }

        try
        {
            await _settings.SetAsync(Module, key, value ?? string.Empty);
            TempData["Success"] = "Provider setting updated.";
            _logger.LogInformation("Provider setting '{Key}' updated by admin.", key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating provider setting '{Key}'.", key);
            TempData["Error"] = "Error updating setting: " + ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }
}
