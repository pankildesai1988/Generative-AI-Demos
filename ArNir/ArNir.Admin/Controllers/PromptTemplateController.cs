using ArNir.Admin.Models;
using ArNir.Core.Entities;
using ArNir.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
    /// <summary>Persists edits to an existing template.</summary>
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, PromptTemplateEntity model)
    {
        if (id != model.Id) return BadRequest();
        if (!ModelState.IsValid) return View("CreateEdit", model);

        await using var db = await _dbFactory.CreateDbContextAsync();
        var entity = await db.PromptTemplates.FindAsync(id);
        if (entity == null) return NotFound();

        entity.Name         = model.Name;
        entity.Style        = model.Style;
        entity.TemplateText = model.TemplateText;
        entity.IsActive     = model.IsActive;

        await db.SaveChangesAsync();
        _logger.LogInformation("PromptTemplate updated: id={Id}.", id);
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
