using System.Collections;
using System.Collections.ObjectModel;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Planova.Reporting.Application.Dto;
using Planova.Reporting.Application.Mappings;
using Planova.Reporting.Domain.Interfaces;
using Planova.Shared.Abstractions;

namespace Planova.UI.ViewModels.Reporting;

public partial class ReportHistoryViewModel : ObservableObject
{
    private readonly IReportInstanceRepository _instanceRepository;
    private readonly IReportExportService _exportService;
    private readonly ICurrentProjectService _currentProjectService;
    private readonly ISettingsService _settings;

    public Action<string>? OnStatusMessage { get; set; }

    public ReportHistoryViewModel(
        IReportInstanceRepository instanceRepository,
        IReportExportService exportService,
        ICurrentProjectService currentProjectService,
        ISettingsService settings)
    {
        _instanceRepository = instanceRepository;
        _exportService = exportService;
        _currentProjectService = currentProjectService;
        _settings = settings;
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasNoHistory))]
    private ObservableCollection<ReportInstanceDto> _instances = [];

    [ObservableProperty]
    private ReportInstanceDto? _selectedInstance;

    public List<ReportInstanceDto> SelectedInstances { get; set; } = [];

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasNoHistory))]
    private bool _isLoading;

    public bool HasNoHistory => Instances.Count == 0 && !IsLoading;

    [ObservableProperty]
    private string? _statusMessage;

    [ObservableProperty]
    private string? _errorMessage;

    [ObservableProperty]
    private string _filterReportType = "All";

    [ObservableProperty]
    private string _filterStatus = "All";

    [ObservableProperty]
    private DateTime? _filterFrom;

    [ObservableProperty]
    private DateTime? _filterTo;

    public static string[] ReportTypeFilters => ["All", "Daily", "Weekly", "Monthly", "Executive"];

    public static string[] StatusFilters => ["All", "Draft", "Final", "Archived"];

    [RelayCommand]
    private async Task LoadHistoryAsync(CancellationToken ct)
    {
        IsLoading = true;
        ErrorMessage = null;

        try
        {
            var list = await _instanceRepository.GetByProjectAsync(1, null, null, FilterFrom, FilterTo, ct);

            var filtered = list.AsEnumerable();

            if (FilterReportType != "All")
                filtered = filtered.Where(i => i.ReportType.ToString() == FilterReportType);

            if (FilterStatus != "All")
                filtered = filtered.Where(i => i.Status.ToString() == FilterStatus);

            Instances = new ObservableCollection<ReportInstanceDto>(
                filtered.Select(i =>
                {
                    var dto = i.ToDto();
                    dto.ExportExists = i.Exports?.Any(e => System.IO.File.Exists(e.FilePath)) ?? false;
                    return dto;
                }));

            OnStatusMessage?.Invoke($"Loaded {Instances.Count} report(s)");
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to load history: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task ReExportAsync(CancellationToken ct)
    {
        if (SelectedInstance is null) return;

        try
        {
            IsLoading = true;
            var content = await _exportService.ExportToPdfAsync(SelectedInstance.Id, ct);

            var projectName = _currentProjectService.CurrentProject?.Name ?? "Project";
            var fileName = $"{SanitizeFileName(projectName)}_{SelectedInstance.Title.Replace(" ", "_")}.pdf";

            var reportsFolder = _settings.Get<string>("Reporting_ReportsFolderPath");
            if (!string.IsNullOrEmpty(reportsFolder) && _settings.Get<bool?>("Reporting_AutoSaveReports") == true)
            {
                var dateFolder = SelectedInstance.PeriodStart.ToString("yyyy-MM-dd");
                var saveDir = Path.Combine(reportsFolder, SelectedInstance.ReportType, dateFolder);
                Directory.CreateDirectory(saveDir);
                var autoPath = Path.Combine(saveDir, fileName);
                await System.IO.File.WriteAllBytesAsync(autoPath, content, ct);
            }

            var saveDialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "PDF Files (*.pdf)|*.pdf",
                FileName = fileName
            };

            if (saveDialog.ShowDialog() == true)
            {
                await System.IO.File.WriteAllBytesAsync(saveDialog.FileName, content, ct);
                OnStatusMessage?.Invoke($"Report re-exported to {saveDialog.FileName}");
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Re-export failed: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task DeleteSelectedInstancesAsync(CancellationToken ct)
    {
        if (SelectedInstances.Count == 0) return;

        try
        {
            IsLoading = true;
            foreach (var item in SelectedInstances)
            {
                var instance = await _instanceRepository.GetByIdAsync(item.Id, ct);
                if (instance != null)
                    await _instanceRepository.DeleteAsync(instance, ct);
            }
            OnStatusMessage?.Invoke($"Deleted {SelectedInstances.Count} report(s)");
            SelectedInstances.Clear();
            SelectedInstance = null;
            await LoadHistoryAsync(ct);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Delete failed: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void SelectAll()
    {
        var allSelected = SelectedInstances.Count == Instances.Count;
        if (allSelected)
        {
            SelectedInstances.Clear();
            SelectedInstance = null;
        }
        else
        {
            SelectedInstances = [.. Instances];
            SelectedInstance = SelectedInstances.FirstOrDefault();
        }
    }

    private static string SanitizeFileName(string name)
    {
        var invalid = System.IO.Path.GetInvalidFileNameChars();
        return string.Concat(name.Where(ch => !invalid.Contains(ch))).Trim();
    }
}
