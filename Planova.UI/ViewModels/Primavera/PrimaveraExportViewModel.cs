using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Planova.Primavera.Application.Models;
using Planova.Primavera.Domain.Interfaces;

namespace Planova.UI.ViewModels.Primavera;

public partial class PrimaveraExportViewModel : ObservableObject
{
    private readonly IPrimaveraExportService _exportService;

    public PrimaveraExportViewModel(IPrimaveraExportService exportService)
    {
        _exportService = exportService;
    }

    [ObservableProperty]
    private string _outputPath = string.Empty;

    [ObservableProperty]
    private bool _isExporting;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    [ObservableProperty]
    private bool _includeActivities = true;

    [ObservableProperty]
    private bool _includeRelationships = true;

    [ObservableProperty]
    private bool _includeResources = true;

    [ObservableProperty]
    private bool _includeCalendars = true;

    [ObservableProperty]
    private bool _includeCodes = true;

    [ObservableProperty]
    private bool _includeBaselines = true;

    [ObservableProperty]
    private bool _includeUdfs = true;

    [ObservableProperty]
    private bool _preserveRawTables = true;

    [ObservableProperty]
    private string _exportStatus = string.Empty;

    [ObservableProperty]
    private int _totalEntitiesExported;

    [ObservableProperty]
    private string _exportDetails = string.Empty;

    [ObservableProperty]
    private bool _hasExportResult;

    [RelayCommand]
    private async Task BrowseOutputPathAsync()
    {
        var dialog = new Microsoft.Win32.SaveFileDialog
        {
            Filter = "XER Files (*.xer)|*.xer",
            Title = "Save Exported XER File"
        };

        if (dialog.ShowDialog() == true)
        {
            OutputPath = dialog.FileName;
        }
    }

    [RelayCommand]
    private async Task ExportAsync()
    {
        if (string.IsNullOrEmpty(OutputPath)) return;

        IsExporting = true;
        StatusMessage = "Exporting...";

        try
        {
            var profile = new PrimaveraExportProfile
            {
                ProjectId = 1,
                OutputPath = OutputPath,
                IncludeActivities = IncludeActivities,
                IncludeRelationships = IncludeRelationships,
                IncludeResourceAssignments = IncludeResources,
                IncludeCalendars = IncludeCalendars,
                IncludeCodes = IncludeCodes,
                IncludeBaselines = IncludeBaselines,
                IncludeUdfs = IncludeUdfs,
                PreserveRawTables = PreserveRawTables
            };

            var filePath = await _exportService.ExportWithProfileAsync(profile);

            ExportStatus = "Completed";
            var counts = new List<string>();
            if (IncludeActivities) counts.Add("Activities");
            if (IncludeRelationships) counts.Add("Relationships");
            if (IncludeResources) counts.Add("Resources");
            if (IncludeCalendars) counts.Add("Calendars");
            if (IncludeCodes) counts.Add("Codes");
            if (IncludeBaselines) counts.Add("Baselines");
            if (IncludeUdfs) counts.Add("UDFs");
            TotalEntitiesExported = counts.Count;
            ExportDetails = string.Join(", ", counts);
            HasExportResult = true;
            StatusMessage = $"Exported to: {filePath}";
        }
        catch (Exception ex)
        {
            ExportStatus = "Failed";
            HasExportResult = true;
            StatusMessage = $"Export failed: {ex.Message}";
        }
        finally
        {
            IsExporting = false;
        }
    }
}
