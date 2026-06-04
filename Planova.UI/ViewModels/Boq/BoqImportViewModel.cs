using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Planova.Boq.Application.Dto;
using Planova.Boq.Domain.Interfaces;
using System.IO;
using Planova.Boq.Application.Services;
using Planova.Boq.CsvReader;

namespace Planova.UI.ViewModels.Boq;

public partial class BoqImportViewModel : ObservableObject
{
    private readonly IBoqImportService _importService;
    private readonly IBoqCsvReader _csvReader;
    private readonly IExcelRowReader _excelReader;
    private readonly IBoqSession _session;

    public BoqImportViewModel(IBoqImportService importService, IBoqCsvReader csvReader,
        IExcelRowReader excelReader, IBoqSession session)
    {
        _importService = importService;
        _csvReader = csvReader;
        _excelReader = excelReader;
        _session = session;
    }

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private Guid _currentProjectId;

    [ObservableProperty]
    private string _selectedFilePath = string.Empty;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    [ObservableProperty]
    private int _progressValue;

    [ObservableProperty]
    private int _currentStep;

    [ObservableProperty]
    private CsvImportOptions? _importOptions;

    [ObservableProperty]
    private BoqImportPreview? _preview;

    [ObservableProperty]
    private bool _canProceed;

    [ObservableProperty]
    private string _codeColumn = "Code";

    [ObservableProperty]
    private string _descriptionColumn = "Description";

    [ObservableProperty]
    private string _unitColumn = "Unit";

    [ObservableProperty]
    private string _quantityColumn = "Quantity";

    [ObservableProperty]
    private string _rateColumn = "Rate";

    [ObservableProperty]
    private string _delimiter = ",";

    [ObservableProperty]
    private bool _hasHeaders = true;

    [ObservableProperty]
    private string _classificationColumn = string.Empty;

    [ObservableProperty]
    private string _divisionColumn = string.Empty;

    [ObservableProperty]
    private bool _isExcel;

    [ObservableProperty]
    private bool _hasMultipleSheets;

    [ObservableProperty]
    private bool _mergeAllSheets;

    [ObservableProperty]
    private string _selectedWorksheet = string.Empty;

    public ObservableCollection<string> DetectedColumns { get; } = new();
    public ObservableCollection<string> AvailableWorksheets { get; } = new();
    public ObservableCollection<ValidationIssue> ValidationErrors { get; } = new();
    public ObservableCollection<ValidationIssue> ValidationWarnings { get; } = new();

    private static readonly string[] CsiDivisionKeywords =
        ["division", "div", "csi", "masterformat", "master format", "section"];
    private static readonly string[] ClassificationKeywords =
        ["classification", "class", "category", "type", "group", "trade"];

    [RelayCommand]
    private async Task SelectFileAsync(CancellationToken ct)
    {
        try
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Excel Files (*.xlsx;*.xls)|*.xlsx;*.xls|CSV Files (*.csv)|*.csv|All Files (*.*)|*.*",
                Title = "Select BOQ File"
            };

