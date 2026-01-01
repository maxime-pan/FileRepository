using FileGallery.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.AspNetCore.Authorization;

namespace FileGallery.Controllers;

public class FilesController : Controller
{
    private readonly IFileRepository _repo;
    private readonly IWebHostEnvironment _env;
    private readonly ISettingsService _settings;
    public FilesController(IFileRepository repo, IWebHostEnvironment env, ISettingsService settings)
    {
        _repo = repo; _env = env; _settings = settings;
    }

    [HttpGet]
    public async Task<IActionResult> Preview(int id)
    {
        var file = await _repo.GetFileAsync(id);
        if (file == null || (!file.Visible)) return NotFound();
        return View(file);
    }

    [HttpGet]
    public async Task<IActionResult> Download(int id)
    {
        var file = await _repo.GetFileAsync(id);
        if (file == null) return NotFound();
        var provider = new FileExtensionContentTypeProvider();
        if (!provider.TryGetContentType(file.FileName, out var contentType)) contentType = "application/octet-stream";
        return PhysicalFile(file.PhysicalPath, contentType, file.FileName);
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Upload() => View();

    [HttpPost]
    [AllowAnonymous]
    [RequestSizeLimit(100_000_000)]
    public async Task<IActionResult> Upload(IFormFile file)
    {
        if (file == null || file.Length == 0) { TempData["UploadMessage"] = "No file selected."; return RedirectToAction("Upload"); }
        var maxMb = _settings.GetInt("MaxUploadSizeMB", 50);
        if (file.Length > maxMb * 1024L * 1024L)
        {
            TempData["UploadMessage"] = $"File too large. Max {maxMb} MB.";
            return RedirectToAction("Upload");
        }
        var allowed = _settings.Get("AllowedExtensions", ".pdf,.png,.jpg,.jpeg,.gif,.txt,.csv,.mp4,.mp3")
                               .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                               .Select(e => e.ToLowerInvariant()).ToHashSet();
        var ext = System.IO.Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!allowed.Contains(ext))
        {
            TempData["UploadMessage"] = $"Extension not allowed ({ext}).";
            return RedirectToAction("Upload");
        }
        var webRoot = _env.WebRootPath ?? Path.Combine(_env.ContentRootPath, "wwwroot");
        var anonFolderName = _settings.Get("AnonymousUploadFolder", "publicupload");
        var targetDir = Path.Combine(webRoot, anonFolderName);
        Directory.CreateDirectory(targetDir);
        var storedName = Guid.NewGuid().ToString("N") + ext;
        var fullPath = System.IO.Path.Combine(targetDir, storedName);
        using (var fs = System.IO.File.Create(fullPath))
        {
            await file.CopyToAsync(fs);
        }
        await _repo.AddOrUpdateFileAsync(new FileGallery.Models.FileItem
        {
            FileName = file.FileName,
            StoredFileName = storedName,
            SizeBytes = file.Length,
            ContentType = file.ContentType,
            Extension = ext,
            PhysicalPath = fullPath,
            IsPublicUpload = true,
            Visible = true,
            CategoryId = (await _repo.GetCategoriesAsync()).FirstOrDefault(c => c.Slug == "root" || c.Name == "Root")?.Id ?? 0
        });
        TempData["UploadMessage"] = "Upload successful.";
        return RedirectToAction("Upload");
    }
}
