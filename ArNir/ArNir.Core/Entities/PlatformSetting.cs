using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ArNir.Core.Entities;

/// <summary>Generic key-value configuration row for a named module (Phase 10 — DB-driven settings).</summary>
[Table("PlatformSettings")]
public class PlatformSetting
{
    /// <summary>Auto-incremented surrogate key.</summary>
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    /// <summary>Logical module this setting belongs to (e.g. "RAG", "AI", "Prompts", "Observability").</summary>
    [Required, MaxLength(50)]
    public string Module { get; set; } = string.Empty;

    /// <summary>Setting key (unique within a module, e.g. "ChunkSize").</summary>
    [Required, MaxLength(100)]
    public string Key { get; set; } = string.Empty;

    /// <summary>Setting value stored as a string; callers parse to the required type.</summary>
    [Required]
    public string Value { get; set; } = string.Empty;

    /// <summary>Optional human-readable description shown in the Admin UI.</summary>
    [MaxLength(300)]
    public string? Description { get; set; }

    /// <summary>UTC timestamp of the last write.</summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
