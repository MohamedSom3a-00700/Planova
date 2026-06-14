using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Planova.ScheduleComparison.Application.Dto;

namespace Planova.UI.ViewModels.ScheduleComparison;

public partial class FloatImpactViewModel : ObservableObject
{
    [ObservableProperty]
    private ObservableCollection<FloatImpactDto> _floatDeltas = new();

    [ObservableProperty]
    private int _negativeFloatCount;

    [ObservableProperty]
    private int _improvedCount;

    [ObservableProperty]
    private int _worsenedCount;

    public void LoadReport(List<FloatImpactDto> deltas, List<string> negative, List<string> improved, List<string> worsened)
    {
        FloatDeltas = new ObservableCollection<FloatImpactDto>(deltas);
        NegativeFloatCount = negative.Count;
        ImprovedCount = improved.Count;
        WorsenedCount = worsened.Count;
    }
}
