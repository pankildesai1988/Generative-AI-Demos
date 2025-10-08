namespace ArNir.Core.Config
{
    public class FileUploadSettings
    {
        public string[] AllowedTypes { get; set; } = Array.Empty<string>();
        public long MaxFileSize { get; set; }
    }
}
