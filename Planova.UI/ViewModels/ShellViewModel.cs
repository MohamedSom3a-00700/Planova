using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using Planova.Application.Dto;
using Planova.Application.Services;
using Planova.Shared.Abstractions;
using Planova.UI.Views.Empty;
using Planova.UI.Views.Projects;
using Planova.UI.Views.Clients;
using Planova.UI.Views.Contracts;
using Planova.UI.Views.Dashboard;
using Planova.UI.Views.Boq;
using Planova.UI.Views.Excel;
using Planova.UI.Views.Profile;
using Planova.UI.Views.Reports;
using Planova.UI.Views.Wbs;
using Planova.UI.Views.Reporting;
using Planova.UI.Views.Activity;
using Planova.UI.Views.Resource;
using Planova.UI.Views.Cost;
using Planova.UI.ViewModels.Reporting;
using Planova.UI.Views;
using Planova.UI.Views.Primavera;
using Planova.UI.Views.ScheduleComparison;
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;

namespace Planova.UI.ViewModels;

public partial class ShellViewModel : ObservableObject
{
    private readonly INavigationService _navigationService;
    private readonly IThemeService _themeService;
    private readonly ILocalizationService _localizationService;
    private readonly ISettingsService _settingsService;
    private readonly IServiceProvider _serviceProvider;
    private readonly IHighContrastDetector? _highContrastDetector;
    private readonly ICurrentProjectService _currentProjectService;
    private readonly IProjectService _projectService;
    private readonly Dictionary<string, WorkspaceTabViewModel> _openTabs = new();
    private readonly List<string> _studioTargetIds = new()
    { "boq", "wbs", "activity", "resource", "cost", "excel-studio", "primavera",
      "schedule-compare", "delay-analysis", "chronology", "knowledge-base", "integration-hub",
      "reports" };

    public ShellViewModel(
        INavigationService navigationService,
        ILoggingService loggingService,
        IThemeService themeService,
        ILocalizationService localizationService,
        ISettingsService settingsService,
        IServiceProvider serviceProvider,
        ICurrentProjectService currentProjectService,
        IProjectService projectService,
        IHighContrastDetector? highContrastDetector = null)
    {
        _navigationService = navigationService;
        _themeService = themeService;
        _localizationService = localizationService;
        _settingsService = settingsService;
        _serviceProvider = serviceProvider;
        _currentProjectService = currentProjectService;
        _projectService = projectService;
        _highContrastDetector = highContrastDetector;

        _themeService.ThemeChanged += OnThemeChanged;
        _localizationService.LanguageChanged += OnLanguageChanged;
        _navigationService.ActiveTargetChanged += OnActiveTargetChanged;
        _currentProjectService.CurrentProjectChanged += OnCurrentProjectChanged;

        if (_highContrastDetector != null)
        {
            _highContrastDetector.HighContrastChanged += OnHighContrastChanged;
        }

        UpdateBrandingAssets(_themeService.CurrentTheme);

        RegisterNavigationTargets();
        BuildNavigationItems();
        UpdateStudioNavigationStates();
        NavigateToTarget("dashboard");

        _ = LoadProjectsAsync();
    }

    public ObservableCollection<NavigationItemViewModel> NavigationItems { get; } = new();

    public ObservableCollection<WorkspaceTabViewModel> Tabs { get; } = new();

    public AssistantPanelViewModel AssistantPanel { get; } = new();

    [ObservableProperty]
    private string _statusText = "Ready";

    [ObservableProperty]
    private WorkspaceTabViewModel? _selectedTab;

    [ObservableProperty]
    private BitmapImage? _logoSource;

    [ObservableProperty]
    private BitmapImage? _wordmarkSource;

    [ObservableProperty]
    private ProjectContext? _currentProject;

    [ObservableProperty]
    private bool _hasActiveProject;

    public ObservableCollection<ProjectSummaryDto> ProjectSelectorItems { get; } = new();

