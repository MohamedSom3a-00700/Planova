using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Planova.Boq.Application.Dto;
using Planova.Boq.Domain.Interfaces;

namespace Planova.UI.ViewModels.Boq;

public partial class BoqEditorViewModel : ObservableObject
{
    private readonly IBoqService _boqService;
    private readonly IBoqSession _session;

    public BoqEditorViewModel(IBoqService boqService, IBoqSession session)
    {
        _boqService = boqService;
        _session = session;
        _session.BoqChanged += OnBoqChanged;
    }

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private bool _hasBoq;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    [ObservableProperty]
    private Guid _currentBoqId;

    [ObservableProperty]
    private BoqItemDto? _selectedTreeItem;

    [ObservableProperty]
    private string _editCode = string.Empty;

    [ObservableProperty]
    private string _editDescription = string.Empty;

    [ObservableProperty]
    private string _editUnit = string.Empty;

    [ObservableProperty]
    private decimal _editQuantity;

    [ObservableProperty]
    private decimal _editRate;

    [ObservableProperty]
    private string _editCostCode = string.Empty;

    [ObservableProperty]
    private bool _editIsActive = true;

    [ObservableProperty]
    private int _itemSortOrder;

    [ObservableProperty]
    private string _itemTypeDisplay = string.Empty;

    [ObservableProperty]
    private int _itemLevel;

    public ObservableCollection<BoqItemDto> RootItems { get; } = new();
    public ObservableCollection<string> AvailableCodes { get; } = new();

    private async void OnBoqChanged(object? sender, Guid boqId)
    {
        await LoadBoqAsync(boqId);
    }

    private async Task LoadBoqAsync(Guid boqId)
    {
        try
        {
            IsLoading = true;
            CurrentBoqId = boqId;
            HasBoq = true;

            var tree = await _boqService.GetTreeAsync(boqId, CancellationToken.None);
            RootItems.Clear();
            AvailableCodes.Clear();
            var flatList = FlattenTree(tree).ToList();
            for (int i = 0; i < flatList.Count; i++)
            {
                RootItems.Add(flatList[i] with { SortOrder = i + 1 });
                AvailableCodes.Add(flatList[i].Code);
            }

            StatusMessage = $"Loaded {AvailableCodes.Count} items";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Load error: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void SelectItem(BoqItemDto? item)
    {
        if (item == null) return;
        SelectedTreeItem = item;
        EditCode = item.Code;
        EditDescription = item.Description;
        EditUnit = item.Unit;
        EditQuantity = item.Quantity;
        EditRate = item.Rate;
        EditCostCode = item.CostCode ?? string.Empty;
        EditIsActive = item.IsActive;
        ItemSortOrder = RootItems.IndexOf(item) + 1;
        ItemTypeDisplay = item.ItemType.ToString();
        ItemLevel = item.Level;
        StatusMessage = $"Selected: {item.Code}";
    }

    [RelayCommand]
    private void SelectCode(string? code)
    {
        if (string.IsNullOrEmpty(code)) return;
        var item = RootItems.FirstOrDefault(i => i.Code == code);
        if (item != null)
        {
            SelectItem(item);
        }
    }

    [RelayCommand]
    private async Task UpdateItemAsync(CancellationToken ct)
    {
        if (SelectedTreeItem == null || CurrentBoqId == Guid.Empty) return;
        try
        {
            IsLoading = true;
            var updated = await _boqService.UpdateItemAsync(
                CurrentBoqId, SelectedTreeItem.Id, EditCode, EditDescription,
                EditUnit, EditQuantity, EditRate, EditCostCode, EditIsActive, ct);
            StatusMessage = $"Updated: {updated.Code}";
            await LoadBoqAsync(CurrentBoqId);
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task AddItemAsync(CancellationToken ct)
    {
        if (CurrentBoqId == Guid.Empty) return;
        try
        {
            IsLoading = true;
            var parentId = SelectedTreeItem?.Id;
            var created = await _boqService.AddItemAsync(
                CurrentBoqId, parentId, EditCode, EditDescription,
                EditUnit, EditQuantity, EditRate, EditCostCode, ct);
            StatusMessage = $"Added: {created.Code}";
            await LoadBoqAsync(CurrentBoqId);
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task DeleteItemAsync(CancellationToken ct)
    {
        if (SelectedTreeItem == null || CurrentBoqId == Guid.Empty) return;
        try
        {
            IsLoading = true;
            await _boqService.DeleteItemAsync(CurrentBoqId, SelectedTreeItem.Id, ct);
            StatusMessage = "Deleted";
            SelectedTreeItem = null;
            await LoadBoqAsync(CurrentBoqId);
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private static IEnumerable<BoqItemDto> FlattenTree(IReadOnlyList<BoqItemDto> items)
    {
        foreach (var item in items)
        {
            yield return item;
            if (item.Children != null)
            {
                foreach (var child in FlattenTree(item.Children))
                {
                    yield return child;
                }
            }
        }
    }
}
