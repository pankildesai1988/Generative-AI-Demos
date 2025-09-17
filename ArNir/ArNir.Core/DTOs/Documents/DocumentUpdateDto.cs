using Microsoft.AspNetCore.Http;

namespace ArNir.Core.DTOs.Documents
{
    public class DocumentUpdateDto
    {
        public string? Name { get; set; }
        public string? UploadedBy { get; set; }
        public string? Type { get; set; }

        public IFormFile? NewFile { get; set; }
    }
}
