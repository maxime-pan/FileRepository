
using FileGallery.Data;
using FileGallery.Models;
using FileGallery.Models;
using FileGallery.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace FileGallery.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
 
        private readonly IFileRepository _repo;
        private readonly IWebHostEnvironment _env;
        private readonly ISettingsService _settings;
        public AdminController(IFileRepository repo, IWebHostEnvironment env, ISettingsService settings) { _repo = repo; _env = env; _settings = settings; }

        public async Task<IActionResult> Index()
        {
            ViewBag.Categories = await _repo.GetCategoriesAsync();
            return View();
        }

        [HttpPost]
        public async Task< IActionResult> CreateCategory(string name, string? slug)
        {
            if (string.IsNullOrWhiteSpace(name)) { TempData["Error"] = "Name required"; return RedirectToAction("Index"); }
            slug = string.IsNullOrWhiteSpace(slug) ? name.Trim().ToLower().Replace(' ', '-') : slug.Trim().ToLower();
            var categories = await _repo.GetCategoriesAsync();
           
            if (categories.Any(c => c.Slug == slug)) { TempData["Error"] = "Slug exists"; return RedirectToAction("Index"); }
            var finalSlug = slug ?? "";
            await _repo.AddOrUpdateCategoryAsync(new Category() { Name = name.Trim(), Slug = finalSlug, FolderPath = finalSlug });
            var webRoot = _env.WebRootPath ?? Path.Combine(_env.ContentRootPath, "wwwroot");
            var uploadDir = Path.Combine(webRoot, "uploads", finalSlug);
            Directory.CreateDirectory(uploadDir);
            TempData["Message"] = "Category created";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var cat = await _repo.GetCategoryAsync(id);
            if (cat == null) { TempData["Error"] = "Not found"; return RedirectToAction("Index"); }
            var files= await _repo.GetFilesByCategoryAsync(id, onlyVisible: false);
            if (files.Count > 0)
            {
                var webRoot = _env.WebRootPath ?? Path.Combine(_env.ContentRootPath, "wwwroot");
                foreach (var item in files)
                {
                    string baseDir = item.IsPublicUpload ? "publicupload" : "uploads";
                    var path = Path.Combine(webRoot, baseDir, cat.Slug ?? string.Empty, item.StoredFileName);
                    if (System.IO.File.Exists(path)) System.IO.File.Delete(path);
                    await _repo.DeleteFileAsync(item.Id);
                }
            }
            await _repo.DeleteCategoryAsync(id);
            TempData["Message"] = "Category deleted";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Upload(int categoryId, IFormFile file)
        {
            var category = await _repo.GetCategoryAsync(categoryId);
            if (category == null || file == null || file.Length == 0)
            {
                TempData["Error"] = "Select category and file";
                return RedirectToAction("Index");
            }

            var safeFileName = System.IO.Path.GetFileName(file.FileName);
            var ext = System.IO.Path.GetExtension(safeFileName).ToLowerInvariant();
            var storedName = $"{Guid.NewGuid():N}{ext}";
            var webRoot = _env.WebRootPath ?? Path.Combine(_env.ContentRootPath, "wwwroot");
            var categoryDir = Path.Combine(webRoot, "uploads", category.Slug ?? string.Empty);
            Directory.CreateDirectory(categoryDir);
            var fullPath = Path.Combine(categoryDir, storedName);

            try
            {
                using (var stream = new FileStream(fullPath, FileMode.Create)) { await file.CopyToAsync(stream); }

                var saveFile = new FileItem { FileName = safeFileName, StoredFileName = storedName, PhysicalPath= fullPath, ContentType = file.ContentType, Extension = ext, SizeBytes = file.Length, CategoryId = category.Id, IsPublicUpload = false };
                await _repo.AddOrUpdateFileAsync(saveFile);
                TempData["Message"] = "File uploaded";
            }
            catch (Exception ex)
            {
             
                TempData["Error"] = "Upload failed: " + ex.Message;
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteFile(int id)
        {

            var file = await _repo.GetFileAsync(id);
           
            if (file == null) { TempData["Error"] = "Not found"; return RedirectToAction("Index"); }
            file.Category = await _repo.GetCategoryAsync(file.CategoryId);
            string baseDir = file.IsPublicUpload ? "publicupload" : "uploads";
            var webRoot = _env.WebRootPath ?? Path.Combine(_env.ContentRootPath, "wwwroot");
            var path = Path.Combine(webRoot, baseDir, file.Category?.Slug ?? string.Empty, file.StoredFileName);
            if (System.IO.File.Exists(path)) System.IO.File.Delete(path);
            await _repo.DeleteFileAsync(id);
          
            TempData["Message"] = "File deleted";
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> PublicUpload()
        {
            var files = await _repo.GetPublicUploadFilesAsync();
            ViewBag.Categories = await _repo.GetCategoriesAsync();
            return View(files);
        }

        [HttpPost]
        public async Task<IActionResult> MovePublicToCategory(int fileId, int categoryId)
        {
            var file = await _repo.GetFileAsync(fileId);
            if(file==null ||!file.IsPublicUpload) { TempData["Error"] = "Invalid"; return RedirectToAction("PublicUpload"); }


            var cat = await _repo.GetCategoryAsync(categoryId);
            if (cat == null) { TempData["Error"] = "Invalid"; return RedirectToAction("PublicUpload"); }
            // move physical file
            var webRoot = _env.WebRootPath ?? Path.Combine(_env.ContentRootPath, "wwwroot");
            var src = Path.Combine(webRoot, "publicupload", file.StoredFileName);
            var destDir = Path.Combine(webRoot, "uploads", cat.Slug ?? string.Empty);
            Directory.CreateDirectory(destDir);
            var dest = Path.Combine(destDir, file.StoredFileName);
            if (System.IO.File.Exists(src)) System.IO.File.Move(src, dest, true);
            file.IsPublicUpload = false;
            file.CategoryId = cat.Id;
            await _repo.AddOrUpdateFileAsync(file);
        
            TempData["Message"] = "Moved to category";
            return RedirectToAction("PublicUpload");
        }
    }
}
