using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using Planova.Excel.Models;
using Planova.Excel.Services;

namespace Planova.UI.ViewModels.Excel;

public partial class ImportViewModel : ObservableObject
{
    private readonly IImportService _importService;
    private readonly IWorkbookPreviewService _previewService;
    private readonly IMappingProfileService _profileService;

    public ImportViewModel(IImportService importService, IWorkbookPreviewService previewService, IMappingProfileService profileService)
    {
        _importService = importService;
        _previewService = previewService;
        _profileService = profileService;
        AvailableProfiles.CollectionChanged += (_, _) => OnPropertyChanged(nameof(HasAvailableProfiles));
    }

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private bool _hasError;

    [ObservableProperty]
    private int _currentStep = 1;

    public bool IsStep1Visible => CurrentStep == 1;
    public bool IsStep2Visible => CurrentStep == 2;
    public bool IsStep3Visible => CurrentStep == 3;
    public bool IsStep4Visible => CurrentStep == 4;
    public bool CanGoBack => CurrentStep > 1;
    public bool CanGoNext => CurrentStep switch
    {
        1 => !string.IsNullOrWhiteSpace(FilePath) && WorkbookInfo is not null,
        2 => !string.IsNullOrWhiteSpace(SelectedWorksheet) && !string.IsNullOrWhiteSpace(SelectedEntityType),
        3 => ValidationResult?.IsValid == true,
        _ => false
    };
    public bool IsStep3 => CurrentStep == 3;
    public bool IsNotStep3 => CurrentStep != 3;

    partial void OnCurrentStepChanged(int value)
    {
        OnPropertyChanged(nameof(IsStep1Visible));
        OnPropertyChanged(nameof(IsStep2Visible));
        OnPropertyChanged(nameof(IsStep3Visible));
        OnPropertyChanged(nameof(IsStep4Visible));
        OnPropertyChanged(nameof(CanGoBack));
        OnPropertyChanged(nameof(IsStep3));
        OnPropertyChanged(nameof(IsNotStep3));
        OnPropertyChanged(nameof(CanGoNext));
    }

    [ObservableProperty]
    private string _filePath = string.Empty;

    [ObservableProperty]
    private WorkbookInfo? _workbookInfo;

    [ObservableProperty]
    private string _selectedWorksheet = string.Empty;

    [ObservableProperty]
    private string _selectedEntityType = string.Empty;

    [ObservableProperty]
    private Dictionary<string, string> _columnMappings = new();

    [ObservableProperty]
    private ValidationResult? _validationResult;

    [ObservableProperty]
    private ImportResult? _importResult;

    [ObservableProperty]
    private DuplicateStrategy _duplicateStrategy = DuplicateStrategy.Prompt;

    [ObservableProperty]
    private int _importProgress;

    [ObservableProperty]
    private MappingProfile? _selectedProfile;

    public ObservableCollection<ColumnMappingEntry> ColumnMappingEntries { get; } = new();

    public bool HasAvailableProfiles => AvailableProfiles.Count > 0;

    public ObservableCollection<string> EntityFields { get; } = new();

