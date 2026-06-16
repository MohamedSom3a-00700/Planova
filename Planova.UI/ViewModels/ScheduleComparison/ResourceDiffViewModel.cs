using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Planova.ScheduleComparison.Application.Dto;

namespace Planova.UI.ViewModels.ScheduleComparison;

public partial class ResourceDiffViewModel : ObservableObject
{
    [ObservableProperty]
    private ObservableCollection<ResourceDiffDto> _diffs = new();

    [ObservableProperty]
    private int _totalCount;

    [ObservableProperty]
    private int _addedCount;

    [ObservableProperty]
    private int _removedCount;

    [ObservableProperty]
    private int _modifiedCount;

    public void LoadDiffs(List<ResourceDiffDto> diffs)
    {
        Diffs = new ObservableCollection<ResourceDiffDto>(diffs);
        TotalCount = diffs.Count;
        AddedCount = diffs.Count(d => d.ChangeType == "Added");
        RemovedCount = diffs.Count(d => d.ChangeType == "Removed");
        ModifiedCount = diffs.Count(d => d.ChangeType == "Modified");
    }
}
