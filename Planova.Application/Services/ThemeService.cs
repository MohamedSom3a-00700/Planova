using Planova.Shared.Abstractions;

namespace Planova.Application.Services;

public class ThemeService : IThemeService
{
    private AppTheme _currentTheme = AppTheme.Dark;
    private string _currentThemeName = "Dark";

    public AppTheme CurrentTheme => _currentTheme;

    public event EventHandler<ThemeChangedEventArgs>? ThemeChanged;
    public event EventHandler<string>? ThemeChangedLegacy;

    public void SetTheme(AppTheme theme)
    {
        if (_currentTheme == theme)
            return;

        _currentTheme = theme;
        _currentThemeName = theme.ToString();
        ThemeChanged?.Invoke(this, new ThemeChangedEventArgs { NewTheme = theme });
        ThemeChangedLegacy?.Invoke(this, _currentThemeName);
    }

    public void SetTheme(string themeName)
    {
        var theme = themeName switch
        {
            "Light" => AppTheme.Light,
            "HighContrast" => AppTheme.HighContrast,
            _ => AppTheme.Dark
        };
        SetTheme(theme);
    }

    public string GetCurrentTheme() => _currentThemeName;
}
