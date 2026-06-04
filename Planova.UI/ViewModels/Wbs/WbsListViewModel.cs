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

public sealed partial class WbsListViewModel : ObservableObject
{
    private readonly IWbsService _wbsService;
    private readonly IWbsItemRepository _itemRepository;
    private readonly IBoqSession _session;

    public WbsListViewModel(IWbsService wbsService, IWbsItemRepository itemRepository, IBoqSession session)
    {
        _wbsService = wbsService;
        _itemRepository = itemRepository;
        _session = session;
    }

    public ObservableCollection<WbsEntity> WbsItems { get; } = new();

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private WbsStatus? _statusFilter;

    [ObservableProperty]
    private WbsSource? _sourceFilter;

    [ObservableProperty]
    private WbsEntity? _selectedWbs;

    [ObservableProperty]
    private string _newWbsName = "New WBS";

    [RelayCommand]
    private async Task LoadWbsListAsync(CancellationToken ct)
    {
        var projectId = _session.CurrentProjectId;
        if (projectId is null) return;
        var pId = projectId.Value.GetHashCode();
        var items = await _wbsService.GetByProjectAsync(pId, ct);
        WbsItems.Clear();
        foreach (var item in items)
        {
            var allItems = await _itemRepository.GetByWbsIdAsync(item.Id, ct);
            item.ItemCount = allItems.Count;
            WbsItems.Add(item);
        }
    }

    [RelayCommand]
    private async Task CreateWbsAsync(CancellationToken ct)
    {
        var projectId = _session.CurrentProjectId;
        if (projectId is null) return;
        var pId = projectId.Value.GetHashCode();
        var name = string.IsNullOrWhiteSpace(NewWbsName) ? "New WBS" : NewWbsName;
        await _wbsService.CreateAsync(name, pId, WbsSource.Manual, null, ct);
        NewWbsName = "New WBS";
        await LoadWbsListAsync(ct);
        WeakReferenceMessenger.Default.Send(new WbsStudioMessage(WbsChangeType.Created));
    }

    [RelayCommand]
    private async Task DeleteWbsAsync(CancellationToken ct)
    {
        if (SelectedWbs is null) return;
        await _wbsService.DeleteAsync(SelectedWbs.Id, ct);
        await LoadWbsListAsync(ct);
        WeakReferenceMessenger.Default.Send(new WbsStudioMessage(WbsChangeType.Deleted));
    }

    [RelayCommand]
    private async Task RenameWbsAsync(CancellationToken ct)
    {
        if (SelectedWbs is null) return;
        await _wbsService.UpdateAsync(SelectedWbs, ct);
        await LoadWbsListAsync(ct);
        WeakReferenceMessenger.Default.Send(new WbsStudioMessage(WbsChangeType.Updated));
    }
}
