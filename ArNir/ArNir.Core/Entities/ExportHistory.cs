using System;
using System.ComponentModel.DataAnnotations;

namespace ArNir.Core.Entities
{
    public class ExportHistory
    {
        [Key]
        public int Id { get; set; }
        public string? UserName { get; set; }
        public string? Provider { get; set; }
        public string? Format { get; set; }
        public string? DateRange { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
