using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Planova.ScheduleComparison.Domain.Interfaces;
using Planova.Shared.Abstractions;

namespace Planova.UI.ViewModels.ScheduleComparison;

public partial class ComparisonExportViewModel : ObservableObject
{
    private readonly IComparisonExportService _exportService;
    private readonly ICurrentProjectService _currentProjectService;

    [ObservableProperty]
    private Guid _sessionId;

    [ObservableProperty]
    private bool _isExporting;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    [ObservableProperty]
    private string _exportPath = string.Empty;

    public ComparisonExportViewModel(
        IComparisonExportService exportService,
        ICurrentProjectService currentProjectService)
    {
        _exportService = exportService;
        _currentProjectService = currentProjectService;
    }

    [RelayCommand]
    private async Task ExportToExcelAsync(CancellationToken ct)
    {
        IsExporting = true;
        StatusMessage = "Exporting to Excel...";
        try
        {
            ExportPath = await _exportService.ExportToExcelAsync(SessionId, GetExportDirectory(), ct);
            StatusMessage = "Excel export completed.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Export failed: {ex.Message}";
        }
        finally
        {
            IsExporting = false;
        }
    }

    [RelayCommand]
    private async Task ExportToPdfAsync(CancellationToken ct)
    {
        IsExporting = true;
        StatusMessage = "Exporting to PDF...";
        try
        {
            ExportPath = await _exportService.ExportToPdfAsync(SessionId, GetExportDirectory(), ct);
            StatusMessage = "PDF export completed.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Export failed: {ex.Message}";
        }
        finally
        {
            IsExporting = false;
        }
    }

    [RelayCommand]
    private async Task ExportToJsonAsync(CancellationToken ct)
    {
        IsExporting = true;
        StatusMessage = "Exporting to JSON...";
        try
        {
            ExportPath = await _exportService.ExportToJsonAsync(SessionId, GetExportDirectory(), ct);
            StatusMessage = "JSON export completed.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Export failed: {ex.Message}";
        }
        finally
        {
            IsExporting = false;
        }
    }

    private string GetExportDirectory()
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var projectId = _currentProjectService.CurrentProject?.Id ?? 0;
        return Path.Combine(appData, "Planova", "Projects", projectId.ToString(), "Comparisons", SessionId.ToString());
    }
}
