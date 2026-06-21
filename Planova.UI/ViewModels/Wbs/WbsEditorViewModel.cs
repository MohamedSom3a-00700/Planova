using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Planova.Boq.Domain.Interfaces;
using Planova.Wbs.Domain.Entities;
using Planova.Wbs.Domain.Enums;
using Planova.Wbs.Domain.Interfaces;

using WbsEntity = Planova.Wbs.Domain.Entities.Wbs;

namespace Planova.UI.ViewModels.Wbs;

public sealed partial class WbsEditorViewModel : ObservableObject
{
    private readonly IWbsService _wbsService;
    private readonly IWbsItemRepository _itemRepository;
    private readonly IWbsValidationService _validationService;
    private readonly IWbsTemplateService _templateService;
    private readonly IBoqSession _session;
    private readonly IWbsCodeGenerationService _codeGenerationService;

    private bool _autoGenerateCodes = true;

    public WbsEditorViewModel(
        IWbsService wbsService,
        IWbsItemRepository itemRepository,
        IWbsValidationService validationService,
        IWbsTemplateService templateService,
        IBoqSession session,
        IWbsCodeGenerationService codeGenerationService)
    {
        _wbsService = wbsService;
        _itemRepository = itemRepository;
        _validationService = validationService;
        _templateService = templateService;
        _session = session;
        _codeGenerationService = codeGenerationService;
        _ = LoadTemplatesAsync(CancellationToken.None);
        _ = LoadWbsListAsync(CancellationToken.None);

        WeakReferenceMessenger.Default.Register<WbsStudioMessage>(this, async (r, m) =>
        {
            await LoadTemplatesAsync(CancellationToken.None);
            await LoadWbsListAsync(CancellationToken.None);
        });
    }

    public ObservableCollection<WbsItem> Items { get; } = new();

    public ObservableCollection<WbsItem> SelectedItems { get; } = new();

    public ObservableCollection<WbsTemplate> AvailableTemplates { get; } = new();

    public ObservableCollection<WbsEntity> AvailableWbsList { get; } = new();

    [ObservableProperty]
    private WbsTemplate? _selectedTemplate;

    [ObservableProperty]
    private WbsEntity? _selectedWbs;

    [ObservableProperty]
    private Guid _wbsId;

    [ObservableProperty]
    private WbsItem? _selectedItem;

    [ObservableProperty]
    private bool _isBulkEdit;

    [ObservableProperty]
    private DateTime? _bulkPlannedStart;

    [ObservableProperty]
    private DateTime? _bulkPlannedFinish;

    [ObservableProperty]
    private string? _bulkAssignedTo;

    [ObservableProperty]
    private Guid? _dragDropItemId;

    public List<string> WbsLevelOptions { get; } = new()
    {
        "Summary", "ControlAccount", "WorkPackage", "PlanningPackage"
    };

    partial void OnSelectedWbsChanged(WbsEntity? value)
    {
        if (value is not null)
        {
            WbsId = value.Id;
            _ = LoadItemsAsync(CancellationToken.None);
        }
    }

    partial void OnSelectedItemChanged(WbsItem? value)
    {
        OnPropertyChanged(nameof(SelectedItemWbsLevel));
        OnPropertyChanged(nameof(SelectedNodeDiscipline));
        OnPropertyChanged(nameof(SelectedNodeCode));
    }

    public string? SelectedNodeDiscipline
    {
        get => SelectedItem?.Discipline;
        set
        {
            if (SelectedItem is not null)
            {
                SelectedItem.Discipline = value;
                OnPropertyChanged();
            }
        }
    }

