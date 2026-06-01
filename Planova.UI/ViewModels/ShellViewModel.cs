using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using Planova.Shared.Abstractions;
using Planova.UI.Views.Projects;
using Planova.UI.Views.Clients;
using Planova.UI.Views.Contracts;
using Planova.UI.Views.Dashboard;
using Planova.UI.Views.Profile;
using Planova.UI.Views.Reports;

namespace Planova.UI.ViewModels;

public partial class ShellViewModel : ObservableObject
{
    private readonly INavigationService _navigationService;
    private readonly IThemeService _themeService;
    private readonly ILocalizationService _localizationService;
    private readonly ISettingsService _settingsService;
    private readonly IServiceProvider _serviceProvider;
    private readonly Dictionary<string, WorkspaceTabViewModel> _openTabs = new();

    public ShellViewModel(
        INavigationService navigationService,
        ILoggingService loggingService,
        IThemeService themeService,
        ILocalizationService localizationService,
        ISettingsService settingsService,
        IServiceProvider serviceProvider)
    {
        _navigationService = navigationService;
        _themeService = themeService;
        _localizationService = localizationService;
        _settingsService = settingsService;
        _serviceProvider = serviceProvider;

        _themeService.ThemeChanged += OnThemeChanged;
        _localizationService.LanguageChanged += OnLanguageChanged;
        _navigationService.ActiveTargetChanged += OnActiveTargetChanged;

        RegisterNavigationTargets();
        BuildNavigationItems();
        NavigateToTarget("dashboard");
    }

    public ObservableCollection<NavigationItemViewModel> NavigationItems { get; } = new();

    public ObservableCollection<WorkspaceTabViewModel> Tabs { get; } = new();

    [ObservableProperty]
    private string _statusText = "Ready";

    [ObservableProperty]
    private WorkspaceTabViewModel? _selectedTab;

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

    [RelayCommand]
    private void NavigateTo(string targetId)
    {
        _navigationService.NavigateTo(targetId);
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
        _navigationService.RegisterTarget("dashboard", "Dashboard", () => _serviceProvider.GetRequiredService<DashboardView>());
        _navigationService.RegisterTarget("projects", "Projects", () => _serviceProvider.GetRequiredService<ProjectsWorkspaceView>());
        _navigationService.RegisterTarget("clients", "Clients", () => _serviceProvider.GetRequiredService<ClientsWorkspaceView>());
        _navigationService.RegisterTarget("contracts", "Contracts", () => _serviceProvider.GetRequiredService<ContractsWorkspaceView>());
        _navigationService.RegisterTarget("profile", "Profile", () => _serviceProvider.GetRequiredService<UserProfileView>());
        _navigationService.RegisterTarget("reports", "Reports", () => _serviceProvider.GetRequiredService<ReportView>());
        _navigationService.RegisterTarget("boq", "BOQ", () => new System.Windows.Controls.ContentControl());
        _navigationService.RegisterTarget("wbs", "WBS", () => new System.Windows.Controls.ContentControl());
        _navigationService.RegisterTarget("scheduling", "Scheduling", () => new System.Windows.Controls.ContentControl());
        _navigationService.RegisterTarget("claims", "Claims", () => new System.Windows.Controls.ContentControl());
        _navigationService.RegisterTarget("settings", "Settings", () => new System.Windows.Controls.ContentControl());
    }

    private void BuildNavigationItems()
    {
        NavigationItems.Clear();
        foreach (var target in _navigationService.GetTargets())
        {
            NavigationItems.Add(new NavigationItemViewModel(target.Id, target.DisplayName, NavigateToCommand));
        }
    }

    private void OnActiveTargetChanged(object? sender, string targetId)
    {
        OpenTarget(targetId);
    }

    private void NavigateToTarget(string targetId)
    {
        _navigationService.NavigateTo(targetId);
    }

    private void OpenTarget(string targetId)
    {
        if (_openTabs.TryGetValue(targetId, out var existing))
        {
            SelectedTab = existing;
            StatusText = existing.DisplayName;
            return;
        }

        if (!_navigationService.TryCreateView(targetId, out var view) || view == null)
            return;

        var target = _navigationService.GetTargets().FirstOrDefault(t => t.Id == targetId);
        if (target is null)
            return;

        var tab = new WorkspaceTabViewModel(target.Id, target.DisplayName, view);
        _openTabs[target.Id] = tab;
        Tabs.Add(tab);
        SelectedTab = tab;
        StatusText = target.DisplayName;
    }
}
