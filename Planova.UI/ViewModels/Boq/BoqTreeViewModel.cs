using System.Collections.ObjectModel;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Planova.Application.Services;
using Planova.Boq.Application.Dto;
using Planova.Boq.Domain.Enums;
using Planova.Boq.Domain.Interfaces;
using Planova.Shared.Abstractions;

namespace Planova.UI.ViewModels.Boq;

public partial class BoqOutlineItem : ObservableObject
{
    public int SortOrder { get; }
    public string Code { get; }
    public string Description { get; }
    public string Unit { get; }
    public decimal Quantity { get; }
    public decimal Rate { get; }
    public decimal Amount { get; }
    public ItemType ItemType { get; }
    public int Level { get; }
    public string? CostCode { get; }
    public bool IsActive { get; }
    public Guid Id { get; }
    public Guid BoqId { get; }
    public Guid? ParentId { get; }
    public decimal? Subtotal { get; }

    [ObservableProperty]
    private bool _isExpanded = true;

    public ObservableCollection<BoqOutlineItem> Children { get; } = new();

    public BoqOutlineItem(BoqItemDto dto)
    {
        Id = dto.Id;
        BoqId = dto.BoqId;
        ParentId = dto.ParentId;
        SortOrder = dto.SortOrder;
        Code = dto.Code;
        Description = dto.Description;
        Unit = dto.Unit;
        Quantity = dto.Quantity;
        Rate = dto.Rate;
        Amount = dto.Amount;
        ItemType = dto.ItemType;
        Level = dto.Level;
        CostCode = dto.CostCode;
        IsActive = dto.IsActive;
        Subtotal = dto.Subtotal;
    }

    public bool IsSection => ItemType == ItemType.Section;
    public string ItemTypeIcon => IsSection ? "Folder24" : "Document24";
    public string IndentedMargin => $"{Level * 20},0,0,0";
    public string DisplayCode => IsSection ? Code : $"  {Code}";
    public string DisplayAmount => Subtotal.HasValue && IsSection
        ? $"{Subtotal.Value:N2}"
        : $"{Amount:N2}";
}

public partial class BoqTreeViewModel : ObservableObject
{
    private readonly IBoqService _boqService;
    private readonly IBoqSession _session;
    private readonly ICurrentProjectService _currentProjectService;
    private readonly IProjectDocumentService _projectDocumentService;

    public BoqTreeViewModel(IBoqService boqService, IBoqSession session,
        ICurrentProjectService currentProjectService,
        IProjectDocumentService projectDocumentService)
    {
        _boqService = boqService;
        _session = session;
        _currentProjectService = currentProjectService;
        _projectDocumentService = projectDocumentService;
        _session.BoqChanged += OnBoqChanged;
        _currentProjectService.CurrentProjectChanged += OnCurrentProjectChanged;

        if (_currentProjectService.CurrentProject is not null)
        {
            _ = LoadAvailableBoqsAsync(CancellationToken.None);
        }
    }

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    [ObservableProperty]
    private string _boqName = string.Empty;

    [ObservableProperty]
    private decimal _grandTotal;

    [ObservableProperty]
    private Guid _currentBoqId;

    [ObservableProperty]
    private BoqOutlineItem? _selectedOutlineItem;

    [ObservableProperty]
    private BoqSummaryDto? _selectedBoq;

    [ObservableProperty]
    private bool _isDeleteConfirmVisible;

    [ObservableProperty]
    private int _totalSections;

    [ObservableProperty]
    private int _totalItems;

    public ObservableCollection<BoqSummaryDto> AvailableBoqs { get; } = new();
    public ObservableCollection<BoqOutlineItem> OutlineItems { get; } = new();
    public ObservableCollection<BoqItemDto> FlatItems { get; } = new();

    public bool HasNoBoqs => AvailableBoqs.Count == 0;
    public bool HasBoqSelected => SelectedBoq is not null;