    public string? SelectedNodeCode
    {
        get => SelectedItem?.Code;
        set
        {
            if (SelectedItem is not null)
            {
                SelectedItem.Code = value ?? string.Empty;
                OnPropertyChanged();
            }
        }
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

    public string? SelectedItemWbsLevel
    {
        get => SelectedItem?.WbsLevel.ToString();
        set
        {
            if (SelectedItem is not null && value is not null
                && Enum.TryParse<WbsLevelType>(value, out var level))
            {
                SelectedItem.WbsLevel = level;
                OnPropertyChanged();
            }
        }
    }

    [RelayCommand]
    private async Task LoadTemplatesAsync(CancellationToken ct)
    {
        try
        {
            var templates = await _templateService.GetAllAsync(ct);
            AvailableTemplates.Clear();
            foreach (var t in templates)
                AvailableTemplates.Add(t);
        }
        catch
        {
            // non-critical
        }
    }

    [RelayCommand]
    private async Task LoadFromTemplateAsync(CancellationToken ct)
    {
        if (SelectedTemplate is null) return;

        IsBulkEdit = false;

        try
        {
            var projectId = _session.CurrentProjectId;
            var wbs = await _templateService.ApplyAsync(
                SelectedTemplate.Id, $"WBS from {SelectedTemplate.Name}",
                projectId?.GetHashCode() ?? 0, ct);
            WbsId = wbs.Id;
            await LoadItemsAsync(ct);
        }
        catch (Exception ex)
        {
            // Template load failed silently
            System.Diagnostics.Debug.WriteLine($"Template load failed: {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task AddChildAsync(CancellationToken ct)
    {
        if (SelectedItem?.WbsLevel == WbsLevelType.WorkPackage) return;

        var item = new WbsItem
        {
            Id = Guid.NewGuid(),
            WbsId = WbsId,
            ParentId = SelectedItem?.Id,
            Name = "New Item",
            Level = (SelectedItem?.Level ?? -1) + 1,
            SortOrder = Items.Count,
            IsActive = true,
        };
        await _itemRepository.AddAsync(item, ct);
        await _wbsService.RedistributeWeightsAsync(WbsId, ct);
        await AutoRegenerateCodesAsync(ct);
        await LoadItemsAsync(ct);
    }

    [RelayCommand]
    private async Task DeleteItemAsync(CancellationToken ct)
    {
        if (SelectedItem == null) return;
        var allItems = await _itemRepository.GetByWbsIdAsync(WbsId, ct);
        var idsToDelete = CollectDescendantIds(SelectedItem.Id, allItems);
        idsToDelete.Add(SelectedItem.Id);
        await _itemRepository.DeleteRangeAsync(idsToDelete, ct);
        await _wbsService.RedistributeWeightsAsync(WbsId, ct);
        await AutoRegenerateCodesAsync(ct);
        await LoadItemsAsync(ct);
    }

    [RelayCommand]
    private async Task MoveUpAsync(CancellationToken ct)
    {
        if (SelectedItem == null) return;
        var siblings = (await _itemRepository.GetByWbsIdAsync(WbsId, ct))
            .Where(i => i.ParentId == SelectedItem.ParentId)
            .OrderBy(i => i.SortOrder)
            .ToList();
        var idx = siblings.FindIndex(i => i.Id == SelectedItem.Id);
        if (idx > 0)
        {
            (siblings[idx].SortOrder, siblings[idx - 1].SortOrder) = (siblings[idx - 1].SortOrder, siblings[idx].SortOrder);
            await _itemRepository.UpdateRangeAsync(new[] { siblings[idx], siblings[idx - 1] }, ct);
            await AutoRegenerateCodesAsync(ct);
            await LoadItemsAsync(ct);
        }
    }

    [RelayCommand]
    private async Task MoveDownAsync(CancellationToken ct)
    {
        if (SelectedItem == null) return;
        var siblings = (await _itemRepository.GetByWbsIdAsync(WbsId, ct))
            .Where(i => i.ParentId == SelectedItem.ParentId)
            .OrderBy(i => i.SortOrder)
            .ToList();
        var idx = siblings.FindIndex(i => i.Id == SelectedItem.Id);
        if (idx < siblings.Count - 1)
        {
            (siblings[idx].SortOrder, siblings[idx + 1].SortOrder) = (siblings[idx + 1].SortOrder, siblings[idx].SortOrder);
            await _itemRepository.UpdateRangeAsync(new[] { siblings[idx], siblings[idx + 1] }, ct);
            await AutoRegenerateCodesAsync(ct);
            await LoadItemsAsync(ct);
        }
    }

    [RelayCommand]
    private async Task SaveItemAsync(CancellationToken ct)
    {
        if (SelectedItem == null) return;
        var errors = await _validationService.ValidateItemAsync(SelectedItem, Items, ct);
        if (errors.Count > 0) return;
        await _itemRepository.UpdateRangeAsync(new[] { SelectedItem }, ct);
    }

    [RelayCommand]
    private async Task BulkUpdateAsync(CancellationToken ct)
    {
        if (SelectedItems.Count == 0) return;

        foreach (var item in SelectedItems)
        {
            if (BulkPlannedStart.HasValue)
                item.PlannedStart = BulkPlannedStart;
            if (BulkPlannedFinish.HasValue)
                item.PlannedFinish = BulkPlannedFinish;
            if (BulkAssignedTo != null)
                item.AssignedTo = BulkAssignedTo;
        }

        await _itemRepository.UpdateRangeAsync(SelectedItems, ct);
        await LoadItemsAsync(ct);

        BulkPlannedStart = null;
        BulkPlannedFinish = null;
        BulkAssignedTo = null;
        IsBulkEdit = false;
    }

    [RelayCommand]
    private async Task AddRootAsync(CancellationToken ct)
    {
        var item = new WbsItem
        {
            Id = Guid.NewGuid(),
            WbsId = WbsId,
            ParentId = null,
            Name = "New Root Item",
            Level = 0,
            SortOrder = Items.Count,
            IsActive = true,
        };
        await _itemRepository.AddAsync(item, ct);
        await _wbsService.RedistributeWeightsAsync(WbsId, ct);
        await AutoRegenerateCodesAsync(ct);
        await LoadItemsAsync(ct);
    }

    [RelayCommand]
    private async Task AddSiblingAsync(CancellationToken ct)
    {
        if (SelectedItem == null) return;

        var item = new WbsItem
        {
            Id = Guid.NewGuid(),
            WbsId = WbsId,
            ParentId = SelectedItem.ParentId,
            Name = "New Sibling",
            Level = SelectedItem.Level,
            SortOrder = SelectedItem.SortOrder + 1,
            IsActive = true,
        };
        await _itemRepository.AddAsync(item, ct);
        await _wbsService.RedistributeWeightsAsync(WbsId, ct);
        await AutoRegenerateCodesAsync(ct);
        await LoadItemsAsync(ct);
    }

    [RelayCommand]
    private async Task DuplicateNodeAsync(CancellationToken ct)
    {
        if (SelectedItem == null) return;

        var duplicate = new WbsItem
        {
            Id = Guid.NewGuid(),
            WbsId = WbsId,
            ParentId = SelectedItem.ParentId,
            Code = SelectedItem.Code,
            ShortCode = SelectedItem.ShortCode,
            Name = SelectedItem.Name + " (Copy)",
            Description = SelectedItem.Description,
            Level = SelectedItem.Level,
            SortOrder = SelectedItem.SortOrder + 1,
            WbsLevel = SelectedItem.WbsLevel,
            Weight = SelectedItem.Weight,
            PlannedStart = SelectedItem.PlannedStart,
            PlannedFinish = SelectedItem.PlannedFinish,
            DurationDays = SelectedItem.DurationDays,
            AssignedTo = SelectedItem.AssignedTo,
            Owner = SelectedItem.Owner,
            Discipline = SelectedItem.Discipline,
            Deliverable = SelectedItem.Deliverable,
            Notes = SelectedItem.Notes,
            IsActive = SelectedItem.IsActive,
        };
        await _itemRepository.AddAsync(duplicate, ct);
        await _wbsService.RedistributeWeightsAsync(WbsId, ct);
        await AutoRegenerateCodesAsync(ct);
        await LoadItemsAsync(ct);
    }

    [RelayCommand]
    private async Task RenameNodeAsync(CancellationToken ct)
    {
        if (SelectedItem != null)
        {
            OnPropertyChanged(nameof(SelectedItem));
        }
    }

    [RelayCommand]
    private async Task IndentNodeAsync(CancellationToken ct)
    {
        if (SelectedItem == null) return;
        var allItems = await _itemRepository.GetByWbsIdAsync(WbsId, ct);
        var siblings = allItems
            .Where(i => i.ParentId == SelectedItem.ParentId)
            .OrderBy(i => i.SortOrder)
            .ToList();
        var idx = siblings.FindIndex(i => i.Id == SelectedItem.Id);
        if (idx <= 0) return;

        var previousSibling = siblings[idx - 1];
        SelectedItem.ParentId = previousSibling.Id;
        SelectedItem.Level = previousSibling.Level + 1;
        SelectedItem.SortOrder = allItems
            .Where(i => i.ParentId == previousSibling.Id)
            .DefaultIfEmpty()
            .Max(i => i?.SortOrder ?? 0) + 1;

        await _itemRepository.UpdateRangeAsync(new[] { SelectedItem }, ct);
        await _wbsService.RedistributeWeightsAsync(WbsId, ct);
        await AutoRegenerateCodesAsync(ct);
        await LoadItemsAsync(ct);
    }

    [RelayCommand]
    private async Task OutdentNodeAsync(CancellationToken ct)
    {
        if (SelectedItem?.ParentId == null) return;

        var allItems = await _itemRepository.GetByWbsIdAsync(WbsId, ct);
        var parent = allItems.FirstOrDefault(i => i.Id == SelectedItem.ParentId);
        if (parent == null) return;

        SelectedItem.ParentId = parent.ParentId;
        SelectedItem.Level = parent.Level;
        SelectedItem.SortOrder = parent.SortOrder + 1;

        await _itemRepository.UpdateRangeAsync(new[] { SelectedItem }, ct);
        await _wbsService.RedistributeWeightsAsync(WbsId, ct);
        await AutoRegenerateCodesAsync(ct);
        await LoadItemsAsync(ct);
    }

    [RelayCommand]
    private async Task RegenerateCodesAsync(CancellationToken ct)
    {
        await _codeGenerationService.RegenerateCodesAsync(WbsId, ct);
        await LoadItemsAsync(ct);
    }

    [RelayCommand]
    private async Task HandleDropAsync(CancellationToken ct)
    {
        if (DragDropItemId == null || SelectedItem == null) return;

        var allItems = await _itemRepository.GetByWbsIdAsync(WbsId, ct);
        var draggedItem = allItems.FirstOrDefault(i => i.Id == DragDropItemId.Value);
        if (draggedItem == null) return;

        draggedItem.ParentId = SelectedItem.ParentId;
        draggedItem.Level = SelectedItem.Level;
        draggedItem.SortOrder = SelectedItem.SortOrder;

        await _itemRepository.UpdateRangeAsync(new[] { draggedItem }, ct);
        await _wbsService.RedistributeWeightsAsync(WbsId, ct);
        await AutoRegenerateCodesAsync(ct);
        await LoadItemsAsync(ct);

        DragDropItemId = null;
    }

    private async Task AutoRegenerateCodesAsync(CancellationToken ct)
    {
        if (_autoGenerateCodes)
        {
            await _codeGenerationService.RegenerateCodesAsync(WbsId, ct);
        }
    }

    public static void GenerateWbsCodes(IReadOnlyList<WbsItem> items)
    {
        var roots = items.Where(i => i.ParentId is null)
                         .OrderBy(i => i.SortOrder)
                         .ToList();

        for (var i = 0; i < roots.Count; i++)
        {
            AssignCode(roots[i], items, (i + 1).ToString());
        }
    }

    private static void AssignCode(WbsItem item, IReadOnlyList<WbsItem> allItems, string code)
    {
        item.Code = code;
        var children = allItems.Where(i => i.ParentId == item.Id)
                               .OrderBy(i => i.SortOrder)
                               .ToList();
        for (var i = 0; i < children.Count; i++)
        {
            AssignCode(children[i], allItems, $"{code}.{i + 1}");
        }
    }

    private static List<Guid> CollectDescendantIds(Guid parentId, IReadOnlyList<WbsItem> allItems)
    {
        var ids = new List<Guid>();
        var children = allItems.Where(i => i.ParentId == parentId).ToList();
        foreach (var child in children)
        {
            ids.Add(child.Id);
            ids.AddRange(CollectDescendantIds(child.Id, allItems));
        }
        return ids;
    }

    private async Task LoadItemsAsync(CancellationToken ct)
    {
        var items = await _itemRepository.GetByWbsIdAsync(WbsId, ct);
        Items.Clear();
        foreach (var item in items)
            Items.Add(item);
    }
}
