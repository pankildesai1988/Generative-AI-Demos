using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ArNir.Core.Entities;

/// <summary>Versioned prompt template stored in the database (Phase 10 — Layer 1 of 3-layer prompt resolution).</summary>
[Table("PromptTemplates")]
public class PromptTemplateEntity
{
    /// <summary>Primary key.</summary>
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>Prompt style key (e.g. "zero-shot", "few-shot", "role", "rag", "hybrid").</summary>
    [Required, MaxLength(50)]
    public string Style { get; set; } = string.Empty;

    /// <summary>Human-readable template name.</summary>
    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    /// <summary>Template body. Use {0} for query, {1} for context.</summary>
    [Required]
    public string TemplateText { get; set; } = string.Empty;

    /// <summary>Version number — higher wins on the same style.</summary>
    public int Version { get; set; } = 1;

    /// <summary>Only active templates are used by the resolver.</summary>
    public bool IsActive { get; set; } = true;

    /// <summary>Indicates origin: "Database" | "Config" | "Code".</summary>
    [MaxLength(20)]
    public string Source { get; set; } = "Database";

    /// <summary>UTC timestamp when this template was created.</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
