using System.Windows;
using Planova.Shared.Abstractions;

namespace Planova.UI.Services;

public class ThemeService : IThemeService
{
    private AppTheme _currentTheme = AppTheme.Dark;
    private readonly IHighContrastDetector _highContrastDetector;
    private AppTheme _previousTheme = AppTheme.Dark;

    public AppTheme CurrentTheme => _currentTheme;

    public event EventHandler<ThemeChangedEventArgs>? ThemeChanged;

    public ThemeService(IHighContrastDetector highContrastDetector)
    {
        _highContrastDetector = highContrastDetector;
        _highContrastDetector.HighContrastChanged += OnHighContrastChanged;
    }

    public void SetTheme(AppTheme theme)
    {
        if (_currentTheme == theme)
            return;

        _currentTheme = theme;
        ApplyTheme(theme);
        ThemeChanged?.Invoke(this, new ThemeChangedEventArgs { NewTheme = theme });
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

    public string GetCurrentTheme() => _currentTheme.ToString();

    private void OnHighContrastChanged(object? sender, bool isHighContrast)
    {
        if (isHighContrast)
        {
            _previousTheme = _currentTheme;
            SetTheme(AppTheme.HighContrast);
        }
        else
        {
            SetTheme(_previousTheme);
        }
    }

    private void ApplyTheme(AppTheme theme)
    {
        var app = System.Windows.Application.Current;
        if (app == null) return;

        var merged = app.Resources.MergedDictionaries;

        var darkTheme = merged.FirstOrDefault(d =>
            d.Source?.OriginalString?.Contains("DarkTheme") == true);
        var lightTheme = merged.FirstOrDefault(d =>
            d.Source?.OriginalString?.Contains("LightTheme") == true);
        var highContrastFallback = merged.FirstOrDefault(d =>
            d.Source?.OriginalString?.Contains("HighContrastFallback") == true);
        var brandColors = merged.FirstOrDefault(d =>
            d.Source?.OriginalString?.Contains("BrandColors") == true);
        var gradients = merged.FirstOrDefault(d =>
            d.Source?.OriginalString?.Contains("Gradients") == true);

        switch (theme)
        {
            case AppTheme.Dark:
                if (highContrastFallback != null) merged.Remove(highContrastFallback);
                if (lightTheme != null) merged.Remove(lightTheme);
                if (brandColors == null)
                    merged.Add(new ResourceDictionary
                        { Source = new Uri("Styles/BrandColors.xaml", UriKind.Relative) });
                if (gradients == null)
                    merged.Add(new ResourceDictionary
                        { Source = new Uri("Styles/Gradients.xaml", UriKind.Relative) });
                if (darkTheme == null)
                    merged.Insert(merged.Count, new ResourceDictionary
                        { Source = new Uri("Styles/DarkTheme.xaml", UriKind.Relative) });
                break;

            case AppTheme.Light:
                if (highContrastFallback != null) merged.Remove(highContrastFallback);
                if (darkTheme != null) merged.Remove(darkTheme);
                if (brandColors == null)
                    merged.Add(new ResourceDictionary
                        { Source = new Uri("Styles/BrandColors.xaml", UriKind.Relative) });
                if (gradients == null)
                    merged.Add(new ResourceDictionary
                        { Source = new Uri("Styles/Gradients.xaml", UriKind.Relative) });
                if (lightTheme == null)
                    merged.Insert(merged.Count, new ResourceDictionary
                        { Source = new Uri("Styles/LightTheme.xaml", UriKind.Relative) });
                break;

            case AppTheme.HighContrast:
                if (brandColors != null) merged.Remove(brandColors);
                if (gradients != null) merged.Remove(gradients);
                if (darkTheme != null) merged.Remove(darkTheme);
                if (lightTheme != null) merged.Remove(lightTheme);
                if (highContrastFallback == null)
                    merged.Insert(merged.Count, new ResourceDictionary
                        { Source = new Uri("Styles/HighContrastFallback.xaml", UriKind.Relative) });
                break;
        }

        System.Windows.Application.Current.Dispatcher.Invoke(() =>
        {
            foreach (System.Windows.Window window in System.Windows.Application.Current.Windows)
            {
                window.InvalidateVisual();
            }
        });
    }
}