namespace FileGallery.Models;

public class Category
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string FolderPath { get; set; } = string.Empty;
    public int DisplayOrder { get; set; } = 0;
    public bool IsActive { get; set; } = true;
    public List<FileItem> Files { get; set; } = new();
    public string? Slug { get; set; } = string.Empty;
}
