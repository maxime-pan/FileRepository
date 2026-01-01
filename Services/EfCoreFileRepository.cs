using FileGallery.Data;
using FileGallery.Models;
using Microsoft.EntityFrameworkCore;


namespace FileGallery.Services;


public class EfCoreFileRepository : IFileRepository
{
    private readonly AppDbContext _db;
    public EfCoreFileRepository(AppDbContext db) => _db = db;


    public Task<List<Category>> GetCategoriesAsync() => _db.Categories.Include(c => c.Files).OrderBy(c => c.DisplayOrder).ToListAsync();


    public Task<Category?> GetCategoryAsync(int id) => _db.Categories.FirstOrDefaultAsync(c => c.Id == id);


    public async Task<Admin?> GetAdminAsync(string username, string pass)
    {
        return await _db.Admins.FirstOrDefaultAsync(c => c.Username == username && c.PasswordHash == pass);
    }


    }


    public Task<List<FileItem>> GetFilesByCategoryAsync(int categoryId, bool onlyVisible) =>
        _db.Files.Where(f => f.CategoryId == categoryId && (!onlyVisible || f.Visible))
                 .OrderBy(f => f.FileName)
                 .ToListAsync();


    public Task<FileItem?> GetFileAsync(int id) => _db.Files.FirstOrDefaultAsync(f => f.Id == id);
    public Task<List<FileItem>> GetPublicUploadFilesAsync() => _db.Files.Where(f =>f.IsPublicUpload==true).OrderByDescending(m => m.Uploaded).ToListAsync();


    public async Task AddOrUpdateCategoryAsync(Category cat)
    {
        if (cat.Id == 0) _db.Categories.Add(cat); else _db.Categories.Update(cat);
        await _db.SaveChangesAsync();
    }


    public async Task DeleteCategoryAsync(int id)
    {
        var c = await _db.Categories.FindAsync(id);
        if (c != null) _db.Categories.Remove(c);
        await _db.SaveChangesAsync();
    }


    public async Task AddOrUpdateFileAsync(FileItem file)
    {
        if (file.Id == 0) _db.Files.Add(file); else _db.Files.Update(file);
        await _db.SaveChangesAsync();
    }


    public async Task DeleteFileAsync(int id)
    {
        var f = await _db.Files.FindAsync(id);
        if (f != null) _db.Files.Remove(f);
        await _db.SaveChangesAsync();
