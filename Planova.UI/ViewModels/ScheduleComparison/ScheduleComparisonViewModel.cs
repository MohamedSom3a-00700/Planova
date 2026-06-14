using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using Planova.ScheduleComparison.Domain.Interfaces;

namespace Planova.UI.ViewModels.ScheduleComparison;

public partial class ScheduleComparisonTab : ObservableObject
{
    [ObservableProperty]
    private string _header = string.Empty;

    [ObservableProperty]
    private object _content = null!;

    public ScheduleComparisonTab(string header, object content)
    {
        _header = header;
        _content = content;
    }
}

public partial class ScheduleComparisonViewModel : ObservableObject
{
    private readonly IScheduleComparisonService _comparisonService;
    private readonly IScheduleSnapshotService _snapshotService;

    [ObservableProperty]
    private ScheduleComparisonTab? _selectedTab;

    public ObservableCollection<ScheduleComparisonTab> Tabs { get; } = new();

    public ScheduleComparisonViewModel(
        IScheduleComparisonService comparisonService,
        IScheduleSnapshotService snapshotService)
    {
        _comparisonService = comparisonService;
        _snapshotService = snapshotService;
    }

    public void InitializeTabs(IServiceProvider serviceProvider)
    {
        Tabs.Clear();

        var compareVm = serviceProvider.GetRequiredService<CompareViewModel>();
        Tabs.Add(new ScheduleComparisonTab("Compare", compareVm));

        var activityDiffVm = serviceProvider.GetRequiredService<ActivityDiffViewModel>();
        Tabs.Add(new ScheduleComparisonTab("Activities", activityDiffVm));

        var logicDiffVm = serviceProvider.GetRequiredService<LogicDiffViewModel>();
        Tabs.Add(new ScheduleComparisonTab("Logic", logicDiffVm));

        var resourceDiffVm = serviceProvider.GetRequiredService<ResourceDiffViewModel>();
        Tabs.Add(new ScheduleComparisonTab("Resources", resourceDiffVm));

        var criticalPathVm = serviceProvider.GetRequiredService<CriticalPathDiffViewModel>();
        Tabs.Add(new ScheduleComparisonTab("Critical Path", criticalPathVm));

        var floatVm = serviceProvider.GetRequiredService<FloatImpactViewModel>();
        Tabs.Add(new ScheduleComparisonTab("Float", floatVm));

        var historyVm = serviceProvider.GetRequiredService<ComparisonHistoryViewModel>();
        Tabs.Add(new ScheduleComparisonTab("History", historyVm));

        var exportVm = serviceProvider.GetRequiredService<ComparisonExportViewModel>();
        Tabs.Add(new ScheduleComparisonTab("Export", exportVm));

        if (Tabs.Count > 0)
            SelectedTab = Tabs[0];
    }
}
