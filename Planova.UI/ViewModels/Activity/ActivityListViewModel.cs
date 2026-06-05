using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Planova.Activity.Application.Dto;
using Planova.Activity.Domain.Interfaces;

namespace Planova.UI.ViewModels.Activity;

public partial class ActivityListViewModel : ObservableObject
{
    private readonly IActivityService _activityService;

    public ActivityListViewModel(IActivityService activityService)
    {
        _activityService = activityService;
    }

    [ObservableProperty]
    private ObservableCollection<ActivityDto> _activities = [];

    [ObservableProperty]
    private ActivityDto? _selectedActivity;

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private string? _statusFilter;

    [ObservableProperty]
    private string? _typeFilter;

    [ObservableProperty]
    private bool _isLoading;

    [RelayCommand]
    private async Task LoadActivitiesAsync(CancellationToken ct)
    {
        IsLoading = true;
        try
        {
            var filter = new ActivityFilter
            {
                SearchText = SearchText,
                Status = StatusFilter,
                ActivityType = TypeFilter
            };
            var activities = await _activityService.GetByProjectAsync(1, filter, ct);
            Activities = new ObservableCollection<ActivityDto>(activities);
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task DeleteActivityAsync(CancellationToken ct)
    {
        if (SelectedActivity is null) return;
        await _activityService.DeleteAsync(SelectedActivity.Id, ct);
        Activities.Remove(SelectedActivity);
    }
}
