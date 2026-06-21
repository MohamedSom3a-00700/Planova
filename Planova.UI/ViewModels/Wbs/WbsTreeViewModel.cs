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

public enum WbsTreeViewMode
{
    Standard,
    PrimaveraColors,
    WeightView,
    ResponsibilityView
}

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

    public List<WbsTreeViewMode> ViewModes { get; } = new()
    {
        WbsTreeViewMode.Standard,
        WbsTreeViewMode.PrimaveraColors,
        WbsTreeViewMode.WeightView,
        WbsTreeViewMode.ResponsibilityView
    };

    [ObservableProperty]
    private WbsEntity? _selectedWbs;

    [ObservableProperty]
    private Guid _wbsId;

    [ObservableProperty]
    private bool _usePrimaveraColors;

    [ObservableProperty]
    private WbsTreeViewMode _viewMode = WbsTreeViewMode.Standard;

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private ObservableCollection<WbsTreeItemNode> _filteredRootNodes = new();

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

    partial void OnViewModeChanged(WbsTreeViewMode value)
    {
        switch (value)
        {
            case WbsTreeViewMode.Standard:
                UsePrimaveraColors = false;
                break;
            case WbsTreeViewMode.PrimaveraColors:
                UsePrimaveraColors = true;
                break;
        }
        RebuildNodeDisplay();
    }

    partial void OnSearchTextChanged(string value)
    {
        ApplySearchFilter();
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
        ApplySearchFilter();
    }

    [RelayCommand]
    private void ExpandAll()
    {
        SetExpandCollapseAll(RootNodes, true);
    }

    [RelayCommand]
    private void CollapseAll()
    {
        SetExpandCollapseAll(RootNodes, false);
    }

    private static void SetExpandCollapseAll(ObservableCollection<WbsTreeItemNode> nodes, bool expand)
    {
        foreach (var node in nodes)
        {
            node.IsExpanded = expand;
            SetExpandCollapseAll(node.Children, expand);
        }
    }

    [RelayCommand]
    private void SearchNode()
    {
        ApplySearchFilter();
    }

    [RelayCommand]
    private void ClearSearch()
    {
        SearchText = string.Empty;
    }

    private void ApplySearchFilter()
    {
        if (string.IsNullOrWhiteSpace(SearchText))
        {
            FilteredRootNodes = new ObservableCollection<WbsTreeItemNode>(RootNodes);
            return;
        }

        var filtered = new ObservableCollection<WbsTreeItemNode>();
        var search = SearchText.Trim().ToUpperInvariant();
        foreach (var node in RootNodes)
        {
            var match = FilterNode(node, search);
            if (match is not null)
                filtered.Add(match);
        }
        FilteredRootNodes = filtered;
    }

    private static WbsTreeItemNode? FilterNode(WbsTreeItemNode node, string search)
    {
        var matches = node.Name.Contains(search, StringComparison.OrdinalIgnoreCase)
                      || node.Code.Contains(search, StringComparison.OrdinalIgnoreCase);

        var filteredChildren = new ObservableCollection<WbsTreeItemNode>();
        foreach (var child in node.Children)
        {
            var childMatch = FilterNode(child, search);
            if (childMatch is not null)
                filteredChildren.Add(childMatch);
        }

        if (matches || filteredChildren.Count > 0)
        {
            var clone = CloneNode(node);
            foreach (var fc in filteredChildren)
                clone.Children.Add(fc);
            if (filteredChildren.Count > 0)
                clone.IsExpanded = true;
            return clone;
        }

        return null;
    }

    private static WbsTreeItemNode CloneNode(WbsTreeItemNode node)
    {
        return new WbsTreeItemNode
        {
            Id = node.Id,
            Code = node.Code,
            Name = node.Name,
            Level = node.Level,
            Weight = node.Weight,
            WbsLevel = node.WbsLevel,
            WeightPercent = node.WeightPercent,
            LevelColor = node.LevelColor,
            AssignedTo = node.AssignedTo,
            Owner = node.Owner,
            NodeInfo = node.NodeInfo
        };
    }

    private void RebuildNodeDisplay()
    {
        foreach (var node in RootNodes)
            RebuildNodeDisplayRecursive(node);
        ApplySearchFilter();
    }

    private void RebuildNodeDisplayRecursive(WbsTreeItemNode node)
    {
        node.NodeInfo = ViewMode switch
        {
            WbsTreeViewMode.WeightView when node.Weight.HasValue => $"{node.Weight:F1}%",
            WbsTreeViewMode.ResponsibilityView => !string.IsNullOrEmpty(node.AssignedTo)
                ? $"({node.AssignedTo})"
                : "",
            _ => ""
        };
        foreach (var child in node.Children)
            RebuildNodeDisplayRecursive(child);
    }

    private WbsTreeItemNode BuildNode(WbsItem item, IReadOnlyList<WbsItem> allItems, bool usePrimavera)
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
                : GetLevelColor(item.WbsLevel),
            AssignedTo = item.AssignedTo,
            Owner = item.Owner
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

    [ObservableProperty]
    private string _nodeInfo = string.Empty;

    public string? AssignedTo { get; set; }
    public string? Owner { get; set; }

    public ObservableCollection<WbsTreeItemNode> Children { get; } = new();

    [ObservableProperty]
    private bool _isExpanded;

    [ObservableProperty]
    private bool _isSelected;
}
