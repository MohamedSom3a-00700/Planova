using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Planova.Application.Dto;
using Planova.Application.Services;
using Planova.Shared.Abstractions;
using Planova.UI.ViewModels.Dashboard;

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
    private bool _hasError;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private bool _isEmpty = true;

    public ObservableCollection<HealthCardViewModel> HealthCards { get; } = new();

    public ObservableCollection<CostCardViewModel> CostCards { get; } = new();

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

    public ObservableCollection<DistributionSummaryItem> StatusDistributionItems { get; } = new();
    public ObservableCollection<DistributionSummaryItem> BoqDistributionItems { get; } = new();
    public ObservableCollection<DistributionSummaryItem> WbsDistributionItems { get; } = new();
    public ObservableCollection<DistributionSummaryItem> ActivityDistributionItems { get; } = new();
    public ObservableCollection<DistributionSummaryItem> ResourceTypeDistributionItems { get; } = new();

    public ObservableCollection<RecentActivityItem> RecentActivity { get; } = new();

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

            PopulateDistributionItems(summary.ProjectsByStatus, StatusDistributionItems, GetStatusColor);
            PopulateDistributionItems(summary.BoqStatusDistribution, BoqDistributionItems, GetStatusColor);
            PopulateDistributionItems(summary.WbsStatusDistribution, WbsDistributionItems, GetStatusColor);
            PopulateDistributionItems(summary.ActivitiesByStatus, ActivityDistributionItems, GetStatusColor);
            PopulateDistributionItems(summary.ResourceTypeDistribution, ResourceTypeDistributionItems, GetResourceTypeColor);

            RecentActivity.Clear();
            foreach (var item in summary.RecentActivity)
                RecentActivity.Add(item);

            IsEmpty = TotalProjects == 0;
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

    private static void PopulateDistributionItems(
        Dictionary<string, int> source,
        ObservableCollection<DistributionSummaryItem> target,
        Func<string, string> colorSelector)
    {
        target.Clear();
        foreach (var kvp in source)
            target.Add(new DistributionSummaryItem
            {
                Label = kvp.Key,
                Count = kvp.Value,
                Color = colorSelector(kvp.Key)
            });
    }

    private static string GetStatusColor(string status) => status switch
    {
        "Active" or "In Progress" or "Approved" or "Submitted" => "#4CAF50",
        "Completed" or "Finished" => "#2196F3",
        "On Hold" or "Draft" or "Pending" or "Not Started" => "#FF9800",
        "Cancelled" or "Rejected" or "Delayed" or "Overdue" => "#F44336",
        _ => "#9E9E9E"
    };

    private static string GetResourceTypeColor(string type) => type switch
    {
        "Labor" => "#2196F3",
        "Equipment" => "#FF9800",
        "Material" => "#4CAF50",
        "Subcontractor" => "#9C27B0",
        _ => "#9E9E9E"
    };
}
