using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Planova.Application.Dto;
using Planova.Application.Services;
using Planova.Shared.Abstractions;

namespace Planova.UI.ViewModels;

public partial class DashboardViewModel : ObservableObject
{
    private readonly IDashboardService _dashboardService;
    private readonly INavigationService _navigationService;
    private readonly ICurrentProjectService _currentProjectService;

    public DashboardViewModel(
        IDashboardService dashboardService,
        INavigationService navigationService,
        ICurrentProjectService currentProjectService)
    {
        _dashboardService = dashboardService;
        _navigationService = navigationService;
        _currentProjectService = currentProjectService;

        _navigationService.ActiveTargetChanged += OnActiveTargetChanged;
        _currentProjectService.CurrentProjectChanged += OnCurrentProjectChanged;
    }

    private void OnActiveTargetChanged(object? sender, string targetId)
    {
        if (targetId == "dashboard")
            LoadCommand.Execute(null);
    }

    private void OnCurrentProjectChanged(object? sender, ProjectContext? project)
    {
        LoadCommand.Execute(null);
    }

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private int _totalProjects;

    [ObservableProperty]
    private int _totalClients;

    [ObservableProperty]
    private int _totalContracts;

    [ObservableProperty]
    private int _totalBoqs;

    [ObservableProperty]
    private int _totalWbsEntries;

    [ObservableProperty]
    private int _totalActivities;

    [ObservableProperty]
    private int _totalResources;

    [ObservableProperty]
    private string _statusDistributionSummary = string.Empty;

    [ObservableProperty]
    private string _boqDistributionSummary = string.Empty;

    [ObservableProperty]
    private string _wbsDistributionSummary = string.Empty;

    [ObservableProperty]
    private string _activityDistributionSummary = string.Empty;

    [ObservableProperty]
    private string _resourceTypeSummary = string.Empty;

    public ObservableCollection<RecentActivityItem> RecentActivity { get; } = new();

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private bool _hasError;

    [RelayCommand]
    private async Task LoadAsync()
    {
        IsLoading = true;
        HasError = false;

        try
        {
            var summary = await _dashboardService.GetSummaryAsync();
            TotalProjects = summary.TotalProjects;
            TotalClients = summary.TotalClients;
            TotalContracts = summary.TotalContracts;
            TotalBoqs = summary.TotalBoqs;
            TotalWbsEntries = summary.TotalWbsEntries;
            TotalActivities = summary.TotalActivities;
            TotalResources = summary.TotalResources;

            StatusDistributionSummary = summary.ProjectsByStatus.Count > 0
                ? string.Join(", ", summary.ProjectsByStatus.Select(kv => $"{kv.Key}: {kv.Value}"))
                : "No projects yet";

            BoqDistributionSummary = summary.BoqStatusDistribution.Count > 0
                ? string.Join(", ", summary.BoqStatusDistribution.Select(kv => $"{kv.Key}: {kv.Value}"))
                : "No BOQs yet";

            WbsDistributionSummary = summary.WbsStatusDistribution.Count > 0
                ? string.Join(", ", summary.WbsStatusDistribution.Select(kv => $"{kv.Key}: {kv.Value}"))
                : "No WBS entries yet";

            ActivityDistributionSummary = summary.ActivitiesByStatus.Count > 0
                ? string.Join(", ", summary.ActivitiesByStatus.Select(kv => $"{kv.Key}: {kv.Value}"))
                : "No activities yet";

            ResourceTypeSummary = summary.ResourceTypeDistribution.Count > 0
                ? string.Join(", ", summary.ResourceTypeDistribution.Select(kv => $"{kv.Key}: {kv.Value}"))
                : "No resources yet";

            RecentActivity.Clear();
            foreach (var item in summary.RecentActivity)
                RecentActivity.Add(item);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to load dashboard: {ex.Message}";
            HasError = true;
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void NavigateToProjects()
    {
        _navigationService.NavigateTo("projects");
    }

    [RelayCommand]
    private void NavigateToClients()
    {
        _navigationService.NavigateTo("clients");
    }

    [RelayCommand]
    private void NavigateToContracts()
    {
        _navigationService.NavigateTo("contracts");
    }

    [RelayCommand]
    private void NavigateToBoq()
    {
        _navigationService.NavigateTo("boq");
    }

    [RelayCommand]
    private void NavigateToWbs()
    {
        _navigationService.NavigateTo("wbs");
    }

    [RelayCommand]
    private void NavigateToActivity()
    {
        _navigationService.NavigateTo("activity");
    }

    [RelayCommand]
    private void NavigateToResource()
    {
        _navigationService.NavigateTo("resource");
    }
}
