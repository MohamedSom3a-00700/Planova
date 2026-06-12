using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Planova.Reporting.Application.Dto;
using Planova.Reporting.Domain.Enums;
using Planova.Reporting.Domain.Interfaces;
using Planova.Shared.Abstractions;

namespace Planova.UI.ViewModels.Reporting;

public partial class MonthlyReportViewModel : ObservableObject
{
    private readonly IReportDataProvider<MonthlyReportDataDto> _dataProvider;
    private readonly IReportEngine _reportEngine;
    private readonly IReportAiService _aiService;
    private readonly IReportExportService _exportService;
    private readonly IReportInstanceRepository _instanceRepository;
    private readonly ICurrentProjectService _currentProjectService;
    private readonly ISettingsService _settings;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasCurrentInstance))]
    private Guid? _currentInstanceId;

    public Action<string>? OnStatusMessage { get; set; }

    public MonthlyReportViewModel(
        IReportDataProvider<MonthlyReportDataDto> dataProvider,
        IReportEngine reportEngine,
        IReportAiService aiService,
        IReportExportService exportService,
        IReportInstanceRepository instanceRepository,
        ICurrentProjectService currentProjectService,
        ISettingsService settings)
    {
        _dataProvider = dataProvider;
        _reportEngine = reportEngine;
        _aiService = aiService;
        _exportService = exportService;
        _instanceRepository = instanceRepository;
        _currentProjectService = currentProjectService;
        _settings = settings;
        SelectedMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
    }

    public static List<string> StatusOptions => ["Draft", "Final", "Archived"];

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasCurrentInstance))]
    private string _currentInstanceStatus = "Draft";

    public bool HasCurrentInstance => CurrentInstanceId.HasValue;

    [ObservableProperty]
    private DateTime _selectedMonth;

    [ObservableProperty]
    private MonthlyReportDataDto? _reportData;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasNoData))]
    private bool _isLoading;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasNoData))]
    private bool _hasData;

    public bool HasNoData => !HasData && !IsLoading;

    [ObservableProperty]
    private string? _aiNarrative;

    [ObservableProperty]
    private bool _hasAiNarrative;

    [ObservableProperty]
    private string? _statusMessage;

    [ObservableProperty]
    private string? _errorMessage;

    [RelayCommand]
    private async Task GenerateReportAsync(CancellationToken ct)
    {
        IsLoading = true;
        HasData = false;
        ErrorMessage = null;
        AiNarrative = null;
        HasAiNarrative = false;

        try
        {
            var start = SelectedMonth.Date;
            var end = start.AddMonths(1).AddDays(-1).AddHours(23).AddMinutes(59).AddSeconds(59);
            ReportData = await _dataProvider.CollectDataAsync(1, start, end, ct);
            HasData = true;
            OnStatusMessage?.Invoke($"Monthly report generated for {start:MMMM yyyy}");
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to generate report: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task GenerateAiNarrativeAsync(CancellationToken ct)
    {
        if (ReportData is null) return;

        try
        {
            AiNarrative = await _aiService.GenerateNarrativeAsync(ReportType.Monthly, ReportData, ct);
            HasAiNarrative = !string.IsNullOrEmpty(AiNarrative);
            OnStatusMessage?.Invoke(HasAiNarrative ? "AI narrative generated" : "AI narrative unavailable");
        }
        catch (Exception ex)
        {
            ErrorMessage = $"AI generation failed: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task ExportToExcelAsync(CancellationToken ct)
    {
        if (ReportData is null) return;
        await ExportAsync(_exportService.ExportToExcelAsync, ct);
    }

    [RelayCommand]
    private async Task ExportToPdfAsync(CancellationToken ct)
    {
        if (ReportData is null) return;
        await ExportAsync(_exportService.ExportToPdfAsync, ct);
    }

    [RelayCommand]
    private async Task ExportToWordAsync(CancellationToken ct)
    {
        if (ReportData is null) return;
        await ExportAsync(_exportService.ExportToWordAsync, ct);
    }

    private async Task ExportAsync(Func<Guid, object, CancellationToken, Task<byte[]>> exportFunc, CancellationToken ct)
    {
        try
        {
            IsLoading = true;
            var start = SelectedMonth.Date;
            var end = start.AddMonths(1).AddDays(-1).AddHours(23).AddMinutes(59).AddSeconds(59);
            var instance = await _reportEngine.GenerateAsync(1, ReportType.Monthly, start, end, ct);
            CurrentInstanceId = instance.Id;
            CurrentInstanceStatus = instance.Status;
            var content = await exportFunc.Invoke(instance.Id, ReportData!, ct);

            var projectName = _currentProjectService.CurrentProject?.Name ?? "Project";
            var extension = exportFunc.Method.Name switch
            {
                nameof(IReportExportService.ExportToExcelAsync) => ".xlsx",
                nameof(IReportExportService.ExportToPdfAsync) => ".pdf",
                nameof(IReportExportService.ExportToWordAsync) => ".docx",
                _ => ".bin"
            };

            var fileName = $"{SanitizeFileName(projectName)}_Monthly_{start:yyyy-MM}{extension}";

            var reportsFolder = _settings.Get<string>("Reporting_ReportsFolderPath");
            if (string.IsNullOrEmpty(reportsFolder))
            {
                var folderDialog = new Microsoft.Win32.OpenFileDialog
                {
                    Title = "Select Reports Export Folder",
                    ValidateNames = false,
                    CheckFileExists = false,
                    CheckPathExists = true,
                    FileName = "Select"
                };
                if (folderDialog.ShowDialog() == true)
                {
                    var folder = System.IO.Path.GetDirectoryName(folderDialog.FileName);
                    if (!string.IsNullOrEmpty(folder))
                    {
                        reportsFolder = folder;
                        _settings.Set("Reporting_ReportsFolderPath", folder);
                        _settings.Set("Reporting_AutoSaveReports", true);
                        await _settings.Save();
                    }
                }
                else
                {
                    ErrorMessage = "Export cancelled: please set a Reports folder path in Settings.";
                    return;
                }
            }

            var dateFolder = start.ToString("yyyy-MM-dd");
            var saveDir = Path.Combine(reportsFolder, "Monthly", dateFolder);
            Directory.CreateDirectory(saveDir);
            var autoPath = Path.Combine(saveDir, fileName);
            await File.WriteAllBytesAsync(autoPath, content, ct);
            OnStatusMessage?.Invoke($"Export complete — {autoPath}");
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Export failed: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private static string SanitizeFileName(string name)
    {
        var invalid = Path.GetInvalidFileNameChars();
        return string.Concat(name.Where(ch => !invalid.Contains(ch))).Trim();
    }

    [RelayCommand]
    private async Task ChangeStatusAsync(CancellationToken ct)
    {
        if (CurrentInstanceId is null) return;

        try
        {
            IsLoading = true;
            var status = Enum.Parse<ReportStatus>(CurrentInstanceStatus);
            var instance = await _instanceRepository.GetByIdAsync(CurrentInstanceId.Value, ct);
            if (instance != null)
            {
                instance.Status = status;
                instance.UpdatedAt = DateTime.UtcNow;
                await _instanceRepository.UpdateAsync(instance, ct);
                OnStatusMessage?.Invoke($"Status changed to {CurrentInstanceStatus}");
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to change status: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }
}