    partial void OnCurrentProjectChanged(ProjectContext? value)
    {
        HasActiveProject = value != null;
        StatusText = value != null ? $"Project: {value.Name}" : "Ready";
        UpdateStudioNavigationStates();
    }

    private void OnCurrentProjectChanged(object? sender, ProjectContext? project)
    {
        CurrentProject = project;
    }

    [RelayCommand]
    private async Task SelectProjectAsync(ProjectSummaryDto? project)
    {
        if (project == null) return;
        var detail = await _projectService.GetByIdAsync(project.Id);
        if (detail != null)
        {
            _currentProjectService.SetProject(new ProjectContext(
                detail.Id, detail.Code, detail.Name));
        }
    }

    [RelayCommand]
    private void ClearProject()
    {
        _currentProjectService.SetProject(null);
    }

    [RelayCommand]
    private void ToggleTheme()
    {
        var next = _themeService.CurrentTheme switch
        {
            AppTheme.Dark => AppTheme.Light,
            AppTheme.Light => AppTheme.Dark,
            _ => AppTheme.Dark
        };
        _themeService.SetTheme(next);
    }

    [RelayCommand]
    private void ToggleLanguage()
    {
        var current = _localizationService.GetCurrentLanguage();
        _localizationService.SetLanguage(current == "en" ? "ar" : "en");
    }

    private void OnHighContrastChanged(object? sender, bool isHighContrast)
    {
        _themeService.SetTheme(isHighContrast ? AppTheme.HighContrast : AppTheme.Dark);
    }

    private void OnThemeChanged(object? sender, ThemeChangedEventArgs e)
    {
        if (System.Windows.Application.Current == null) return;

        _settingsService.Set("ThemePreference", e.NewTheme.ToString());
        _ = _settingsService.Save();

        UpdateBrandingAssets(e.NewTheme);

        var merged = System.Windows.Application.Current.Resources.MergedDictionaries;

        var lightTheme = merged.FirstOrDefault(d =>
            d.Source?.OriginalString?.Contains("LightTheme") == true);
        var darkTheme = merged.FirstOrDefault(d =>
            d.Source?.OriginalString?.Contains("DarkTheme") == true);
        var highContrastFallback = merged.FirstOrDefault(d =>
            d.Source?.OriginalString?.Contains("HighContrastFallback") == true);

        switch (e.NewTheme)
        {
            case AppTheme.Light:
                if (darkTheme != null) merged.Remove(darkTheme);
                if (highContrastFallback != null) merged.Remove(highContrastFallback);
                if (!merged.Any(d => d.Source?.OriginalString?.Contains("LightTheme") == true))
                    merged.Insert(merged.Count, new System.Windows.ResourceDictionary
                        { Source = new Uri("Styles/LightTheme.xaml", UriKind.Relative) });
                break;
            case AppTheme.Dark:
                if (lightTheme != null) merged.Remove(lightTheme);
                if (highContrastFallback != null) merged.Remove(highContrastFallback);
                if (!merged.Any(d => d.Source?.OriginalString?.Contains("DarkTheme") == true))
                    merged.Insert(merged.Count, new System.Windows.ResourceDictionary
                        { Source = new Uri("Styles/DarkTheme.xaml", UriKind.Relative) });
                break;
            case AppTheme.HighContrast:
                if (lightTheme != null) merged.Remove(lightTheme);
                if (darkTheme != null) merged.Remove(darkTheme);
                if (!merged.Any(d => d.Source?.OriginalString?.Contains("HighContrastFallback") == true))
                    merged.Insert(merged.Count, new System.Windows.ResourceDictionary
                        { Source = new Uri("Styles/HighContrastFallback.xaml", UriKind.Relative) });
                break;
        }

        ApplicationTheme wpfUiTheme = e.NewTheme switch
        {
            AppTheme.Light => ApplicationTheme.Light,
            AppTheme.HighContrast => ApplicationTheme.HighContrast,
            _ => ApplicationTheme.Dark
        };

        var backdrop = WindowBackdrop.IsSupported(WindowBackdropType.Mica)
            ? WindowBackdropType.Mica
            : WindowBackdropType.None;

        ApplicationThemeManager.Apply(wpfUiTheme, backdrop);
    }

