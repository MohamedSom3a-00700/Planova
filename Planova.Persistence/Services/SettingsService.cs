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

        return key switch
        {
            nameof(UserPreferences.ThemePreference) => (T)(object)_preferences.ThemePreference,
            nameof(UserPreferences.LanguagePreference) => (T)(object)_preferences.LanguagePreference,
            nameof(UserPreferences.WindowX) => _preferences.WindowX is int wx ? (T)(object)wx : default,
            nameof(UserPreferences.WindowY) => _preferences.WindowY is int wy ? (T)(object)wy : default,
            nameof(UserPreferences.WindowWidth) => _preferences.WindowWidth is int ww ? (T)(object)ww : default,
            nameof(UserPreferences.WindowHeight) => _preferences.WindowHeight is int wh ? (T)(object)wh : default,
            nameof(UserPreferences.WindowMaximized) => (T)(object)_preferences.WindowMaximized,
            _ => default
        };
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
        }
    }
}
