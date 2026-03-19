using ArNir.Admin.Models;
using ArNir.Core.Entities;
using ArNir.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace ArNir.Admin.Controllers;

/// <summary>
/// Admin CRUD panel for <see cref="PromptTemplateEntity"/> records.
/// Operators can create, edit, and soft-delete versioned prompt templates
/// so the 3-layer resolver (DB → Config → Code) serves the correct template at runtime.
/// </summary>
[Authorize]
public class PromptTemplateController : Controller
{
    private readonly IDbContextFactory<ArNirDbContext> _dbFactory;
    private readonly ILogger<PromptTemplateController> _logger;

    /// <summary>Initialises the controller.</summary>
    public PromptTemplateController(
        IDbContextFactory<ArNirDbContext> dbFactory,
        ILogger<PromptTemplateController> logger)
    {
        _dbFactory = dbFactory;
        _logger    = logger;
    }

    // GET /PromptTemplate
    /// <summary>Lists all prompt templates (active first, then by style and version descending).</summary>
    public async Task<IActionResult> Index()
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        var templates = await db.PromptTemplates
            .AsNoTracking()
            .OrderByDescending(x => x.IsActive)
            .ThenBy(x => x.Style)
            .ThenByDescending(x => x.Version)
            .ToListAsync();
        return View(templates);
    }

    // GET /PromptTemplate/Create
    /// <summary>Renders the blank create form.</summary>
    public IActionResult Create() => View("CreateEdit", new PromptTemplateEntity());

    // POST /PromptTemplate/Create
    /// <summary>Saves a new prompt template.</summary>
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(PromptTemplateEntity model)
    {
        if (!ModelState.IsValid) return View("CreateEdit", model);

        await using var db = await _dbFactory.CreateDbContextAsync();
        // Set version to max + 1 for this style
        var max = await db.PromptTemplates
            .Where(x => x.Style == model.Style)
            .Select(x => (int?)x.Version)
            .MaxAsync() ?? 0;

        model.Id        = Guid.NewGuid();
        model.Version   = max + 1;
        model.Source    = "Database";
        model.CreatedAt = DateTime.UtcNow;

        db.PromptTemplates.Add(model);
        await db.SaveChangesAsync();
        _logger.LogInformation("PromptTemplate created: style={Style}, v{Version}.", model.Style, model.Version);
        return RedirectToAction(nameof(Index));
    }

    // GET /PromptTemplate/Edit/{id}
    /// <summary>Renders the edit form for an existing template.</summary>
    public async Task<IActionResult> Edit(Guid id)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        var entity = await db.PromptTemplates.FindAsync(id);
        if (entity == null) return NotFound();
        return View("CreateEdit", entity);
    }

    // POST /PromptTemplate/Edit/{id}
    /// <summary>
    /// Creates a new version instead of modifying in-place.
    /// The old version is deactivated, and a new row is inserted with Version = max + 1.
    /// </summary>
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, PromptTemplateEntity model)
    {
        if (id != model.Id) return BadRequest();
        if (!ModelState.IsValid) return View("CreateEdit", model);

        await using var db = await _dbFactory.CreateDbContextAsync();
        var existing = await db.PromptTemplates.FindAsync(id);
        if (existing == null) return NotFound();

        // Deactivate the old version
        existing.IsActive = false;

        // Create a new version
        var maxVersion = await db.PromptTemplates
            .Where(x => x.Style == existing.Style)
            .Select(x => (int?)x.Version)
            .MaxAsync() ?? 0;

        var newVersion = new PromptTemplateEntity
        {
            Id           = Guid.NewGuid(),
            Style        = model.Style,
            Name         = model.Name,
            TemplateText = model.TemplateText,
            Version      = maxVersion + 1,
            IsActive     = model.IsActive,
            Source       = "Database",
            CreatedAt    = DateTime.UtcNow
        };
        db.PromptTemplates.Add(newVersion);

        await db.SaveChangesAsync();
        _logger.LogInformation(
            "PromptTemplate versioned: style={Style}, v{OldVersion} -> v{NewVersion}.",
            existing.Style, existing.Version, newVersion.Version);
        TempData["Success"] = $"New version v{newVersion.Version} created for style '{newVersion.Style}'.";
        return RedirectToAction(nameof(Index));
    }

    // POST /PromptTemplate/Delete/{id}
    /// <summary>Soft-deletes a template by marking it inactive.</summary>
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        var entity = await db.PromptTemplates.FindAsync(id);
        if (entity == null) return NotFound();

        entity.IsActive = false;
        await db.SaveChangesAsync();
        _logger.LogInformation("PromptTemplate soft-deleted: id={Id}.", id);
        return RedirectToAction(nameof(Index));
    }

    // GET /PromptTemplate/ExportJson
    /// <summary>
    /// Exports all prompt templates as an indented JSON file download.
    /// </summary>
    public async Task<IActionResult> ExportJson()
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        var templates = await db.PromptTemplates
            .AsNoTracking()
            .OrderBy(x => x.Style)
            .ThenBy(x => x.Version)
            .ToListAsync();

        var options = new JsonSerializerOptions { WriteIndented = true };
        var bytes   = JsonSerializer.SerializeToUtf8Bytes(templates, options);
        return File(bytes, "application/json", "prompt-templates.json");
    }

    // POST /PromptTemplate/ImportJson
    /// <summary>
    /// Imports prompt templates from a JSON file upload.
    /// Templates whose <see cref="PromptTemplateEntity.Style"/> + <see cref="PromptTemplateEntity.Version"/>
    /// combination already exists are skipped; all others are inserted as new rows.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ImportJson(IFormFile? file)
    {
        if (file == null || file.Length == 0)
        {
            TempData["Error"] = "Please select a valid JSON file.";
            return RedirectToAction(nameof(Index));
        }

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (ext != ".json" && !(file.ContentType?.Contains("json") ?? false))
        {
            TempData["Error"] = "Only .json files are accepted.";
            return RedirectToAction(nameof(Index));
        }

        List<PromptTemplateEntity> imported;
        try
        {
            using var stream = file.OpenReadStream();
            imported = await JsonSerializer.DeserializeAsync<List<PromptTemplateEntity>>(stream)
                       ?? new List<PromptTemplateEntity>();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "ImportJson: failed to deserialise file.");
            TempData["Error"] = "Failed to parse JSON file.";
            return RedirectToAction(nameof(Index));
        }

        await using var db = await _dbFactory.CreateDbContextAsync();

        // Build a set of existing (Style, Version) combinations to support duplicate detection
        var existing = await db.PromptTemplates
            .AsNoTracking()
            .Select(t => new { t.Style, t.Version })
            .ToListAsync();

        var existingSet = existing
            .Select(e => $"{e.Style}|{e.Version}")
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        int inserted = 0;
        int skipped  = 0;

        foreach (var t in imported)
        {
            var key = $"{t.Style}|{t.Version}";
            if (existingSet.Contains(key))
            {
                skipped++;
                continue;
            }

            db.PromptTemplates.Add(new PromptTemplateEntity
            {
                Id           = Guid.NewGuid(),
                Style        = t.Style,
                Name         = t.Name,
                TemplateText = t.TemplateText,
                Version      = t.Version,
                IsActive     = t.IsActive,
                Source       = t.Source ?? "Database",
                CreatedAt    = DateTime.UtcNow
            });
            existingSet.Add(key);   // prevent duplicates within the same import file
            inserted++;
        }

        await db.SaveChangesAsync();
        _logger.LogInformation("ImportJson: {Inserted} inserted, {Skipped} skipped.", inserted, skipped);
        TempData["Success"] = $"Imported {inserted} templates ({skipped} skipped).";
        return RedirectToAction(nameof(Index));
    }

    // GET /PromptTemplate/History?style=rag
    /// <summary>Shows version timeline for a specific prompt style.</summary>
    public async Task<IActionResult> History(string style)
    {
        if (string.IsNullOrWhiteSpace(style)) return RedirectToAction(nameof(Index));

        await using var db = await _dbFactory.CreateDbContextAsync();
        var versions = await db.PromptTemplates
            .AsNoTracking()
            .Where(x => x.Style == style)
            .OrderByDescending(x => x.Version)
            .ToListAsync();

        ViewBag.Style = style;
        return View(versions);
    }

    // POST /PromptTemplate/Rollback/{id}
    /// <summary>
    /// Restores a previous version as the new active template.
    /// Deactivates all versions for this style, then creates a new version
    /// with the rolled-back template text.
    /// </summary>
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Rollback(Guid id)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        var source = await db.PromptTemplates.FindAsync(id);
        if (source == null) return NotFound();

        // Deactivate all versions for this style
        var allVersions = await db.PromptTemplates
            .Where(x => x.Style == source.Style)
            .ToListAsync();
        foreach (var v in allVersions) v.IsActive = false;

        // Create new version from the rolled-back source
        var maxVersion = allVersions.Max(x => x.Version);
        var restored = new PromptTemplateEntity
        {
            Id           = Guid.NewGuid(),
            Style        = source.Style,
            Name         = $"{source.Name} (rollback from v{source.Version})",
            TemplateText = source.TemplateText,
            Version      = maxVersion + 1,
            IsActive     = true,
            Source       = "Database",
            CreatedAt    = DateTime.UtcNow
        };
        db.PromptTemplates.Add(restored);
        await db.SaveChangesAsync();

        _logger.LogInformation(
            "PromptTemplate rollback: style={Style}, v{Source} -> v{New}.",
            source.Style, source.Version, restored.Version);
        TempData["Success"] = $"Rolled back to v{source.Version} as new v{restored.Version} for '{source.Style}'.";
        return RedirectToAction(nameof(History), new { style = source.Style });
    }

    // GET /PromptTemplate/Compare?id1=...&id2=...
    /// <summary>Side-by-side comparison of two template versions.</summary>
    public async Task<IActionResult> Compare(Guid id1, Guid id2)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        var left  = await db.PromptTemplates.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id1);
        var right = await db.PromptTemplates.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id2);

        if (left == null || right == null) return NotFound();

        ViewBag.Left  = left;
        ViewBag.Right = right;
        return View();
    }

    // GET /PromptTemplate/Stats
    /// <summary>
    /// Displays A/B statistics for each prompt style: total runs, average latency,
    /// SLA compliance percentage, average feedback rating, and last used timestamp.
    /// </summary>
    public async Task<IActionResult> Stats()
    {
        await using var db = await _dbFactory.CreateDbContextAsync();

        // Group RAG comparison histories by PromptStyle
        var historyGroups = await db.RagComparisonHistories
            .AsNoTracking()
            .GroupBy(h => h.PromptStyle)
            .Select(g => new
            {
                Style            = g.Key,
                TotalRuns        = g.Count(),
                AvgLatencyMs     = g.Average(h => (double)h.TotalLatencyMs),
                SlaCount         = g.Count(h => h.IsWithinSla),
                LastUsed         = (DateTime?)g.Max(h => h.CreatedAt)
            })
            .ToListAsync();

        // Fetch all feedbacks for rating join
        var feedbacks = await db.Feedbacks
            .AsNoTracking()
            .ToListAsync();

        // Fetch histories for joining feedbacks by HistoryId → PromptStyle
        var historyStyleMap = await db.RagComparisonHistories
            .AsNoTracking()
            .Select(h => new { h.Id, h.PromptStyle })
            .ToListAsync();

        var styleToRatings = feedbacks
            .Join(historyStyleMap,
                f => f.HistoryId,
                h => h.Id,
                (f, h) => new { h.PromptStyle, f.Rating })
            .GroupBy(x => x.PromptStyle)
            .ToDictionary(g => g.Key, g => g.Average(x => (double)x.Rating));

        var stats = historyGroups.Select(g => new PromptStyleStats
        {
            Style             = g.Style ?? "unknown",
            TotalRuns         = g.TotalRuns,
            AvgLatencyMs      = Math.Round(g.AvgLatencyMs, 2),
            SlaCompliancePct  = g.TotalRuns > 0
                                    ? Math.Round(g.SlaCount * 100.0 / g.TotalRuns, 1)
                                    : 0.0,
            AvgRating         = styleToRatings.TryGetValue(g.Style ?? "unknown", out var r) ? Math.Round(r, 2) : 0.0,
            LastUsed          = g.LastUsed
        })
        .OrderBy(s => s.Style)
        .ToList();

        return View(new PromptStatsViewModel { Stats = stats });
    }
}
