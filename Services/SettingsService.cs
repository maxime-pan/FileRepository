using FileGallery.Data;
using FileGallery.Models;

namespace FileGallery.Services;

public class SettingsService : ISettingsService
{
    private readonly AppDbContext _db;
    public SettingsService(AppDbContext db) => _db = db;

    public string Get(string key, string defaultValue = "") =>
        _db.Settings.FirstOrDefault(s => s.Key == key)?.Value ?? defaultValue;

    public int GetInt(string key, int defaultValue = 0) => int.TryParse(Get(key), out var v) ? v : defaultValue;

    public bool GetBool(string key, bool defaultValue = false) => bool.TryParse(Get(key), out var v) ? v : defaultValue;

    public async Task SetAsync(string key, string value)
    {
        var s = _db.Settings.FirstOrDefault(s => s.Key == key);
        if (s == null) _db.Settings.Add(new SiteSetting{ Key = key, Value = value });
        else { s.Value = value; _db.Settings.Update(s);}        
        await _db.SaveChangesAsync();
    }
}
