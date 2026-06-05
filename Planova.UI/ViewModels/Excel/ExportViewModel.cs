using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using Planova.Excel.Models;
using Planova.Excel.Services;

namespace Planova.UI.ViewModels.Excel;

public partial class ExportViewModel : ObservableObject
{
    private readonly IExportService _exportService;

    public ExportViewModel(IExportService exportService)
    {
        _exportService = exportService;
        LoadEntityTypes();
    }

    private void LoadEntityTypes()
    {
        foreach (var type in _exportService.GetExportableEntityTypes())
        {
            EntityTypes.Add(type);
        }
    }

    partial void OnSelectedEntityTypeChanged(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return;
        _ = SelectEntityTypeAsync(value);
    }

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private bool _hasError;

    [ObservableProperty]
    private string _selectedEntityType = string.Empty;

    [ObservableProperty]
    private string _outputPath = string.Empty;

    [ObservableProperty]
    private ExportRequest? _exportRequest;

    [ObservableProperty]
    private ExportResult? _exportResult;

    [ObservableProperty]
    private int _exportProgress;

    public ObservableCollection<string> EntityTypes { get; } = new();
    public ObservableCollection<string> AvailableColumns { get; } = new();
    public ObservableCollection<string> SelectedColumns { get; } = new();

    [RelayCommand]
    private async Task LoadEntityTypesAsync()
    {
        IsLoading = true;
        try
        {
            EntityTypes.Clear();
            foreach (var type in _exportService.GetExportableEntityTypes())
            {
                EntityTypes.Add(type);
            }
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task SelectEntityTypeAsync(string? entityType)
    {
        if (string.IsNullOrWhiteSpace(entityType)) return;

        IsLoading = true;
        HasError = false;

        try
        {
            ExportRequest = await _exportService.BuildRequestAsync(entityType, CancellationToken.None);
            SelectedEntityType = entityType;

            AvailableColumns.Clear();
            SelectedColumns.Clear();
            if (ExportRequest is not null)
            {
                foreach (var col in ExportRequest.SelectedColumns)
                {
                    AvailableColumns.Add(col);
                    SelectedColumns.Add(col);
                }
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to load entity: {ex.Message}";
            HasError = true;
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void BrowseOutputPath()
    {
        var dialog = new SaveFileDialog
        {
            Filter = "Excel Workbook|*.xlsx",
            Title = "Save Exported Workbook",
            FileName = $"{SelectedEntityType}_Export.xlsx"
        };

        if (dialog.ShowDialog() == true)
        {
            OutputPath = dialog.FileName;
        }
    }

    [RelayCommand]
    private async Task ExportAsync()
    {
        if (ExportRequest is null || string.IsNullOrWhiteSpace(OutputPath)) return;

        IsLoading = true;
        HasError = false;

        try
        {
            var request = new ExportRequest
            {
                EntityType = SelectedEntityType,
                SelectedColumns = SelectedColumns.ToList(),
                OutputPath = OutputPath,
                SheetName = SelectedEntityType,
                IncludeHeaders = true
            };

            var progress = new Progress<int>(p => ExportProgress = p);
            ExportResult = await _exportService.ExportAsync(request, progress, CancellationToken.None);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Export failed: {ex.Message}";
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
        SelectedEntityType = string.Empty;
        OutputPath = string.Empty;
        ExportRequest = null;
        ExportResult = null;
        ExportProgress = 0;
        ErrorMessage = string.Empty;
        HasError = false;
        AvailableColumns.Clear();
        SelectedColumns.Clear();
    }
}
