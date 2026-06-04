using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Planova.Boq.Domain.Interfaces;
using Planova.Wbs.Domain.Entities;
using Planova.Wbs.Domain.Enums;
using Planova.Wbs.Domain.Interfaces;

namespace Planova.UI.ViewModels.Wbs;

using WbsEntity = Planova.Wbs.Domain.Entities.Wbs;

public sealed partial class WbsTreeViewModel : ObservableObject
{
    private readonly IWbsService _wbsService;
    private readonly IBoqSession _session;

    public WbsTreeViewModel(IWbsService wbsService, IBoqSession session)
    {
        _wbsService = wbsService;
        _session = session;
        _ = LoadWbsListAsync(CancellationToken.None);

        WeakReferenceMessenger.Default.Register<WbsStudioMessage>(this, async (r, m) =>
        {
            await LoadWbsListAsync(CancellationToken.None);
        });
    }

    public ObservableCollection<WbsTreeItemNode> RootNodes { get; } = new();

    public ObservableCollection<WbsEntity> AvailableWbsList { get; } = new();

    [ObservableProperty]
    private WbsEntity? _selectedWbs;

    [ObservableProperty]
    private Guid _wbsId;

    [ObservableProperty]
    private bool _usePrimaveraColors;

    partial void OnSelectedWbsChanged(WbsEntity? value)
    {
        if (value is not null)
        {
            WbsId = value.Id;
            _ = LoadTreeAsync(CancellationToken.None);
        }
    }

    partial void OnUsePrimaveraColorsChanged(bool value)
    {
        RebuildColors();
    }

    [RelayCommand]
    private async Task LoadWbsListAsync(CancellationToken ct)
    {
        try
        {
            var projectId = _session.CurrentProjectId;
            if (projectId is null) return;
            var list = await _wbsService.GetByProjectAsync(projectId.Value.GetHashCode(), ct);
            AvailableWbsList.Clear();
            foreach (var wbs in list)
                AvailableWbsList.Add(wbs);
        }
        catch
        {
            // non-critical
        }
    }

    [RelayCommand]
    private async Task LoadTreeAsync(CancellationToken ct)
    {
        var items = await _wbsService.GetTreeAsync(WbsId, ct);
        RootNodes.Clear();
        var roots = items.Where(i => i.ParentId == null).OrderBy(i => i.SortOrder);
        foreach (var root in roots)
            RootNodes.Add(BuildNode(root, items, UsePrimaveraColors));
    }

    private static WbsTreeItemNode BuildNode(WbsItem item, IReadOnlyList<WbsItem> allItems, bool usePrimavera)
    {
        var node = new WbsTreeItemNode
        {
            Id = item.Id,
            Code = item.Code,
            Name = item.Name,
            Level = item.Level,
            Weight = item.Weight,
            WbsLevel = item.WbsLevel.ToString(),
            WeightPercent = item.Weight.HasValue ? $"{item.Weight.Value:F1}%" : "",
            LevelColor = usePrimavera
                ? GetPrimaveraLevelColor(item.Level)
                : GetLevelColor(item.WbsLevel)
        };
        var children = allItems.Where(i => i.ParentId == item.Id).OrderBy(i => i.SortOrder);
        foreach (var child in children)
            node.Children.Add(BuildNode(child, allItems, usePrimavera));
        return node;
    }

    private void RebuildColors()
    {
        foreach (var node in RootNodes)
            RebuildColorsRecursive(node);
    }

    private void RebuildColorsRecursive(WbsTreeItemNode node)
    {
        node.LevelColor = UsePrimaveraColors
            ? GetPrimaveraLevelColor(node.Level)
            : GetLevelColor(Enum.TryParse<WbsLevelType>(node.WbsLevel, out var wl) ? wl : WbsLevelType.Summary);
        foreach (var child in node.Children)
            RebuildColorsRecursive(child);
    }

    private static string GetLevelColor(WbsLevelType level) => level switch
    {
        WbsLevelType.Summary => "#2196F3",
        WbsLevelType.ControlAccount => "#4CAF50",
        WbsLevelType.WorkPackage => "#FF9800",
        WbsLevelType.PlanningPackage => "#9E9E9E",
        _ => "#757575"
    };

    private static string GetPrimaveraLevelColor(int level) => level switch
    {
        0 => "#1A237E",
        1 => "#283593",
        2 => "#1565C0",
        3 => "#00897B",
        4 => "#E65100",
        5 => "#AD1457",
        _ => "#546E7A"
    };
}

public sealed partial class WbsTreeItemNode : ObservableObject
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int Level { get; set; }
    public decimal? Weight { get; set; }
    public string WbsLevel { get; set; } = string.Empty;

    [ObservableProperty]
    private string _levelColor = "#757575";

    public string WeightPercent { get; set; } = string.Empty;

    public ObservableCollection<WbsTreeItemNode> Children { get; } = new();

    [ObservableProperty]
    private bool _isExpanded;

    [ObservableProperty]
    private bool _isSelected;
}
