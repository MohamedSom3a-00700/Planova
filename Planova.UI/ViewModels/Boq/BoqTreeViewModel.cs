using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Planova.Boq.Application.Dto;
using Planova.Boq.Domain.Interfaces;

namespace Planova.UI.ViewModels.Boq;

public partial class BoqTreeViewModel : ObservableObject
{
    private readonly IBoqService _boqService;
    private readonly IBoqSession _session;

    public BoqTreeViewModel(IBoqService boqService, IBoqSession session)
    {
        _boqService = boqService;
        _session = session;
        _session.BoqChanged += OnBoqChanged;
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
    private BoqItemDto? _selectedItem;

    public ObservableCollection<BoqSummaryDto> AvailableBoqs { get; } = new();
    public ObservableCollection<BoqItemDto> FlatItems { get; } = new();

    [RelayCommand]
    private async Task LoadBoqsAsync(Guid projectId, CancellationToken ct)
    {
        try
        {
            IsLoading = true;
            var boqs = await _boqService.GetByProjectIdAsync(projectId, ct);
            AvailableBoqs.Clear();
            foreach (var boq in boqs)
            {
                AvailableBoqs.Add(boq);
            }
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task LoadTreeAsync(Guid boqId, CancellationToken ct)
    {
        try
        {
            IsLoading = true;
            CurrentBoqId = boqId;

            var boq = await _boqService.GetByIdAsync(boqId, ct);
            BoqName = boq.Name;

            var tree = await _boqService.GetTreeAsync(boqId, ct);
            FlatItems.Clear();
            var flatList = FlattenTree(tree).ToList();
            for (int i = 0; i < flatList.Count; i++)
            {
                FlatItems.Add(flatList[i] with { SortOrder = i + 1 });
            }

            GrandTotal = await _boqService.ComputeSubtotalAsync(boqId, null, ct);
            StatusMessage = $"Loaded {FlatItems.Count} items";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void ToggleExpand(BoqItemDto item)
    {
    }

    [RelayCommand]
    private async Task RefreshAsync(CancellationToken ct)
    {
        if (CurrentBoqId != Guid.Empty)
        {
            await LoadTreeAsync(CurrentBoqId, ct);
        }
    }

    private async void OnBoqChanged(object? sender, Guid boqId)
    {
        await LoadTreeAsync(boqId, CancellationToken.None);
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