            if (dialog.ShowDialog() == true)
            {
                SelectedFilePath = dialog.FileName;
                IsExcel = Path.GetExtension(SelectedFilePath).Equals(".xlsx", StringComparison.OrdinalIgnoreCase)
                       || Path.GetExtension(SelectedFilePath).Equals(".xls", StringComparison.OrdinalIgnoreCase);

                StatusMessage = $"Selected: {Path.GetFileName(SelectedFilePath)}";

                if (IsExcel)
                {
                    await DetectExcelWorksheetsAsync(ct);
                    CurrentStep = 2;
                }
                else
                {
                    await DetectCsvHeadersAsync(ct);
                    CurrentStep = 1;
                }

                CanProceed = true;
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"File error: {ex.Message}";
        }
    }

    private async Task DetectExcelWorksheetsAsync(CancellationToken ct)
    {
        try
        {
            IsLoading = true;
            var sheets = await _excelReader.GetWorksheetsAsync(SelectedFilePath, ct);
            AvailableWorksheets.Clear();
            foreach (var sheet in sheets)
                AvailableWorksheets.Add(sheet);

            HasMultipleSheets = sheets.Count > 1;
            SelectedWorksheet = sheets.FirstOrDefault() ?? string.Empty;
            MergeAllSheets = HasMultipleSheets;

            StatusMessage = HasMultipleSheets
                ? $"{sheets.Count} sheets detected — set to merge all or pick one"
                : $"Sheet: {SelectedWorksheet}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task DetectCsvHeadersAsync(CancellationToken ct)
    {
        try
        {
            IsLoading = true;
            var headers = await _csvReader.DetectHeadersAsync(SelectedFilePath, Delimiter, ct);
            DetectedColumns.Clear();
            foreach (var header in headers)
            {
                DetectedColumns.Add(header);
            }
            AutoDetectMappingColumns(headers);
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void AutoDetectMappingColumns(string[] headers)
    {
        foreach (var header in headers)
        {
            var lower = header.ToLowerInvariant().Trim();

            if (string.IsNullOrEmpty(CodeColumn) || CodeColumn == "Code")
            {
                if (MatchesAny(lower, ["code", "id", "ref", "reference", "item code", "itemcode",
                    "item no", "itemno", "item #", "item#", "number", "no"]))
                {
                    CodeColumn = header;
                }
            }

            if (string.IsNullOrEmpty(DescriptionColumn) || DescriptionColumn == "Description")
            {
                if (MatchesAny(lower, ["description", "desc", "item description", "itemdescription",
                    "name", "item name", "itemname", "title", "work item", "workitem",
                    "scope of work", "scopework", "scope"]))
                {
                    DescriptionColumn = header;
                }
            }

            if (string.IsNullOrEmpty(UnitColumn) || UnitColumn == "Unit")
            {
                if (MatchesAny(lower, ["unit", "uom", "unit of measure"]))
                    UnitColumn = header;
            }

            if (string.IsNullOrEmpty(QuantityColumn) || QuantityColumn == "Quantity")
            {
                if (MatchesAny(lower, ["quantity", "qty", "qty."]))
                    QuantityColumn = header;
            }

            if (string.IsNullOrEmpty(RateColumn) || RateColumn == "Rate")
            {
                if (MatchesAny(lower, ["rate", "unit rate", "unitrate", "unit price", "unitprice", "price"]))
                    RateColumn = header;
            }

            if (CsiDivisionKeywords.Any(k => lower.Contains(k)) && string.IsNullOrEmpty(DivisionColumn))
                DivisionColumn = header;

            if (ClassificationKeywords.Any(k => lower.Contains(k)) && string.IsNullOrEmpty(ClassificationColumn))
                ClassificationColumn = header;
        }
    }

    private static bool MatchesAny(string value, string[] candidates)
    {
        return candidates.Any(c => value == c || value.Contains(c));
    }

    [RelayCommand]
    private async Task PreviewImportAsync(CancellationToken ct)
    {
        try
        {
            if (string.IsNullOrEmpty(SelectedFilePath)) return;

            IsLoading = true;
            StatusMessage = "Previewing import...";
            ProgressValue = 0;

            var isCsv = Path.GetExtension(SelectedFilePath).Equals(".csv", StringComparison.OrdinalIgnoreCase);

            if (isCsv)
            {
                var options = new CsvImportOptions(
                    HasHeaders: HasHeaders,
                    CodeColumn: CodeColumn,
                    DescriptionColumn: DescriptionColumn,
                    UnitColumn: UnitColumn,
                    QuantityColumn: QuantityColumn,
                    RateColumn: RateColumn,
                    LevelColumn: null,
                    ParentIdColumn: null,
                    ClassificationColumn: string.IsNullOrEmpty(ClassificationColumn) ? null : ClassificationColumn,
                    DivisionColumn: string.IsNullOrEmpty(DivisionColumn) ? null : DivisionColumn,
                    Delimiter: Delimiter
                );

                var rows = await _csvReader.ReadAsync(SelectedFilePath, options, ct);
                var strategy = TreeBuildStrategy.LevelColumn;

                if (!rows.Any(r => r.Level.HasValue))
                {
                    strategy = rows.Any(r => !string.IsNullOrEmpty(r.ParentId))
                        ? TreeBuildStrategy.ParentIdColumn
                        : TreeBuildStrategy.CodePrefix;
                }

                Preview = await _importService.PreviewImportAsync(rows, strategy, ct);
            }

            if (Preview != null)
            {
                StatusMessage = $"Preview loaded: {Preview.TotalItems} items detected";
                CurrentStep = 2;
                CanProceed = true;
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Preview error: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
            ProgressValue = 100;
        }
    }

    [RelayCommand]
    private async Task CommitImportAsync(Guid projectId, CancellationToken ct)
    {
        try
        {
            if (string.IsNullOrEmpty(SelectedFilePath)) return;

            IsLoading = true;
            StatusMessage = "Importing...";

            var progress = new Progress<int>(value =>
            {
                ProgressValue = value;
            });

            var isCsv = Path.GetExtension(SelectedFilePath).Equals(".csv", StringComparison.OrdinalIgnoreCase);

            BoqImportResult? result;
            if (isCsv)
            {
                var options = new CsvImportOptions(
                    HasHeaders: HasHeaders,
                    CodeColumn: CodeColumn,
                    DescriptionColumn: DescriptionColumn,
                    UnitColumn: UnitColumn,
                    QuantityColumn: QuantityColumn,
                    RateColumn: RateColumn,
                    LevelColumn: null,
                    ParentIdColumn: null,
                    ClassificationColumn: string.IsNullOrEmpty(ClassificationColumn) ? null : ClassificationColumn,
                    DivisionColumn: string.IsNullOrEmpty(DivisionColumn) ? null : DivisionColumn,
                    Delimiter: Delimiter
                );

                result = await _importService.ImportFromCsvAsync(projectId, SelectedFilePath, options, progress, ct);
            }
            else
            {
                var sheetName = MergeAllSheets ? null : SelectedWorksheet;
                result = await _importService.ImportFromExcelAsync(projectId, SelectedFilePath, sheetName, null, progress, ct);
            }

            StatusMessage = $"Import complete: {result.ItemsImported} imported, {result.ItemsSkipped} skipped";
            CurrentStep = 3;
            _session.SelectBoq(result.BoqId, projectId);
        }
        catch (Exception ex)
        {
            StatusMessage = $"Import error: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void Reset()
    {
        SelectedFilePath = string.Empty;
        StatusMessage = string.Empty;
        ProgressValue = 0;
        CurrentStep = 0;
        Preview = null;
        CanProceed = false;
        IsExcel = false;
        HasMultipleSheets = false;
        MergeAllSheets = false;
        SelectedWorksheet = string.Empty;
        DetectedColumns.Clear();
        AvailableWorksheets.Clear();
        ValidationErrors.Clear();
        ValidationWarnings.Clear();
    }
}
