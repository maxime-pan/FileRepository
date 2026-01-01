using FileGallery.Services;
using Microsoft.AspNetCore.Mvc;

namespace FileGallery.Controllers;

public class HomeController : Controller
{
    private readonly IFileRepository _repo;
    public HomeController(IFileRepository repo) => _repo = repo;

    public async Task<IActionResult> Index(int? categoryId)
    {
        var categories = await _repo.GetCategoriesAsync();
        var selected = categoryId ?? categories.FirstOrDefault()?.Id;
        var files = selected.HasValue ? await _repo.GetFilesByCategoryAsync(selected.Value, onlyVisible: true) : new List<FileGallery.Models.FileItem>();
        ViewBag.Categories = categories;
        ViewBag.SelectedCategoryId = selected;
        return View(files);
    }
}
