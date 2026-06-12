using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Planova.Application.Dto;
using Planova.Application.Services;
using Planova.Boq.Application.Dto;
using Planova.Boq.Domain.Interfaces;
using Planova.Shared.Abstractions;
using System.IO;
using Planova.Boq.Application.Services;
using Planova.Boq.CsvReader;
using Planova.Excel.Readers;

namespace Planova.UI.ViewModels.Boq;

public partial class BoqImportViewModel : ObservableObject
{
    private readonly IBoqImportService _importService;
    private readonly IBoqCsvReader _csvReader;
    private readonly IExcelRowReader _excelReader;
    private readonly IWorkbookReader _workbookReader;
    private readonly IBoqSession _session;
    private readonly IProjectDocumentService _projectDocumentService;
    private readonly ICurrentProjectService _currentProjectService;

    public BoqImportViewModel(IBoqImportService importService, IBoqCsvReader csvReader,
        IExcelRowReader excelReader, IWorkbookReader workbookReader, IBoqSession session,
        IProjectDocumentService projectDocumentService,
        ICurrentProjectService currentProjectService)
    {
        _importService = importService;
        _csvReader = csvReader;
        _excelReader = excelReader;
        _workbookReader = workbookReader;
        _session = session;
        _projectDocumentService = projectDocumentService;
        _currentProjectService = currentProjectService;
        _currentProjectService.CurrentProjectChanged += OnCurrentProjectChanged;
        DetectedColumns.CollectionChanged += (_, _) => OnPropertyChanged(nameof(HasDetectedColumns));

        if (_currentProjectService.CurrentProject is not null)
        {
            CurrentProjectId = GuidFromInt(_currentProjectService.CurrentProject.Id);
            _ = LoadBoqDocumentsAsync(CancellationToken.None);
        }
    }

    private async void OnCurrentProjectChanged(object? sender, ProjectContext? context)
    {
        if (context != null)
        {
            CurrentProjectId = GuidFromInt(context.Id);
            await LoadBoqDocumentsCommand.ExecuteAsync(null);
        }
    }

    private static Guid GuidFromInt(int value)
    {
        var bytes = new byte[16];
        BitConverter.GetBytes(value).CopyTo(bytes, 0);
        return new Guid(bytes);
    }

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private Guid _currentProjectId;

    [ObservableProperty]
    private ProjectDocumentDto? _selectedProjectDocument;

    [ObservableProperty]
    private ProjectDocumentDto? _selectedBoqDocument;

    [ObservableProperty]
    private string _selectedFilePath = string.Empty;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    [ObservableProperty]
    private int _progressValue;

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

    [ObservableProperty]
    private bool _createNewBoq = true;

    public bool HasProjectDocuments => BoqDocuments.Count > 0;
    public bool HasFileSelected => !string.IsNullOrEmpty(SelectedFilePath) && CreateNewBoq;
    public bool HasDocumentSelected => !string.IsNullOrEmpty(SelectedFilePath) && !CreateNewBoq;
    public bool HasDetectedColumns => DetectedColumns.Count > 0;

    public ObservableCollection<string> DetectedColumns { get; } = new();
    public ObservableCollection<string> AvailableWorksheets { get; } = new();
    public ObservableCollection<ValidationIssue> ValidationErrors { get; } = new();
    public ObservableCollection<ValidationIssue> ValidationWarnings { get; } = new();
    public ObservableCollection<ProjectDocumentDto> BoqDocuments { get; } = new();

    private static readonly string[] CsiDivisionKeywords =
        ["division", "div", "csi", "masterformat", "master format", "section"];
    private static readonly string[] ClassificationKeywords =
        ["classification", "class", "category", "type", "group", "trade"];

    partial void OnCreateNewBoqChanged(bool value)
    {
        ClearSource();
        if (!value)
            _ = LoadBoqDocumentsAsync(default);
        OnPropertyChanged(nameof(HasFileSelected));
        OnPropertyChanged(nameof(HasDocumentSelected));
    }

