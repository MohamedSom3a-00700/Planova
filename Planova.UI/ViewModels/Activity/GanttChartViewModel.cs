using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Planova.Activity.Application.Dto;
using Planova.Activity.Domain.Interfaces;

namespace Planova.UI.ViewModels.Activity;

public partial class GanttChartViewModel : ObservableObject
{
    private readonly IActivityService _activityService;
    private readonly IActivityRelationshipService _relationshipService;

    public GanttChartViewModel(IActivityService activityService, IActivityRelationshipService relationshipService)
    {
        _activityService = activityService;
        _relationshipService = relationshipService;
    }

    [ObservableProperty]
    private bool _isRtl;

    [ObservableProperty]
    private ObservableCollection<ActivityDto> _activities = [];

    [ObservableProperty]
    private List<ActivityRelationshipDto> _relationships = [];

    [ObservableProperty]
    private DateTime _viewStart;

    [ObservableProperty]
    private DateTime _viewEnd;

    [ObservableProperty]
    private string _zoomLevel = "Week";

    [ObservableProperty]
    private double _timeScale = 20.0;

    [RelayCommand]
    private void ZoomIn()
    {
        ZoomLevel = ZoomLevel switch
        {
            "Month" => "Week",
            "Week" => "Day",
            _ => "Day"
        };
        UpdateTimeScale();
    }

    [RelayCommand]
    private void ZoomOut()
    {
        ZoomLevel = ZoomLevel switch
        {
            "Day" => "Week",
            "Week" => "Month",
            _ => "Month"
        };
        UpdateTimeScale();
    }

    [RelayCommand]
    private async Task LoadDataAsync(CancellationToken ct)
    {
        var activities = await _activityService.GetByProjectAsync(1, null, ct);
        Activities = new ObservableCollection<ActivityDto>(activities);

        var relationships = await _relationshipService.GetByProjectAsync(1, ct);
        Relationships = relationships;

        if (activities.Count > 0)
        {
            ViewStart = activities.Min(a => a.PlannedStart) ?? DateTime.Today.AddDays(-7);
            ViewEnd = activities.Max(a => a.PlannedFinish) ?? DateTime.Today.AddDays(30);
        }
        else
        {
            ViewStart = DateTime.Today.AddDays(-7);
            ViewEnd = DateTime.Today.AddDays(30);
        }

        UpdateTimeScale();
    }

    private void UpdateTimeScale()
    {
        TimeScale = ZoomLevel switch
        {
            "Day" => 40.0,
            "Week" => 20.0,
            "Month" => 5.0,
            _ => 20.0
        };
    }
}
