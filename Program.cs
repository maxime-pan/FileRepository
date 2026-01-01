
using System.IO;
using FileGallery.Data;
using FileGallery.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Negotiate;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ----- App data directory & absolute SQLite path -----
var dataDir = Path.Combine(builder.Environment.ContentRootPath, "App_Data");
Directory.CreateDirectory(dataDir); // ensure folder exists

var dbFilePath = Path.Combine(dataDir, "filegallery.db");
// Force absolute path into configuration before DbContext registration
builder.Configuration["ConnectionStrings:Sqlite"] = $"Data Source={dbFilePath}";

// ----- MVC & services -----
builder.Services.AddControllersWithViews();

// EF Core (SQLite)
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("Sqlite")));

// Repositories & settings
builder.Services.AddScoped<IFileRepository, EfCoreFileRepository>();
builder.Services.AddScoped<ISettingsService, SettingsService>();

// ----- Windows Authentication (Negotiate) + site-access toggle policy -----
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("SiteAccessPolicy", policy =>
    {
        policy.RequireAssertion(ctx =>
        {
            // If RequireWindowsAuth is false -> public site (no auth required)
            // If true -> require the request to be authenticated by Windows Auth
            var requireWinAuth = builder.Configuration.GetValue<bool>("Security:RequireWindowsAuth");
            if (!requireWinAuth) return true;
            return ctx.User?.Identity?.IsAuthenticated == true;
        });
    });
});

var app = builder.Build();

// ----- DB migrate & seed -----
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate(); // applies migrations / creates SQLite file if missing
    DataSeeder.Seed(db, app.Configuration);
}

// ----- pipeline -----
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

// Areas (Admin)
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Admin}/{action=Index}/{id?}");

// Public site guarded by policy (which can allow anonymous or require AD, per setting)
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
//    .RequireAuthorization("SiteAccessPolicy");

app.Run();

