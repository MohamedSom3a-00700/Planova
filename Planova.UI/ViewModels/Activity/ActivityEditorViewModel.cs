using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Planova.Activity.Application.Dto;
using Planova.Activity.Domain.Interfaces;

namespace Planova.UI.ViewModels.Activity;

public partial class ActivityEditorViewModel : ObservableObject
{
    private readonly IActivityService _activityService;

    public ActivityEditorViewModel(IActivityService activityService)
    {
        _activityService = activityService;
    }

    [ObservableProperty]
    private Guid _activityId;

    [ObservableProperty]
    private int _projectId = 1;

    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private string? _description;

    [ObservableProperty]
    private string _activityType = "Task";

    [ObservableProperty]
    private int? _duration;

    [ObservableProperty]
    private DateTime? _plannedStart;

    [ObservableProperty]
    private DateTime? _plannedFinish;

    [ObservableProperty]
    private decimal? _weight;

    [ObservableProperty]
    private string? _notes;

    [ObservableProperty]
    private bool _isEditing;

    [ObservableProperty]
    private string? _errorMessage;

    [RelayCommand]
    private async Task SaveAsync(CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(Name))
        {
            ErrorMessage = "Name is required.";
            return;
        }

        ErrorMessage = null;

        if (IsEditing)
        {
            await _activityService.UpdateAsync(new UpdateActivityRequest
            {
                Id = ActivityId,
                Name = Name,
                Description = Description,
                Duration = Duration,
                PlannedStart = PlannedStart,
                PlannedFinish = PlannedFinish,
                Weight = Weight,
                Notes = Notes
            }, ct);
        }
        else
        {
            await _activityService.CreateAsync(new CreateActivityRequest
            {
                ProjectId = ProjectId,
                Name = Name,
                Description = Description,
                ActivityType = ActivityType,
                Duration = Duration,
                PlannedStart = PlannedStart,
                PlannedFinish = PlannedFinish,
                Weight = Weight,
                Notes = Notes
            }, ct);
        }
    }

    public void LoadActivity(ActivityDto activity)
    {
        ActivityId = activity.Id;
        ProjectId = activity.ProjectId;
        Name = activity.Name;
        Description = activity.Description;
        ActivityType = activity.ActivityType;
        Duration = activity.Duration;
        PlannedStart = activity.PlannedStart;
        PlannedFinish = activity.PlannedFinish;
        Weight = activity.Weight;
        Notes = activity.Notes;
        IsEditing = true;
    }

    public void Reset()
    {
        ActivityId = Guid.Empty;
        Name = string.Empty;
        Description = null;
        ActivityType = "Task";
        Duration = null;
        PlannedStart = null;
        PlannedFinish = null;
        Weight = null;
        Notes = null;
        IsEditing = false;
        ErrorMessage = null;
    }
}
