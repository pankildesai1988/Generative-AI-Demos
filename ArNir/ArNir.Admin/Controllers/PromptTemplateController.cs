using ArNir.Core.Entities;
using ArNir.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ArNir.Admin.Controllers;

/// <summary>
/// Admin CRUD panel for <see cref="PromptTemplateEntity"/> records.
/// Operators can create, edit, and soft-delete versioned prompt templates
/// so the 3-layer resolver (DB → Config → Code) serves the correct template at runtime.
/// </summary>
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
}
