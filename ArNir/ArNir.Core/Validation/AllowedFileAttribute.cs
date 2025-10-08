using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace ArNir.Core.Validation
{
    /// <summary>
    /// Lightweight validation attribute that checks file size and type.
    /// NOTE: AllowedTypes and MaxFileSize should be injected from appsettings.json 
    /// into DTOs via FluentValidation or Service Layer for dynamic control.
    /// </summary>
    public class AllowedFileAttribute : ValidationAttribute
    {
        private readonly string[] _allowedTypes;
        private readonly long _maxFileSize;

        public AllowedFileAttribute(string[]? allowedTypes = null, long maxFileSize = 5 * 1024 * 1024)
        {
            _allowedTypes = allowedTypes ?? new string[] { "application/pdf", "text/plain" };
            _maxFileSize = maxFileSize;
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            var file = value as IFormFile;

            if (file == null) return ValidationResult.Success;

            if (!_allowedTypes.Contains(file.ContentType))
                return new ValidationResult($"❌ File type '{file.ContentType}' is not allowed. Allowed: {string.Join(", ", _allowedTypes)}");

            if (file.Length > _maxFileSize)
                return new ValidationResult($"❌ File too large. Max allowed is {_maxFileSize / 1024 / 1024} MB");

            return ValidationResult.Success;
        }
    }
}
