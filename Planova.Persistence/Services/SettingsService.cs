using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Planova.Domain.Entities;
using Planova.Persistence.DbContext;
using Planova.Shared.Abstractions;

namespace Planova.Persistence.Services;

public class SettingsService : ISettingsService
{
    private readonly PlanovaDbContext _context;
    private UserPreferences? _preferences;

    public SettingsService(PlanovaDbContext context)
    {
        _context = context;
    }

    public async Task Load()
    {
        _preferences = await _context.UserPreferences.FirstOrDefaultAsync();
        _preferences ??= new UserPreferences();
    }

    public async Task Save()
    {
        if (_preferences == null) return;

        _preferences.UpdatedAt = DateTime.UtcNow;

        if (_preferences.Id == 0)
        {
            _preferences.CreatedAt = DateTime.UtcNow;
            _context.UserPreferences.Add(_preferences);
        }

        await _context.SaveChangesAsync();
    }

    public T? Get<T>(string key)
    {
        if (_preferences == null) return default;

        var result = key switch
        {
            nameof(UserPreferences.ThemePreference) => _preferences.ThemePreference,
            nameof(UserPreferences.LanguagePreference) => _preferences.LanguagePreference,
            nameof(UserPreferences.WindowX) => _preferences.WindowX?.ToString(),
            nameof(UserPreferences.WindowY) => _preferences.WindowY?.ToString(),
            nameof(UserPreferences.WindowWidth) => _preferences.WindowWidth?.ToString(),
            nameof(UserPreferences.WindowHeight) => _preferences.WindowHeight?.ToString(),
            nameof(UserPreferences.WindowMaximized) => _preferences.WindowMaximized.ToString(),
            _ => GetAdditional(key)
        };

        if (result == null) return default;

        try
        {
            var targetType = typeof(T);
            if (Nullable.GetUnderlyingType(targetType) is { } underlying)
                targetType = underlying;
            return (T)Convert.ChangeType(result, targetType);
        }
        catch
        {
            return default;
        }
    }

    public void Set<T>(string key, T value)
    {
        if (_preferences == null) return;

        switch (key)
        {
            case nameof(UserPreferences.ThemePreference):
                _preferences.ThemePreference = value?.ToString() ?? "Dark";
                break;
            case nameof(UserPreferences.LanguagePreference):
                _preferences.LanguagePreference = value?.ToString() ?? "en";
                break;
            case nameof(UserPreferences.WindowX):
                _preferences.WindowX = value as int?;
                break;
            case nameof(UserPreferences.WindowY):
                _preferences.WindowY = value as int?;
                break;
            case nameof(UserPreferences.WindowWidth):
                _preferences.WindowWidth = value as int?;
                break;
            case nameof(UserPreferences.WindowHeight):
                _preferences.WindowHeight = value as int?;
                break;
            case nameof(UserPreferences.WindowMaximized):
                _preferences.WindowMaximized = value as bool? ?? false;
                break;
            default:
                SetAdditional(key, value?.ToString());
                break;
        }
    }

    private string? GetAdditional(string key)
    {
        if (_preferences?.AdditionalSettings == null) return null;
        try
        {
            var dict = JsonSerializer.Deserialize<Dictionary<string, string>>(_preferences.AdditionalSettings);
            return dict?.GetValueOrDefault(key);
        }
        catch
        {
            return null;
        }
    }

    private void SetAdditional(string key, string? value)
    {
        if (_preferences == null) return;

        Dictionary<string, string> dict;
        try
        {
            dict = _preferences.AdditionalSettings != null
                ? JsonSerializer.Deserialize<Dictionary<string, string>>(_preferences.AdditionalSettings) ?? []
                : [];
        }
        catch
        {
            dict = [];
        }

        if (value != null)
            dict[key] = value;
        else
            dict.Remove(key);

        _preferences.AdditionalSettings = JsonSerializer.Serialize(dict);
    }
}
