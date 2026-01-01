namespace FileGallery.Models;

public class AnonymousUpload
{
    public int Id { get; set; }
    public string OriginalFileName { get; set; } = string.Empty;
    public string StoredFileName { get; set; } = string.Empty;
    public long SizeBytes { get; set; }
    public DateTime UploadedUtc { get; set; } = DateTime.UtcNow;
    public string? ClientIp { get; set; }
}