    private void UpdateBrandingAssets(AppTheme theme)
    {
        var isHighContrast = SystemParameters.HighContrast || theme == AppTheme.HighContrast;
        var isDark = isHighContrast || theme == AppTheme.Dark;

        var logoFile = isHighContrast ? "LogoMonochrome.png" : isDark ? "LogoDark.png" : "LogoLight.png";
        var wordmarkFile = isHighContrast ? "LogoMonochrome.png" : isDark ? "WordmarkDark.png" : "WordmarkLight.png";
        LogoSource = LoadImage(logoFile);
        WordmarkSource = LoadImage(wordmarkFile);
    }

    private static BitmapImage? LoadImage(string fileName)
    {
        try
        {
            var uri = new Uri($"pack://application:,,,/Resources/Branding/{fileName}", UriKind.Absolute);
            return new BitmapImage(uri);
        }
        catch
        {
            return null;
        }
    }

    [RelayCommand]
    private void NavigateTo(string targetId)
    {
        if (_studioTargetIds.Contains(targetId) && !HasActiveProject)
            return;

        _navigationService.NavigateTo(targetId);
    }

    [RelayCommand]
    private void SelectTab(WorkspaceTabViewModel? tab)
    {
        if (tab is null)
            return;

        SetSelectedTab(tab);
        StatusText = tab.DisplayName;
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
        var nav = (NavigationService)_navigationService;

        nav.RegisterTarget("dashboard", "Dashboard", "Home24", false, false,
            () => _serviceProvider.GetRequiredService<DashboardView>());
        nav.RegisterTarget("projects", "Projects", "Folder24", false, false,
            () => _serviceProvider.GetRequiredService<ProjectsWorkspaceView>());
        nav.RegisterTarget("boq", "BOQ Studio", "DocumentBulletList24", true, false, () =>
        {
            var view = _serviceProvider.GetRequiredService<BoqStudioView>();
            view.InitializeTabs(_serviceProvider);
            return view;
        });
        nav.RegisterTarget("wbs", "WBS Studio", "Library24", true, false, () =>
        {
            var view = _serviceProvider.GetRequiredService<WbsStudioView>();
            view.InitializeTabs(_serviceProvider);
            return view;
        });
        nav.RegisterTarget("activity", "Activity Studio", "CalendarDay24", true, false, () =>
        {
            var view = _serviceProvider.GetRequiredService<ActivityStudioView>();
            view.InitializeTabs(_serviceProvider);
            return view;
        });
        nav.RegisterTarget("resource", "Resource Studio", "People24", true, false,
            () =>
            {
                var view = _serviceProvider.GetRequiredService<ResourceStudioView>();
                view.InitializeTabs(_serviceProvider);
                return view;
            });
        nav.RegisterTarget("cost", "Cost Studio", "Money24", true, false, () =>
        {
            var view = _serviceProvider.GetRequiredService<CostStudioView>();
            view.InitializeTabs(_serviceProvider);
            return view;
        });
        nav.RegisterTarget("reports", "Reports", "DocumentText24", true, false, () =>
        {
            var view = _serviceProvider.GetRequiredService<ReportingHubView>();
            var hubVm = (ReportingHubViewModel)view.DataContext;
            hubVm.OnHubStatusMessage = msg => StatusText = msg;
            view.InitializeTabs(_serviceProvider);
            return view;
        });
        nav.RegisterTarget("primavera", "Primavera Studio", "CalendarClock24", true, false, () =>
        {
            var view = _serviceProvider.GetRequiredService<PrimaveraStudioView>();
            view.InitializeTabs(_serviceProvider);
            return view;
        });
        nav.RegisterTarget("schedule-compare", "Schedule Compare", "ArrowSync24", true, false, () =>
        {
            var view = _serviceProvider.GetRequiredService<ScheduleComparisonView>();
            view.InitializeTabs(_serviceProvider);
            return view;
        });
        nav.RegisterTarget("delay-analysis", "Delay Analysis", "ChartMultiple24", true, true,
            () => CreateEmptyState("ChartMultiple24", "Delay Analysis", "Delay analysis module is coming soon."));
        nav.RegisterTarget("claims", "Claims", "DocumentEdit24", false, false,
            () => _serviceProvider.GetRequiredService<ContractsWorkspaceView>());
        nav.RegisterTarget("chronology", "Chronology", "Timeline24", true, true,
            () => CreateEmptyState("Timeline24", "Chronology", "Chronology module is coming soon."));
        nav.RegisterTarget("correspondence", "Correspondence", "Mail24", false, true,
            () => CreateEmptyState("Mail24", "Correspondence", "Correspondence module is coming soon."));
        nav.RegisterTarget("knowledge-base", "Knowledge Base", "BookSearch24", true, true,
            () => CreateEmptyState("BookSearch24", "Knowledge Base", "Knowledge base module is coming soon."));
        nav.RegisterTarget("analytics", "Analytics", "DataHistogram24", false, true,
            () => CreateEmptyState("DataHistogram24", "Analytics", "Analytics module is coming soon."));
        nav.RegisterTarget("integration-hub", "Integration Hub", "PlugConnected24", true, true,
            () => CreateEmptyState("PlugConnected24", "Integration Hub", "Integration hub module is coming soon."));
        nav.RegisterTarget("clients", "Clients", "People24", false, false,
            () => _serviceProvider.GetRequiredService<ClientsWorkspaceView>());
        nav.RegisterTarget("excel-studio", "Excel Studio", "Table24", true, false, () =>
        {
            var view = _serviceProvider.GetRequiredService<ExcelStudioView>();
            view.InitializeTabs(_serviceProvider);
            return view;
        });
        nav.RegisterTarget("settings", "Settings", "Settings24", false, false,
            () => _serviceProvider.GetRequiredService<SettingsView>());
    }

