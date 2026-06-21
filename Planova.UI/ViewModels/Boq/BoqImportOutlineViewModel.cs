using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Planova.Boq.Application.Dto;
using Planova.Boq.Domain.Enums;
using Planova.Boq.Domain.Interfaces;
using Planova.Boq.Application.Services;
using Planova.Shared.Abstractions;
using Planova.Application.Services;

namespace Planova.UI.ViewModels.Boq;

public sealed partial class BoqImportOutlineItem : ObservableObject
{
    public int SortOrder { get; }
    public string Code { get; }
    public string Description { get; }
    public string? Classification { get; }
    public string? Division { get; }
    public string Unit { get; }
    public decimal Quantity { get; }
    public decimal Rate { get; }
    public decimal Amount { get; }
    public ItemType ItemType { get; }
    public int Level { get; }
    public string? SourceSheet { get; }
    public Guid? ParentId { get; }

    [ObservableProperty]
    private bool _isExpanded = true;

    public ObservableCollection<BoqImportOutlineItem> Children { get; } = new();

    public BoqImportOutlineItem(int sortOrder, string code, string description,
        string? classification, string? division, string unit,
        decimal quantity, decimal rate, decimal amount,
        ItemType itemType, int level, string? sourceSheet, Guid? parentId)
    {
        SortOrder = sortOrder;
        Code = code;
        Description = description;
        Classification = classification;
        Division = division;
        Unit = unit;
        Quantity = quantity;
        Rate = rate;
        Amount = amount;
        ItemType = itemType;
        Level = level;
        SourceSheet = sourceSheet;
        ParentId = parentId;
    }

    public string IndentedDescription => $"{new string(' ', Level * 2)}{Description}";
    public string ItemTypeIcon => ItemType == ItemType.Section ? "Folder24" : "Document24";
    public bool IsSection => ItemType == ItemType.Section;
    public string IndentedMargin => $"{Level * 20},0,0,0";
    public string DisplayAmount => Amount > 0 ? $"{Amount:N2}" : "—";
}

public sealed partial class BoqImportOutlineViewModel : ObservableObject
{
    private readonly IBoqImportService _importService;
    private readonly IMultiSheetBoqImportService _multiSheetImportService;
    private readonly IBoqDescriptionParser _descriptionParser;
    private readonly IBoqSession _session;
    private readonly ICurrentProjectService _currentProjectService;
    private readonly IBoqColumnMappingService _columnMappingService;

    public BoqImportOutlineViewModel(
        IBoqImportService importService,
        IMultiSheetBoqImportService multiSheetImportService,
        IBoqDescriptionParser descriptionParser,
        IBoqSession session,
        ICurrentProjectService currentProjectService,
        IBoqColumnMappingService columnMappingService)
    {
        _importService = importService;
        _multiSheetImportService = multiSheetImportService;
        _descriptionParser = descriptionParser;
        _session = session;
        _currentProjectService = currentProjectService;
        _columnMappingService = columnMappingService;
    }

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    [ObservableProperty]
    private int _totalSections;

    [ObservableProperty]
    private int _totalItems;

    [ObservableProperty]
    private int _totalRows;

    [ObservableProperty]
    private decimal _grandTotal;

    [ObservableProperty]
    private int _sheetsDetected;

    [ObservableProperty]
    private int _boqSheetsDetected;

    [ObservableProperty]
    private int _nonBoqSheetsSkipped;

    [ObservableProperty]
    private string _filePath = string.Empty;

    [ObservableProperty]
    private ImportMode _selectedImportMode = ImportMode.Merge;

    [ObservableProperty]
    private bool _hasPreview;

    [ObservableProperty]
    private bool _hasError;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private BoqImportOutlineItem? _selectedItem;

    [ObservableProperty]
    private bool _showChoicePanel = true;

    public ObservableCollection<BoqImportOutlineItem> OutlineItems { get; } = new();
    public ObservableCollection<WorksheetInfo> WorksheetInfos { get; } = new();
    public ObservableCollection<WorksheetSelection> WorksheetSelections { get; } = new();

    public ObservableCollection<ImportMode> ImportModes { get; } = new()
    {
        ImportMode.Single, ImportMode.Multiple, ImportMode.Merge
    };

