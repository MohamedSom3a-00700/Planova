namespace Planova.Shared.Abstractions;

public enum AppTheme
{
    Dark,
    Light,
    HighContrast
}

public class ThemeChangedEventArgs : EventArgs
{
    public AppTheme NewTheme { get; init; }
}

public interface IThemeService
{
    AppTheme CurrentTheme { get; }
    void SetTheme(AppTheme theme);
    event EventHandler<ThemeChangedEventArgs> ThemeChanged;

    void SetTheme(string themeName);
    string GetCurrentTheme();
}