namespace Planova.Shared.Abstractions;

public interface IThemeService
{
    void SetTheme(string themeName);
    string GetCurrentTheme();
    event EventHandler<string> ThemeChanged;
}
