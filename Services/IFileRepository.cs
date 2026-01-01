using FileGallery.Models;

namespace FileGallery.Services;

public interface IFileRepository
{
    Task<List<Category>> GetCategoriesAsync();
    Task<Category?> GetCategoryAsync(int id);
    Task<List<FileItem>> GetFilesByCategoryAsync(int categoryId, bool onlyVisible);
    Task<FileItem?> GetFileAsync(int id);
    Task AddOrUpdateCategoryAsync(Category cat);
    Task DeleteCategoryAsync(int id);
    Task AddOrUpdateFileAsync(FileItem file);
    Task DeleteFileAsync(int id);
    Task<List<AnonymousUpload>> GetAnonymousUploadsAsync();
    Task AddAnonymousUploadAsync(AnonymousUpload au);
    Task<Admin?> GetAdminAsync(string username, string pass);
    Task<List<FileItem>> GetPublicUploadFilesAsync();
}