    partial void OnSelectedBoqChanged(BoqSummaryDto? value)
    {
        OnPropertyChanged(nameof(HasBoqSelected));
        if (value is not null)
        {
            _ = LoadOutlineAsync(value.Id, CancellationToken.None);
        }
        else
        {
            OutlineItems.Clear();
            FlatItems.Clear();
            BoqName = string.Empty;
            GrandTotal = 0;
        }
    }

    partial void OnSelectedOutlineItemChanged(BoqOutlineItem? value)
    {
        if (value is not null)
        {
            StatusMessage = $"Selected: {value.Code} — {value.Description}";
        }
    }

    private async void OnCurrentProjectChanged(object? sender, ProjectContext? context)
    {
        if (context is not null)
        {
            try
            {
                await LoadAvailableBoqsAsync(CancellationToken.None);
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error loading BOQs: {ex.Message}";
            }
        }
        else
        {
            ResetState();
            AvailableBoqs.Clear();
            OnPropertyChanged(nameof(HasNoBoqs));
            StatusMessage = string.Empty;
        }
    }

    [RelayCommand]
    private async Task LoadAvailableBoqsAsync(CancellationToken ct)
    {
        var projectId = _session.CurrentProjectId;
        if (projectId is null && _currentProjectService.CurrentProject is not null)
        {
            projectId = GuidFromInt(_currentProjectService.CurrentProject.Id);
        }
        if (projectId is null)
        {
            ResetState();
            AvailableBoqs.Clear();
            OnPropertyChanged(nameof(HasNoBoqs));
            StatusMessage = string.Empty;
            return;
        }

        try
        {
            IsLoading = true;
            var boqs = await _boqService.GetByProjectIdAsync(projectId.Value, ct);
            AvailableBoqs.Clear();
            foreach (var boq in boqs)
            {
                AvailableBoqs.Add(boq);
            }
            OnPropertyChanged(nameof(HasNoBoqs));

            if (boqs.Count > 0 && SelectedBoq is null)
            {
                SelectedBoq = boqs[0];
                StatusMessage = $"Loaded {boqs.Count} BOQ(s)";
            }
            else if (boqs.Count == 0)
            {
                ResetState();
                var projectIntId = _currentProjectService.CurrentProject?.Id;
                if (projectIntId.HasValue)
                {
                    var boqDocs = await _projectDocumentService.GetByTypeAsync(projectIntId.Value, "Boq", ct);
                    if (boqDocs.Any())
                    {
                        var firstDoc = boqDocs.OrderBy(d => d.FileName).First();
                        var fileExists = !string.IsNullOrEmpty(firstDoc.AbsolutePath) && File.Exists(firstDoc.AbsolutePath);
                        StatusMessage = fileExists
                            ? $"No BOQs imported yet. Document '{firstDoc.FileName}' is available — go to Import tab."
                            : "BOQ document found but file is missing — re-upload the document.";
                    }
                    else
                    {
                        StatusMessage = "No BOQs found. Add a BOQ document in Projects, then import from the Import tab.";
                    }
                }
                else
                {
                    StatusMessage = string.Empty;
                }
            }
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void ResetState()
    {
        SelectedBoq = null;
        CurrentBoqId = Guid.Empty;
        OutlineItems.Clear();
        FlatItems.Clear();
        BoqName = string.Empty;
        GrandTotal = 0;
        TotalSections = 0;
        TotalItems = 0;
        SelectedOutlineItem = null;
    }

    private static Guid GuidFromInt(int value)
    {
        var bytes = new byte[16];
        BitConverter.GetBytes(value).CopyTo(bytes, 0);
        return new Guid(bytes);
    }

    [RelayCommand]
    private async Task LoadOutlineAsync(Guid boqId, CancellationToken ct)
    {
        try
        {
            IsLoading = true;
            CurrentBoqId = boqId;

            var boq = await _boqService.GetByIdAsync(boqId, ct);
            BoqName = boq.Name;

            var tree = await _boqService.GetTreeAsync(boqId, ct);
            OutlineItems.Clear();
            FlatItems.Clear();
            TotalSections = 0;
            TotalItems = 0;

            BuildOutlineItems(tree, OutlineItems, null);

            var flatList = FlattenTree(tree).ToList();
            for (int i = 0; i < flatList.Count; i++)
            {
                FlatItems.Add(flatList[i] with { SortOrder = i + 1 });
            }

            GrandTotal = await _boqService.ComputeSubtotalAsync(boqId, null, ct);
            StatusMessage = $"Loaded {OutlineItems.Count} items";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void BuildOutlineItems(IReadOnlyList<BoqItemDto> items, ObservableCollection<BoqOutlineItem> target, BoqOutlineItem? parent)
    {
        foreach (var dto in items)
        {
            var outlineItem = new BoqOutlineItem(dto);
            if (dto.Children != null && dto.Children.Count > 0)
            {
                BuildOutlineItems(dto.Children, outlineItem.Children, outlineItem);
            }

            if (dto.ItemType == ItemType.Section)
                TotalSections++;
            else
                TotalItems++;

            target.Add(outlineItem);
        }
    }

    [RelayCommand]
    private async Task RefreshAsync(CancellationToken ct)
    {
        if (CurrentBoqId != Guid.Empty)
        {
            await LoadOutlineAsync(CurrentBoqId, ct);
        }
    }

    [RelayCommand]
    private void ToggleExpand(BoqOutlineItem item)
    {
        item.IsExpanded = !item.IsExpanded;
    }

    [RelayCommand]
    private void ExpandAll()
    {
        SetExpandAll(OutlineItems, true);
    }

    [RelayCommand]
    private void CollapseAll()
    {
        SetExpandAll(OutlineItems, false);
    }

    private static void SetExpandAll(ObservableCollection<BoqOutlineItem> items, bool expanded)
    {
        foreach (var item in items)
        {
            item.IsExpanded = expanded;
            SetExpandAll(item.Children, expanded);
        }
    }

    [RelayCommand]
    private void ShowDeleteConfirm()
    {
        IsDeleteConfirmVisible = true;
    }

    [RelayCommand]
    private void HideDeleteConfirm()
    {
        IsDeleteConfirmVisible = false;
    }

    [RelayCommand]
    private async Task DeleteBoqAsync(CancellationToken ct)
    {
        if (SelectedBoq is null) return;

        try
        {
            IsLoading = true;
            await _boqService.DeleteAsync(SelectedBoq.Id, ct);
            IsDeleteConfirmVisible = false;
            StatusMessage = $"BOQ '{SelectedBoq.Name}' deleted successfully";
            SelectedBoq = null;
            await LoadAvailableBoqsAsync(ct);
        }
        catch (Exception ex)
        {
            StatusMessage = $"Delete failed: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task DeleteItemAsync(CancellationToken ct)
    {
        if (SelectedOutlineItem is null) return;

        try
        {
            await _boqService.DeleteItemAsync(CurrentBoqId, SelectedOutlineItem.Id, ct);
            StatusMessage = $"Item '{SelectedOutlineItem.Code}' deleted";
            SelectedOutlineItem = null;
            await LoadOutlineAsync(CurrentBoqId, ct);
        }
        catch (Exception ex)
        {
            StatusMessage = $"Delete item failed: {ex.Message}";
        }
    }

    private async void OnBoqChanged(object? sender, Guid boqId)
    {
        try
        {
            await LoadAvailableBoqsAsync(CancellationToken.None);

            var summary = AvailableBoqs.FirstOrDefault(b => b.Id == boqId);
            if (summary is not null)
            {
                SelectedBoq = summary;
            }
            else
            {
                await LoadOutlineAsync(boqId, CancellationToken.None);
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error loading BOQ: {ex.Message}";
        }
    }

    private static IEnumerable<BoqItemDto> FlattenTree(IReadOnlyList<BoqItemDto> items, int depth = 0)
    {
        foreach (var item in items)
        {
            yield return item with { Level = depth };
            if (item.Children != null)
            {
                foreach (var child in FlattenTree(item.Children, depth + 1))
                {
                    yield return child;
                }
            }
        }
    }
}
