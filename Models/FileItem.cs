namespace FileGallery.Models;

public class FileItem
{
    public int Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string StoredFileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;

    public string Extension { get; set; } = string.Empty;
    public long SizeBytes { get; set; }
    public DateTime Uploaded { get; set; } = DateTime.UtcNow;
    public bool Visible { get; set; } = true;
    public string PhysicalPath { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public Category? Category { get; set; }
    public bool IsPublicUpload { get; set; } = true;

}
