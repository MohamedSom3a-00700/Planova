using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.Json;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Win32;
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

    [RelayCommand]
    private async Task DuplicateWbsAsync(CancellationToken ct)
    {
        if (SelectedWbs is null) return;
        var projectId = _session.CurrentProjectId;
        if (projectId is null) return;
        var pId = projectId.Value.GetHashCode();
        var copyName = $"{SelectedWbs.Name} (Copy)";
        await _wbsService.CreateAsync(copyName, pId, SelectedWbs.Source, SelectedWbs.SourceBoqId, ct);
        await LoadWbsListAsync(ct);
        WeakReferenceMessenger.Default.Send(new WbsStudioMessage(WbsChangeType.Created));
    }

    [RelayCommand]
    private async Task ArchiveWbsAsync(CancellationToken ct)
    {
        if (SelectedWbs is null) return;
        await _wbsService.ChangeStatusAsync(SelectedWbs.Id, WbsStatus.Archived, ct);
        await LoadWbsListAsync(ct);
        WeakReferenceMessenger.Default.Send(new WbsStudioMessage(WbsChangeType.Updated));
    }

    [RelayCommand]
    private async Task ExportWbsAsync(string format, CancellationToken ct)
    {
        if (SelectedWbs is null) return;

        var dialog = new SaveFileDialog
        {
            FileName = $"{SelectedWbs.Name}_Export"
        };

        switch (format)
        {
            case "Excel":
                dialog.Filter = "Excel Files|*.xlsx|All Files|*.*";
                dialog.FileName += ".xlsx";
                break;
            case "PDF":
                dialog.Filter = "PDF Files|*.pdf|All Files|*.*";
                dialog.FileName += ".pdf";
                break;
            case "CSV":
                dialog.Filter = "CSV Files|*.csv|All Files|*.*";
                dialog.FileName += ".csv";
                break;
            case "JSON":
                dialog.Filter = "JSON Files|*.json|All Files|*.*";
                dialog.FileName += ".json";
                break;
            default:
                return;
        }

        if (dialog.ShowDialog() != true) return;

        var wbs = SelectedWbs;
        var items = await _itemRepository.GetByWbsIdAsync(wbs.Id, ct);

        switch (format)
        {
            case "Excel":
                await ExportToExcelAsync(wbs, items, dialog.FileName, ct);
                break;
            case "PDF":
                await ExportToPdfAsync(wbs, items, dialog.FileName, ct);
                break;
            case "CSV":
                await ExportToCsvAsync(wbs, items, dialog.FileName, ct);
                break;
            case "JSON":
                await ExportToJsonAsync(wbs, items, dialog.FileName, ct);
                break;
        }
    }

    private static async Task ExportToExcelAsync(WbsEntity wbs, IReadOnlyList<WbsItem> items, string filePath, CancellationToken ct)
    {
        using var stream = new MemoryStream();
        using var writer = new StreamWriter(stream);

        writer.WriteLine($"WBS Export: {wbs.Name}");
        writer.WriteLine($"Status: {wbs.Status}  Revision: {wbs.Revision}  Total Weight: {wbs.TotalWeight:F2}");
        writer.WriteLine();
        writer.WriteLine("Code,Name,Level,Weight,Assigned To");
        foreach (var item in items.OrderBy(i => i.Level).ThenBy(i => i.SortOrder))
            writer.WriteLine($"{item.Code},{item.Name},{item.Level},{item.Weight},{item.AssignedTo}");

        writer.Flush();
        await File.WriteAllBytesAsync(filePath, stream.ToArray(), ct);
    }

    private static async Task ExportToPdfAsync(WbsEntity wbs, IReadOnlyList<WbsItem> items, string filePath, CancellationToken ct)
    {
        using var stream = new MemoryStream();
        using var writer = new StreamWriter(stream);

        writer.WriteLine("=== WBS Export ===");
        writer.WriteLine($"WBS: {wbs.Name}");
        writer.WriteLine($"Status: {wbs.Status}  Revision: {wbs.Revision}  Total Weight: {wbs.TotalWeight:F2}");
        writer.WriteLine(new string('-', 60));
        writer.WriteLine($"{"Code",-12} {"Name",-25} {"Level",-6} {"Weight",-8} {"Assigned To",-15}");
        writer.WriteLine(new string('-', 60));
        foreach (var item in items.OrderBy(i => i.Level).ThenBy(i => i.SortOrder))
        {
            var name = (item.Name ?? "").Length > 24 ? item.Name[..23] + "..." : item.Name ?? "";
            writer.WriteLine($"{item.Code,-12} {name,-25} {item.Level,-6} {item.Weight,-8:F1} {item.AssignedTo,-15}");
        }

        writer.Flush();
        await File.WriteAllBytesAsync(filePath, stream.ToArray(), ct);
    }

    private static async Task ExportToCsvAsync(WbsEntity wbs, IReadOnlyList<WbsItem> items, string filePath, CancellationToken ct)
    {
        var csv = new StringBuilder();
        csv.AppendLine("Code,Name,Level,Weight,AssignedTo,Description,Discipline");
        foreach (var item in items.OrderBy(i => i.Level).ThenBy(i => i.SortOrder))
        {
            var name = (item.Name ?? "").Contains(',') ? $"\"{item.Name}\"" : item.Name ?? "";
            var desc = (item.Description ?? "").Contains(',') ? $"\"{item.Description}\"" : item.Description ?? "";
            csv.AppendLine(CultureInfo.InvariantCulture,
                $"{item.Code},{name},{item.Level},{item.Weight},{item.AssignedTo},{desc},{item.Discipline}");
        }
        await File.WriteAllTextAsync(filePath, csv.ToString(), ct);
    }

    private static async Task ExportToJsonAsync(WbsEntity wbs, IReadOnlyList<WbsItem> items, string filePath, CancellationToken ct)
    {
        var exportData = new
        {
            wbs.Id,
            wbs.Name,
            wbs.Description,
            Status = wbs.Status.ToString(),
            wbs.Revision,
            Source = wbs.Source.ToString(),
            wbs.TotalWeight,
            wbs.CreatedAt,
            wbs.UpdatedAt,
            wbs.CreatedBy,
            Items = items.OrderBy(i => i.Level).ThenBy(i => i.SortOrder).Select(i => new
            {
                i.Id,
                i.Code,
                i.Name,
                i.Description,
                i.Level,
                i.Weight,
                i.AssignedTo,
                i.Owner,
                i.Discipline,
                i.Deliverable,
                i.DurationDays,
                i.PlannedStart,
                i.PlannedFinish,
                i.Notes
            })
        };

        var json = JsonSerializer.Serialize(exportData, new JsonSerializerOptions
        {
            WriteIndented = true
        });

        await File.WriteAllTextAsync(filePath, json, ct);
    }
}
