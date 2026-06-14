using System.Collections.ObjectModel;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Planova.ScheduleComparison.Application.Dto;
using Planova.ScheduleComparison.Application.Mappings;
using Planova.ScheduleComparison.Domain.Interfaces;

namespace Planova.UI.ViewModels.ScheduleComparison;

public partial class ComparisonHistoryViewModel : ObservableObject
{
    private readonly IScheduleComparisonService _comparisonService;
    private readonly IComparisonExportService _exportService;

    [ObservableProperty]
    private ObservableCollection<ComparisonSessionDto> _sessions = new();

    [ObservableProperty]
    private ComparisonSessionDto? _selectedSession;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    public ComparisonHistoryViewModel(
        IScheduleComparisonService comparisonService,
        IComparisonExportService exportService)
    {
        _comparisonService = comparisonService;
        _exportService = exportService;
    }

    [RelayCommand]
    private async Task LoadSessionsAsync(int projectId, CancellationToken ct)
    {
        IsLoading = true;
        try
        {
            var sessions = await _comparisonService.ListSessionsAsync(projectId, ct);
            Sessions = new ObservableCollection<ComparisonSessionDto>(sessions.ToDtoList());
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task ReOpenSessionAsync(CancellationToken ct)
    {
        if (SelectedSession == null)
            return;

        await _comparisonService.ReOpenSessionAsync(SelectedSession.Id, ct);
        StatusMessage = $"Session '{SelectedSession.SourceLabel ?? SelectedSession.Id.ToString()}' re-opened.";
    }

    [RelayCommand]
    private async Task DeleteSessionAsync(CancellationToken ct)
    {
        if (SelectedSession == null)
            return;

        await _comparisonService.SoftDeleteSessionAsync(SelectedSession.Id, ct);
        Sessions.Remove(SelectedSession);
        StatusMessage = "Session deleted.";
    }

    [RelayCommand]
    private async Task ReExportSessionAsync(CancellationToken ct)
    {
        if (SelectedSession == null)
            return;

        var projectId = SelectedSession.ProjectId;
        var exportDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "Planova", "Projects", projectId.ToString(),
            "Comparisons", SelectedSession.Id.ToString());

        StatusMessage = "Re-exporting...";
        try
        {
            var excelPath = await _exportService.ExportToExcelAsync(SelectedSession.Id, exportDir, ct);
            var pdfPath = await _exportService.ExportToPdfAsync(SelectedSession.Id, exportDir, ct);
            var jsonPath = await _exportService.ExportToJsonAsync(SelectedSession.Id, exportDir, ct);
            StatusMessage = $"Re-exported: Excel ({Path.GetFileName(excelPath)}), PDF ({Path.GetFileName(pdfPath)}), JSON ({Path.GetFileName(jsonPath)})";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Re-export failed: {ex.Message}";
        }
    }
}