    partial void OnSelectedWorksheetChanged(string value)
    {
        if (IsExcel && !string.IsNullOrEmpty(value) && !string.IsNullOrEmpty(SelectedFilePath))
            _ = DetectExcelHeadersAsync(default);
    }

    partial void OnMergeAllSheetsChanged(bool value)
    {
        if (IsExcel && value && AvailableWorksheets.Count > 0)
            _ = DetectExcelHeadersAsync(default);
    }

    [RelayCommand]
    private async Task LoadBoqDocumentsAsync(CancellationToken ct)
    {
        var projectId = _currentProjectService.CurrentProject?.Id;
        if (projectId == null) return;

        BoqDocuments.Clear();
        SelectedBoqDocument = null;
        OnPropertyChanged(nameof(HasProjectDocuments));

        try
        {
            var docs = (await _projectDocumentService.GetByTypeAsync(projectId.Value, "Boq", ct)).ToList();
            foreach (var doc in docs.OrderBy(d => d.FileName))
                BoqDocuments.Add(doc);
            OnPropertyChanged(nameof(HasProjectDocuments));
        }
        catch (Exception ex)
        {
            StatusMessage = $"Failed to load BOQ documents: {ex.Message}";
        }
    }

    private void ClearSource()
    {
        SelectedFilePath = string.Empty;
        SelectedProjectDocument = null;
        IsExcel = false;
        HasMultipleSheets = false;
        MergeAllSheets = false;
        SelectedWorksheet = string.Empty;
        DetectedColumns.Clear();
        AvailableWorksheets.Clear();
        StatusMessage = string.Empty;
        Preview = null;
        CanProceed = false;
        OnPropertyChanged(nameof(HasFileSelected));
        OnPropertyChanged(nameof(HasDocumentSelected));
    }

    [RelayCommand]
    private async Task SelectBoqDocumentAsync()
    {
        if (SelectedBoqDocument == null || CreateNewBoq) return;
        SelectedProjectDocument = SelectedBoqDocument;
        await LoadFromDocumentAsync(SelectedBoqDocument, CancellationToken.None);
    }

    [RelayCommand]
    private async Task SelectFileAsync(CancellationToken ct)
    {
        try
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Excel Files (*.xlsx;*.xls;*.xlsm)|*.xlsx;*.xls;*.xlsm|CSV Files (*.csv)|*.csv|All Files (*.*)|*.*",
                Title = "Select BOQ File"
            };

