
using FileGallery.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;

namespace FileGallery.Data
{
    public static class DataSeeder
    {
        public static void Seed(AppDbContext db, IConfiguration cfg)
        {
            // 0) Defensive: make sure we can reach the database file
            //    (especially important for SQLite + IIS deployments)
            try
            {
                // If we cannot connect, it's likely the file/folder isn't ready yet.
                // CanConnect() will create the file for SQLite on first connect if needed.
                if (!db.Database.CanConnect())
                {
                    // If the database is empty (no tables), create schema from the model.
                    // This does not use migrations.
                    db.Database.EnsureCreated();  // safe when no tables exist
                }
            }
            catch
            {
                // If the provider is relational, explicitly create tables via the relational creator.
                // This helps in cases where CanConnect succeeds but tables are still missing.
                var creator = db.Database.GetService<IRelationalDatabaseCreator>();
                creator.Create();        // creates the database (for SQLite: file)
                creator.CreateTables();  // creates tables from the model
            }

            // 1) Prefer migrations when they exist; otherwise stay with EnsureCreated
            try
            {
                var hasMigrations = db.Database.GetMigrations().Any();
                if (hasMigrations)
                {
                    // Apply migrations; this also creates tables if the DB is empty
                    db.Database.Migrate();
                }
                else
                {
                    // EnsureCreated initializes schema when NO tables exist
                    db.Database.EnsureCreated();
                }
            }
            catch (Exception ex)
            {
                // Provide a clear message if schema init fails
                throw new InvalidOperationException(
                    $"Failed to initialize database schema before seeding. {ex.Message}", ex);
            }

            // 2) Now it is safe to query and seed
            if (!db.Categories.Any())
            {
                var cats = new[]
                {
                    new Category { Name = "HR",        FolderPath = "HR",        DisplayOrder = 1, Slug = "hr" },
                    new Category { Name = "IT",        FolderPath = "IT",        DisplayOrder = 2, Slug = "it" },
                    new Category { Name = "Office",    FolderPath = "Office",    DisplayOrder = 3, Slug = "office" },
                    new Category { Name = "Documents", FolderPath = "Documents", DisplayOrder = 4, Slug = "documents" },
                    new Category { Name = "Reports",   FolderPath = "Reports",   DisplayOrder = 5, Slug = "reports" },
                    new Category { Name = "Root",      FolderPath = "",          DisplayOrder = 6, Slug = "root" }
                };
                db.Categories.AddRange(cats);
                db.SaveChanges();
            }

            if (!db.Settings.Any())
            {
                db.Settings.AddRange(new[]
                {
                    new SiteSetting { Key = "RequireWindowsAuth",
                        Value = (cfg.GetValue<bool>("Security:RequireWindowsAuth")).ToString() },
                    new SiteSetting { Key = "AllowedExtensions",
                        Value = cfg["Security:AllowedExtensions"] ??
                                ".pdf,.png,.jpg,.jpeg,.gif,.txt,.csv,.mp4,.mp3" },
                    new SiteSetting { Key = "MaxUploadSizeMB",
                        Value = (cfg.GetValue<int>("Security:MaxUploadSizeMB")).ToString() },
                    new SiteSetting { Key = "AdminADGroup",
                        Value = cfg["Security:AdminADGroup"] ?? string.Empty },
                    new SiteSetting { Key = "StorageRoot",
                        Value = cfg["Storage:Root"] ?? "Content" },
                    new SiteSetting { Key = "AnonymousUploadFolder",
                        Value = cfg["Storage:AnonymousUploadFolder"] ?? "publicupload" }
                });
                db.SaveChanges();
            }
        }
    }
}
