namespace FileGallery.Services;

public interface ISettingsService
{
    string Get(string key, string defaultValue = "");
    int GetInt(string key, int defaultValue = 0);
    bool GetBool(string key, bool defaultValue = false);
    Task SetAsync(string key, string value);
}
