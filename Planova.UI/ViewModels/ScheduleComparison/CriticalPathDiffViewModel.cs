using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Planova.ScheduleComparison.Application.Dto;

namespace Planova.UI.ViewModels.ScheduleComparison;

public partial class CriticalPathDiffViewModel : ObservableObject
{
    [ObservableProperty]
    private double? _sourceDuration;

    [ObservableProperty]
    private double? _targetDuration;

    [ObservableProperty]
    private double? _durationChange;

    [ObservableProperty]
    private ObservableCollection<string> _enteredCriticalPath = new();

    [ObservableProperty]
    private ObservableCollection<string> _exitedCriticalPath = new();

    [ObservableProperty]
    private ObservableCollection<string> _remainedOnCriticalPath = new();

    public void LoadDiff(CriticalPathDiffDto? diff)
    {
        if (diff == null)
            return;

        SourceDuration = diff.SourceDuration;
        TargetDuration = diff.TargetDuration;
        DurationChange = diff.DurationChange;
        EnteredCriticalPath = new ObservableCollection<string>(diff.EnteredCriticalPath);
        ExitedCriticalPath = new ObservableCollection<string>(diff.ExitedCriticalPath);
        RemainedOnCriticalPath = new ObservableCollection<string>(diff.RemainedOnCriticalPath);
    }
}
