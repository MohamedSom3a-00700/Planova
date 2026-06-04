using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Planova.Shared.Abstractions;

namespace Planova.UI.ViewModels;

public sealed partial class SettingsViewModel : ObservableObject
{
    private readonly ISettingsService _settingsService;
    private readonly IThemeService _themeService;
    private readonly ILocalizationService _localizationService;
    private readonly string _dbPath;

    public SettingsViewModel(ISettingsService settingsService, IThemeService themeService, ILocalizationService localizationService)
    {
        _settingsService = settingsService;
        _themeService = themeService;
        _localizationService = localizationService;
        _dbPath = GetDatabasePath();
        ThemeOptions = ["Dark", "Light"];
        LanguageOptions = ["en", "ar"];
        LoadSettings();
    }

    public List<string> ThemeOptions { get; }
    public List<string> LanguageOptions { get; }

    [ObservableProperty]
    private string _displayName = string.Empty;

    [ObservableProperty]
    private string _organization = string.Empty;

    [ObservableProperty]
    private string _roleLabel = string.Empty;

    [ObservableProperty]
    private string _dbLocation = string.Empty;

    [ObservableProperty]
    private string _dbSize = string.Empty;

    [ObservableProperty]
    private string _connectionString = string.Empty;

    [ObservableProperty]
    private string _selectedTheme = "Dark";

    [ObservableProperty]
    private string _selectedLanguage = "en";

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    partial void OnSelectedThemeChanged(string value)
    {
        _themeService.SetTheme(value);
        _settingsService.Set("ThemePreference", value);
    }

    partial void OnSelectedLanguageChanged(string value)
    {
        _localizationService.SetLanguage(value);
        _settingsService.Set("LanguagePreference", value);
    }

    private void LoadSettings()
    {
        DisplayName = _settingsService.Get<string>("DisplayName") ?? string.Empty;
        Organization = _settingsService.Get<string>("OrganizationName") ?? string.Empty;
        RoleLabel = _settingsService.Get<string>("RoleLabel") ?? string.Empty;
        SelectedTheme = _settingsService.Get<string>("ThemePreference") ?? "Dark";
        SelectedLanguage = _settingsService.Get<string>("LanguagePreference") ?? "en";

        DbLocation = _dbPath;
        ConnectionString = $"Data Source={_dbPath}";
        DbSize = GetDatabaseSize();
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        _settingsService.Set("DisplayName", DisplayName);
        _settingsService.Set("OrganizationName", Organization);
        _settingsService.Set("RoleLabel", RoleLabel);
        _settingsService.Set("ThemePreference", SelectedTheme);
        _settingsService.Set("LanguagePreference", SelectedLanguage);

        await _settingsService.Save();
        StatusMessage = "Settings saved successfully.";
    }

    [RelayCommand]
    private void ResetDefaults()
    {
        DisplayName = string.Empty;
        Organization = string.Empty;
        RoleLabel = string.Empty;
        SelectedTheme = "Dark";
        SelectedLanguage = "en";
        StatusMessage = "Defaults restored. Click Save to apply.";
    }

    private static string GetDatabasePath()
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        return Path.Combine(appData, "Planova", "planova.db");
    }

    private string GetDatabaseSize()
    {
        try
        {
            if (File.Exists(_dbPath))
            {
                var info = new FileInfo(_dbPath);
                var sizeBytes = info.Length;
                return sizeBytes switch
                {
                    < 1024 => $"{sizeBytes} B",
                    < 1024 * 1024 => $"{sizeBytes / 1024.0:F1} KB",
                    < 1024 * 1024 * 1024 => $"{sizeBytes / (1024.0 * 1024):F1} MB",
                    _ => $"{sizeBytes / (1024.0 * 1024 * 1024):F2} GB"
                };
            }
            return "Database not found";
        }
        catch
        {
            return "Unable to read database size";
        }
    }
}