            if (dialog.ShowDialog() == true)
            {
                SelectedProjectDocument = null;
                SelectedFilePath = dialog.FileName;
                IsExcel = Path.GetExtension(SelectedFilePath).Equals(".xlsx", StringComparison.OrdinalIgnoreCase)
                       || Path.GetExtension(SelectedFilePath).Equals(".xls", StringComparison.OrdinalIgnoreCase)
                       || Path.GetExtension(SelectedFilePath).Equals(".xlsm", StringComparison.OrdinalIgnoreCase);

                StatusMessage = $"Selected: {Path.GetFileName(SelectedFilePath)}";
                OnPropertyChanged(nameof(HasFileSelected));

                if (IsExcel)
                    await DetectExcelWorksheetsAsync(ct);
                else
                    await DetectCsvHeadersAsync(ct);

                CanProceed = true;
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"File error: {ex.Message}";
        }
    }

    private async Task LoadFromDocumentAsync(ProjectDocumentDto doc, CancellationToken ct)
    {
        try
        {
            IsLoading = true;
            ClearSource();
            SelectedFilePath = doc.AbsolutePath;

            if (!File.Exists(SelectedFilePath))
            {
                StatusMessage = $"File not found at: {SelectedFilePath}";
                return;
            }

            IsExcel = doc.FileExtension.Equals(".xlsx", StringComparison.OrdinalIgnoreCase)
                   || doc.FileExtension.Equals(".xls", StringComparison.OrdinalIgnoreCase)
                   || doc.FileExtension.Equals(".xlsm", StringComparison.OrdinalIgnoreCase);

            StatusMessage = $"Selected: {doc.FileName}";
            OnPropertyChanged(nameof(HasDocumentSelected));

            if (IsExcel)
                await DetectExcelWorksheetsAsync(ct);
            else
                await DetectCsvHeadersAsync(ct);

            CanProceed = true;
        }
        catch (Exception ex)
        {
            ClearSource();
            StatusMessage = $"Error loading document: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task DetectExcelWorksheetsAsync(CancellationToken ct)
    {
        IsLoading = true;
        try
        {
            var sheets = await _excelReader.GetWorksheetsAsync(SelectedFilePath, ct);
            AvailableWorksheets.Clear();
            foreach (var sheet in sheets)
                AvailableWorksheets.Add(sheet);

            HasMultipleSheets = sheets.Count > 1;
            if (sheets.Count > 0)
            {
                SelectedWorksheet = sheets[0];
                MergeAllSheets = sheets.Count > 1;
            }
            else
            {
                SelectedWorksheet = string.Empty;
                MergeAllSheets = false;
            }

            StatusMessage = sheets.Count > 1
                ? $"{sheets.Count} sheets detected"
                : sheets.Count == 1
                    ? $"Sheet: {SelectedWorksheet}"
                    : "No worksheets found. The file may be corrupt or in an unsupported format.";

            if (sheets.Count > 0)
                await DetectExcelHeadersAsync(ct);
        }
        catch (Exception ex)
        {
            StatusMessage = $"Sheet detection failed: {ex.Message}";
            if (ex.Message.Contains("formula notes", StringComparison.OrdinalIgnoreCase))
            {
                StatusMessage = "The Excel file contains formula notes that could not be parsed. " +
                    "Try opening and resaving the file in Excel, or removing any formula comments.";
            }
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task DetectExcelHeadersAsync(CancellationToken ct)
    {
        try
        {
            var sheetName = MergeAllSheets ? AvailableWorksheets.FirstOrDefault() : SelectedWorksheet;
            if (string.IsNullOrEmpty(sheetName)) return;

            var sheetInfo = await _workbookReader.GetWorksheetInfoAsync(SelectedFilePath, sheetName, ct);
            DetectedColumns.Clear();
            foreach (var col in sheetInfo.Columns)
                DetectedColumns.Add(col);

            AutoDetectMappingColumns([.. sheetInfo.Columns]);
        }
        catch
        {
        }
    }

    private async Task DetectCsvHeadersAsync(CancellationToken ct)
    {
        try
        {
            var headers = await _csvReader.DetectHeadersAsync(SelectedFilePath, Delimiter, ct);
            DetectedColumns.Clear();
            foreach (var header in headers)
                DetectedColumns.Add(header);
            AutoDetectMappingColumns(headers);
        }
        catch (Exception ex)
        {
            StatusMessage = $"CSV header detection failed: {ex.Message}";
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
            var sheetName = MergeAllSheets ? null : SelectedWorksheet;

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
            else
            {
                var rows = await _excelReader.ReadAsync(SelectedFilePath, sheetName, ct);
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
    private async Task CommitImportAsync(CancellationToken ct)
    {
        try
        {
            if (string.IsNullOrEmpty(SelectedFilePath)) return;

            IsLoading = true;
            StatusMessage = "Importing...";

            var projectId = CurrentProjectId;
            var progress = new Progress<int>(value =>
            {
                ProgressValue = value;
            });

            var isCsv = Path.GetExtension(SelectedFilePath).Equals(".csv", StringComparison.OrdinalIgnoreCase);
            var sheetName = MergeAllSheets ? null : SelectedWorksheet;

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
                result = await _importService.ImportFromExcelAsync(projectId, SelectedFilePath, sheetName, null, progress, ct);
            }

            StatusMessage = $"Import complete: {result.ItemsImported} imported, {result.ItemsSkipped} skipped";
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
        ClearSource();
    }
}