    public string ImportModeLabel => SelectedImportMode switch
    {
        ImportMode.Single => "Import selected sheet only",
        ImportMode.Multiple => "Import each sheet as separate BOQ",
        ImportMode.Merge => "Merge all BOQ sheets into one",
        _ => "Merge all BOQ sheets into one"
    };

    partial void OnSelectedImportModeChanged(ImportMode value)
    {
        OnPropertyChanged(nameof(ImportModeLabel));
        if (HasPreview)
            _ = LoadOutlineAsync(CancellationToken.None);
    }

    [RelayCommand]
    private void UseSelected()
    {
        ShowChoicePanel = false;
    }

    [RelayCommand]
    private void ImportNew()
    {
        ShowChoicePanel = false;
        if (!string.IsNullOrEmpty(FilePath))
            _ = LoadOutlineAsync(CancellationToken.None);
    }

    [RelayCommand]
    private async Task ScanFileAsync(string filePath, CancellationToken ct)
    {
        if (string.IsNullOrEmpty(filePath)) return;

        IsLoading = true;
        HasError = false;
        FilePath = filePath;
        ShowChoicePanel = true;

        try
        {
            var scanned = await _multiSheetImportService.ScanAsync(filePath, ct);
            WorksheetInfos.Clear();
            WorksheetSelections.Clear();

            foreach (var ws in scanned)
            {
                WorksheetInfos.Add(ws);
                WorksheetSelections.Add(new WorksheetSelection(
                    ws.Index,
                    ws.IsBoqSheet ? ImportAction.Import : ImportAction.Skip,
                    null));
            }

            SheetsDetected = scanned.Count;
            BoqSheetsDetected = scanned.Count(s => s.IsBoqSheet);
            NonBoqSheetsSkipped = scanned.Count(s => !s.IsBoqSheet);
            StatusMessage = $"{scanned.Count} sheets detected ({BoqSheetsDetected} BOQ, {NonBoqSheetsSkipped} non-BOQ skipped)";
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Scan failed: {ex.Message}";
            HasError = true;
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task LoadOutlineAsync(CancellationToken ct)
    {
        if (string.IsNullOrEmpty(FilePath)) return;

        IsLoading = true;
        HasError = false;
        HasPreview = false;

        try
        {
            var scanned = await _multiSheetImportService.ScanAsync(FilePath, ct);
            var boqSelections = WorksheetSelections
                .Where(s => s.Action == ImportAction.Import)
                .ToList();

            if (boqSelections.Count == 0)
            {
                StatusMessage = "No BOQ sheets selected for import";
                return;
            }

            var projectId = _session.CurrentProjectId ?? (_currentProjectService.CurrentProject is not null
                ? GuidFromInt(_currentProjectService.CurrentProject.Id) : Guid.Empty);

            var preview = await _multiSheetImportService.PreviewAsync(
                FilePath, projectId, SelectedImportMode, boqSelections, ct);

            OutlineItems.Clear();
            TotalSections = preview.WorksheetsToImport;
            TotalRows = preview.EstimatedRows;

            foreach (var wsPreview in preview.Previews)
            {
                var scan = scanned.FirstOrDefault(s => s.Index == wsPreview.WorksheetIndex);
                var sheetName = scan?.Name ?? $"Sheet{wsPreview.WorksheetIndex + 1}";

                var mapping = await _columnMappingService.DetectMappingAsync(FilePath, wsPreview.WorksheetIndex, ct);

                var sectionItem = new BoqImportOutlineItem(
                    wsPreview.WorksheetIndex + 1,
                    sheetName,
                    $"Sheet: {sheetName} ({wsPreview.RowCount} rows)",
                    null, null, "LS", 1, wsPreview.RowCount, wsPreview.RowCount,
                    ItemType.Section, 0, sheetName, null);

                OutlineItems.Add(sectionItem);
            }

            TotalItems = OutlineItems.Count(i => !i.IsSection);
            GrandTotal = OutlineItems.Sum(i => i.Amount);
            HasPreview = true;
            StatusMessage = $"Outline: {TotalSections} sections, {TotalRows} estimated rows";
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Outline preview failed: {ex.Message}";
            HasError = true;
        }
        finally
        {
            IsLoading = false;
        }
    }

    private static Guid GuidFromInt(int value)
    {
        var bytes = new byte[16];
        BitConverter.GetBytes(value).CopyTo(bytes, 0);
        return new Guid(bytes);
    }
}
