using System.Collections.ObjectModel;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Planova.Shared.Abstractions;

namespace Planova.UI.ViewModels;

public partial class ShellViewModel : ObservableObject
{
    private readonly INavigationService _navigationService;
    private readonly ILoggingService _loggingService;
    private readonly IThemeService _themeService;
    private readonly ILocalizationService _localizationService;
    private readonly ISettingsService _settingsService;

    public ShellViewModel(
        INavigationService navigationService,
        ILoggingService loggingService,
        IThemeService themeService,
        ILocalizationService localizationService,
        ISettingsService settingsService)
    {
        _navigationService = navigationService;
        _loggingService = loggingService;
        _themeService = themeService;
        _localizationService = localizationService;
        _settingsService = settingsService;

        _themeService.ThemeChanged += OnThemeChanged;
        _localizationService.LanguageChanged += OnLanguageChanged;

        RegisterNavigationTargets();
    }

    public ObservableCollection<object> Tabs { get; } = new();

    [ObservableProperty]
    private string _statusText = "Ready";

    [RelayCommand]
    private void ToggleTheme()
    {
        var current = _themeService.GetCurrentTheme();
        _themeService.SetTheme(current == "Dark" ? "Light" : "Dark");
    }

    [RelayCommand]
    private void ToggleLanguage()
    {
        var current = _localizationService.GetCurrentLanguage();
        _localizationService.SetLanguage(current == "en" ? "ar" : "en");
    }

    private void OnThemeChanged(object? sender, string themeName)
    {
        if (System.Windows.Application.Current == null) return;

        _settingsService.Set("ThemePreference", themeName);
        _ = _settingsService.Save();

        var merged = System.Windows.Application.Current.Resources.MergedDictionaries;

        var lightTheme = merged.FirstOrDefault(d =>
            d.Source?.OriginalString?.Contains("LightTheme") == true);
        var darkTheme = merged.FirstOrDefault(d =>
            d.Source?.OriginalString?.Contains("DarkTheme") == true);

        if (lightTheme == null || darkTheme == null) return;

        if (themeName == "Light")
        {
            merged.Remove(darkTheme);
            if (!merged.Contains(lightTheme))
                merged.Insert(0, lightTheme);
        }
        else
        {
            merged.Remove(lightTheme);
            if (!merged.Contains(darkTheme))
                merged.Insert(0, darkTheme);
        }
    }

    private void OnLanguageChanged(object? sender, string languageCode)
    {
        _settingsService.Set("LanguagePreference", languageCode);
        _ = _settingsService.Save();

        if (System.Windows.Application.Current?.MainWindow == null) return;
        System.Windows.Application.Current.MainWindow.FlowDirection =
            languageCode == "ar" ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;
    }

    private void RegisterNavigationTargets()
    {
        _navigationService.RegisterTarget("boq", "BOQ", () => new System.Windows.Controls.ContentControl());
        _navigationService.RegisterTarget("wbs", "WBS", () => new System.Windows.Controls.ContentControl());
        _navigationService.RegisterTarget("scheduling", "Scheduling", () => new System.Windows.Controls.ContentControl());
        _navigationService.RegisterTarget("claims", "Claims", () => new System.Windows.Controls.ContentControl());
        _navigationService.RegisterTarget("settings", "Settings", () => new System.Windows.Controls.ContentControl());
    }
}
