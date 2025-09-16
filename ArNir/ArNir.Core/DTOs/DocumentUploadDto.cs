using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace ArNir.Core.DTOs
{
    public class DocumentUploadDto
    {
        [Required]
        public IFormFile File { get; set; }

        public string? UploadedBy { get; set; }
    }
}
