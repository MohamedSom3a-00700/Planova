using System.Collections.ObjectModel;
using System.Text.Json;
using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using Planova.ScheduleComparison.Application.Dto;
using Planova.ScheduleComparison.Application.Mappings;
using Planova.ScheduleComparison.Application.Models;
using Planova.ScheduleComparison.Domain.Interfaces;

namespace Planova.UI.ViewModels.ScheduleComparison;

public partial class ScheduleComparisonTab : ObservableObject
{
    [ObservableProperty]
    private string _header = string.Empty;

    [ObservableProperty]
    private object _content = null!;

    [ObservableProperty]
    private bool _isEnabled = true;

    public ScheduleComparisonTab(string header, object content, bool isEnabled = true)
    {
        _header = header;
        _content = content;
        _isEnabled = isEnabled;
    }
}

public partial class ScheduleComparisonViewModel : ObservableObject
{
    private readonly IScheduleComparisonService _comparisonService;
    private readonly IScheduleSnapshotService _snapshotService;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter() }
    };

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
        compareVm.OnComparisonCompleted = async (resultJson) => await LoadSessionResultAsync(resultJson);
        Tabs.Add(new ScheduleComparisonTab("Compare", compareVm, isEnabled: true));

        var activityDiffVm = serviceProvider.GetRequiredService<ActivityDiffViewModel>();
        Tabs.Add(new ScheduleComparisonTab("Activities", activityDiffVm, isEnabled: false));

        var logicDiffVm = serviceProvider.GetRequiredService<LogicDiffViewModel>();
        Tabs.Add(new ScheduleComparisonTab("Logic", logicDiffVm, isEnabled: false));

        var resourceDiffVm = serviceProvider.GetRequiredService<ResourceDiffViewModel>();
        Tabs.Add(new ScheduleComparisonTab("Resources", resourceDiffVm, isEnabled: false));

        var criticalPathVm = serviceProvider.GetRequiredService<CriticalPathDiffViewModel>();
        Tabs.Add(new ScheduleComparisonTab("Critical Path", criticalPathVm, isEnabled: false));

        var floatVm = serviceProvider.GetRequiredService<FloatImpactViewModel>();
        Tabs.Add(new ScheduleComparisonTab("Float", floatVm, isEnabled: false));

        var historyVm = serviceProvider.GetRequiredService<ComparisonHistoryViewModel>();
        Tabs.Add(new ScheduleComparisonTab("History", historyVm, isEnabled: true));

        var exportVm = serviceProvider.GetRequiredService<ComparisonExportViewModel>();
        Tabs.Add(new ScheduleComparisonTab("Export", exportVm, isEnabled: false));

        if (Tabs.Count > 0)
            SelectedTab = Tabs[0];
    }

    public async Task LoadSessionResultAsync(string resultJson)
    {
        var result = JsonSerializer.Deserialize<ScheduleComparisonResult>(resultJson, JsonOptions);
        if (result == null)
            return;

        foreach (var tab in Tabs)
        {
            switch (tab.Content)
            {
                case CompareViewModel vm:
                    vm.CurrentSummary = result.Summary.ToSummaryDto();
                    break;
                case ActivityDiffViewModel vm:
                    vm.LoadDiffs(result.ActivityDiffs.Select(d => d.ToDto()).ToList());
                    break;
                case LogicDiffViewModel vm:
                    vm.LoadDiffs(result.LogicDiffs.Select(d => d.ToDto()).ToList());
                    break;
                case ResourceDiffViewModel vm:
                    vm.LoadDiffs(result.ResourceDiffs.Select(d => d.ToDto()).ToList());
                    break;
                case CriticalPathDiffViewModel vm:
                    vm.LoadDiff(result.CriticalPathDiffResult?.ToDto());
                    break;
                case FloatImpactViewModel vm:
                    vm.LoadReport(
                        result.FloatReport?.ActivityFloatDeltas.Select(d => d.ToDto()).ToList() ?? [],
                        result.FloatReport?.ActivitiesWithNegativeFloat ?? [],
                        result.FloatReport?.ActivitiesWithImprovedFloat ?? [],
                        result.FloatReport?.ActivitiesWithWorsenedFloat ?? []);
                    break;
                case ComparisonExportViewModel vm:
                    vm.SessionId = result.SessionId;
                    break;
            }
        }

        foreach (var tab in Tabs)
        {
            if (tab.Content is not CompareViewModel and not ComparisonHistoryViewModel)
                tab.IsEnabled = true;
        }

        SelectedTab = Tabs.FirstOrDefault(t => t.Content is ActivityDiffViewModel) ?? Tabs.FirstOrDefault();
    }
}