    private static readonly Dictionary<string, string[]> EntityFieldMap = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Project"] = new[] { "Id", "Code", "Name", "Status", "Client", "StartDate", "FinishDate" },
        ["Activity"] = new[] { "Id", "Code", "Name", "ProjectId", "StartDate", "FinishDate", "Duration" },
        ["Resource"] = new[] { "Id", "Name", "Type", "Rate", "Currency" },
        ["Cost"] = new[] { "Id", "Description", "Amount", "Category", "ProjectId" },
        ["Risk"] = new[] { "Id", "Title", "Category", "Probability", "Impact", "Status" },
    };

    public ObservableCollection<string> EntityTypes { get; } = new()
    {
        "Project", "Activity", "Resource", "Cost", "Risk"
    };

    public ObservableCollection<WorksheetInfo> Worksheets { get; } = new();
    public ObservableCollection<MappingProfile> AvailableProfiles { get; } = new();

    public List<string> StepTitles { get; } = new()
    {
        "Select File", "Map Columns", "Validate", "Import"
    };

    [RelayCommand]
    private async Task SelectFileAsync()
    {
        var dialog = new OpenFileDialog
        {
            Filter = "Excel Workbooks|*.xlsx;*.xlsm|All Files|*.*",
            Title = "Select Excel Workbook to Import"
        };

        if (dialog.ShowDialog() != true) return;

        FilePath = dialog.FileName;

        IsLoading = true;
        HasError = false;

        try
        {
            WorkbookInfo = await _previewService.GetWorkbookInfoAsync(FilePath, CancellationToken.None);
            Worksheets.Clear();
            if (WorkbookInfo is not null)
            {
                foreach (var ws in WorkbookInfo.Worksheets)
                    Worksheets.Add(ws);
            }
            CurrentStep = 2;
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to open workbook: {ex.Message}";
            HasError = true;
        }
        finally
        {
            IsLoading = false;
        }
    }

    partial void OnFilePathChanged(string value) => OnPropertyChanged(nameof(CanGoNext));
    partial void OnWorkbookInfoChanged(WorkbookInfo? value) => OnPropertyChanged(nameof(CanGoNext));

    partial void OnSelectedWorksheetChanged(string value)
    {
        OnPropertyChanged(nameof(CanGoNext));
        PopulateColumns();
    }

    partial void OnSelectedEntityTypeChanged(string value)
    {
        OnPropertyChanged(nameof(CanGoNext));
        AutoMapColumns();
        _ = LoadProfilesAsync();
    }

    partial void OnValidationResultChanged(ValidationResult? value) => OnPropertyChanged(nameof(CanGoNext));

    private void PopulateColumns()
    {
        // Unsubscribe from old entries
        foreach (var entry in ColumnMappingEntries)
            entry.PropertyChanged -= OnMappingEntryPropertyChanged;
        ColumnMappingEntries.Clear();

        if (WorkbookInfo is null || string.IsNullOrWhiteSpace(SelectedWorksheet)) return;

        var ws = WorkbookInfo.Worksheets.FirstOrDefault(w =>
            w.Name.Equals(SelectedWorksheet, StringComparison.OrdinalIgnoreCase));
        if (ws is null) return;

        foreach (var col in ws.Columns)
        {
            var entry = new ColumnMappingEntry(col);
            entry.PropertyChanged += OnMappingEntryPropertyChanged;
            ColumnMappingEntries.Add(entry);
        }

        AutoMapColumns();
    }

    private void OnMappingEntryPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ColumnMappingEntry.MappedField))
            SyncColumnMappings();
    }

    private void AutoMapColumns()
    {
        EntityFields.Clear();
        if (string.IsNullOrWhiteSpace(SelectedEntityType)) return;

        if (EntityFieldMap.TryGetValue(SelectedEntityType, out var fields))
        {
            foreach (var f in fields)
                EntityFields.Add(f);
        }

        foreach (var entry in ColumnMappingEntries)
        {
            var match = EntityFields.FirstOrDefault(f =>
                f.Equals(entry.ExcelColumn, StringComparison.OrdinalIgnoreCase));
            entry.MappedField = match;
        }

        SyncColumnMappings();
    }

    private void ApplyMappingsToEntries()
    {
        if (ColumnMappings.Count == 0) return;
        foreach (var entry in ColumnMappingEntries)
        {
            if (ColumnMappings.TryGetValue(entry.ExcelColumn, out var mapped))
                entry.MappedField = mapped;
        }
    }

    private void SyncColumnMappings()
    {
        ColumnMappings = ColumnMappingEntries
            .Where(e => !string.IsNullOrWhiteSpace(e.MappedField))
            .ToDictionary(e => e.ExcelColumn, e => e.MappedField!);
    }

    [RelayCommand]
    private void NextStep()
    {
        if (!CanGoNext) return;
        if (CurrentStep < 4) CurrentStep++;
    }

    [RelayCommand]
    private void PreviousStep()
    {
        if (CurrentStep > 1) CurrentStep--;
    }

    [RelayCommand]
    private async Task LoadProfilesAsync()
    {
        if (string.IsNullOrWhiteSpace(SelectedEntityType)) return;

        try
        {
            var profiles = await _profileService.GetAllAsync(SelectedEntityType, CancellationToken.None);
            AvailableProfiles.Clear();
            foreach (var p in profiles)
                AvailableProfiles.Add(p);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to load profiles: {ex.Message}";
            HasError = true;
        }
    }

    [RelayCommand]
    private async Task ApplyProfileAsync()
    {
        if (SelectedProfile is null) return;

        try
        {
            ColumnMappings = new Dictionary<string, string>(SelectedProfile.ColumnMappings);
            ApplyMappingsToEntries();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to apply profile: {ex.Message}";
            HasError = true;
        }
    }

    [RelayCommand]
    private async Task ValidateAsync()
    {
        if (string.IsNullOrWhiteSpace(SelectedWorksheet) || string.IsNullOrWhiteSpace(SelectedEntityType))
            return;

        IsLoading = true;
        HasError = false;

        try
        {
            var request = new ImportRequest
            {
                FilePath = FilePath,
                WorksheetName = SelectedWorksheet,
                EntityType = SelectedEntityType,
                ColumnMappings = ColumnMappings,
                MappingProfileId = SelectedProfile?.Id,
                DuplicateHandling = DuplicateStrategy
            };

            ValidationResult = await _importService.ValidateAsync(request, CancellationToken.None);

            if (ValidationResult.IsValid)
            {
                CurrentStep = 4;
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Validation failed: {ex.Message}";
            HasError = true;
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task ImportAsync()
    {
        if (string.IsNullOrWhiteSpace(SelectedWorksheet))
        {
            ErrorMessage = "Please select a worksheet before importing.";
            HasError = true;
            return;
        }

        IsLoading = true;
        HasError = false;

        try
        {
            var request = new ImportRequest
            {
                FilePath = FilePath,
                WorksheetName = SelectedWorksheet,
                EntityType = SelectedEntityType,
                ColumnMappings = ColumnMappings,
                MappingProfileId = SelectedProfile?.Id,
                DuplicateHandling = DuplicateStrategy
            };

            var progress = new Progress<int>(p => ImportProgress = p);
            ImportResult = await _importService.ImportAsync(request, progress, CancellationToken.None);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Import failed: {ex.Message}";
            HasError = true;
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void Reset()
    {
        CurrentStep = 1;
        FilePath = string.Empty;
        WorkbookInfo = null;
        SelectedWorksheet = string.Empty;
        SelectedEntityType = string.Empty;
        ColumnMappings = new();
        SelectedProfile = null;
        ValidationResult = null;
        ImportResult = null;
        ImportProgress = 0;
        ErrorMessage = string.Empty;
        HasError = false;
        Worksheets.Clear();
        AvailableProfiles.Clear();
        foreach (var entry in ColumnMappingEntries)
            entry.PropertyChanged -= OnMappingEntryPropertyChanged;
        ColumnMappingEntries.Clear();
    }
}
