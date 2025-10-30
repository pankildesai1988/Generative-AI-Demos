using System;

namespace ArNir.Core.DTOs.Intelligence
{
    public class ExportHistoryDto
    {
        public int Id { get; set; }
        public string? UserName { get; set; }
        public string? Provider { get; set; }
        public string? Format { get; set; }
        public string? DateRange { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
