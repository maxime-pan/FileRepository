using FileGallery.Models;
using Microsoft.EntityFrameworkCore;


namespace FileGallery.Data;


public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }


    public DbSet<Admin> Admins => Set<Admin>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<FileItem> Files => Set<FileItem>();
    public DbSet<SiteSetting> Settings => Set<SiteSetting>();
    public DbSet<AnonymousUpload> AnonymousUploads => Set<AnonymousUpload>();


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Admin>().HasData(new Admin
        {
            AdminId = 1,
            Username = "Admin",
            PasswordHash = "e86f78a8a3caf0b60d8e74e5942aa6d86dc150cd3c03338aef25b7d2d7e3acc7"
        });
        modelBuilder.Entity<Category>().HasMany(c => c.Files).WithOne(f => f.Category!).HasForeignKey(f => f.CategoryId);
        modelBuilder.Entity<Category>().HasIndex(c => c.DisplayOrder);
        modelBuilder.Entity<FileItem>().HasIndex(f => new { f.CategoryId, f.Visible });
        modelBuilder.Entity<SiteSetting>().HasIndex(s => s.Key).IsUnique();
    }
}
