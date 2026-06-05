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

    public DashboardViewModel(IDashboardService dashboardService, INavigationService navigationService)
    {
        _dashboardService = dashboardService;
        _navigationService = navigationService;
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
    private string _statusDistributionSummary = string.Empty;

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

            if (summary.ProjectsByStatus.Count > 0)
            {
                StatusDistributionSummary = string.Join(", ",
                    summary.ProjectsByStatus.Select(kv => $"{kv.Key}: {kv.Value}"));
            }
            else
            {
                StatusDistributionSummary = "No projects yet";
            }

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
}
