using Planova.Shared.Abstractions;

namespace Planova.Application.Services;

public class ThemeService : IThemeService
{
    private string _currentTheme = "Dark";

    public event EventHandler<string>? ThemeChanged;

    public string GetCurrentTheme() => _currentTheme;

    public void SetTheme(string themeName)
    {
        if (_currentTheme == themeName)
            return;

        _currentTheme = themeName;
        ThemeChanged?.Invoke(this, themeName);
    }
}