    private void BuildNavigationItems()
    {
        NavigationItems.Clear();
        foreach (var target in _navigationService.GetTargets())
        {
            NavigationItems.Add(new NavigationItemViewModel(
                target.Id,
                target.DisplayName,
                NavigateToCommand,
                target.IconGlyph ?? string.Empty,
                target.IsPlaceholder));
        }

        SetSelectedNavigation("dashboard");
    }

    [RelayCommand]
    private async Task LoadProjectsAsync()
    {
        var projects = await _projectService.GetAllAsync();
        ProjectSelectorItems.Clear();
        foreach (var p in projects)
            ProjectSelectorItems.Add(p);
    }

    private void UpdateStudioNavigationStates()
    {
        foreach (var item in NavigationItems)
        {
            if (_studioTargetIds.Contains(item.Id))
            {
                item.IsEnabled = HasActiveProject;
            }
        }
    }

    private static EmptyStateView CreateEmptyState(string iconGlyph, string title, string description)
    {
        return new EmptyStateView
        {
            DataContext = new { IconGlyph = iconGlyph, Title = title, Description = description }
        };
    }

    private void OnActiveTargetChanged(object? sender, string targetId)
    {
        SetSelectedNavigation(targetId);
        OpenTarget(targetId);
    }

    private void NavigateToTarget(string targetId)
    {
        _navigationService.NavigateTo(targetId);
    }

    private void OpenTarget(string targetId)
    {
        if (_studioTargetIds.Contains(targetId) && !HasActiveProject)
            return;

        if (_openTabs.TryGetValue(targetId, out var existing))
        {
            SetSelectedTab(existing);
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
        SetSelectedTab(tab);
        StatusText = target.DisplayName;
    }

    private void SetSelectedTab(WorkspaceTabViewModel? tab)
    {
        foreach (var item in Tabs)
            item.IsSelected = ReferenceEquals(item, tab);

        SelectedTab = tab;
    }

    private void SetSelectedNavigation(string targetId)
    {
        foreach (var item in NavigationItems)
            item.IsSelected = string.Equals(item.Id, targetId, StringComparison.OrdinalIgnoreCase);
    }
}
