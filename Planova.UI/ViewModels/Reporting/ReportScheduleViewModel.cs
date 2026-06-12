using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Planova.Reporting.Application.Dto;
using Planova.Reporting.Domain.Interfaces;

namespace Planova.UI.ViewModels.Reporting;

public partial class ReportScheduleViewModel : ObservableObject
{
    private readonly IReportSchedulerService _schedulerService;

    public Action<string>? OnStatusMessage { get; set; }

    public ReportScheduleViewModel(IReportSchedulerService schedulerService)
    {
        _schedulerService = schedulerService;
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasNoSchedules))]
    private ObservableCollection<ReportScheduleDto> _schedules = [];

    public bool HasNoSchedules => Schedules.Count == 0 && !IsLoading;

    [ObservableProperty]
    private ReportScheduleDto? _selectedSchedule;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasNoSchedules))]
    private bool _isLoading;

    [ObservableProperty]
    private bool _isFormOpen;

    [ObservableProperty]
    private string? _statusMessage;

    [ObservableProperty]
    private string? _errorMessage;

    [ObservableProperty]
    private string _formReportType = "Daily";

    [ObservableProperty]
    private string _formFrequency = "Daily";

    [ObservableProperty]
    private int? _formDayOfWeek;

    [ObservableProperty]
    private int? _formDayOfMonth;

    [ObservableProperty]
    private string _formTimeOfDay = "08:00";

    [ObservableProperty]
    private string _formTimeZoneId = "UTC";

    [ObservableProperty]
    private string _formExportFormats = "Excel";

    [ObservableProperty]
    private int _formMaxRetries = 3;

    [ObservableProperty]
    private bool _isEditing;

    private Guid _editingId;

    [RelayCommand]
    private async Task LoadSchedulesAsync(CancellationToken ct)
    {
        IsLoading = true;
        ErrorMessage = null;

        try
        {
            var list = await _schedulerService.GetSchedulesAsync(1, ct);
            Schedules = new ObservableCollection<ReportScheduleDto>(list);
            OnStatusMessage?.Invoke($"Loaded {list.Count} schedule(s)");
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to load schedules: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void OpenNewForm()
    {
        IsEditing = false;
        FormReportType = "Daily";
        FormFrequency = "Daily";
        FormDayOfWeek = null;
        FormDayOfMonth = null;
        FormTimeOfDay = "08:00";
        FormTimeZoneId = "UTC";
        FormExportFormats = "Excel";
        FormMaxRetries = 3;
        IsFormOpen = true;
        OnStatusMessage?.Invoke("New schedule form opened");
    }

    [RelayCommand]
    private void OpenEditForm()
    {
        if (SelectedSchedule is null) return;

        IsEditing = true;
        _editingId = SelectedSchedule.Id;
        FormReportType = SelectedSchedule.ReportType;
        FormFrequency = SelectedSchedule.Frequency;
        FormDayOfWeek = SelectedSchedule.DayOfWeek;
        FormDayOfMonth = SelectedSchedule.DayOfMonth;
        FormTimeOfDay = SelectedSchedule.TimeOfDay;
        FormTimeZoneId = SelectedSchedule.TimeZoneId;
        FormExportFormats = SelectedSchedule.ExportFormats;
        FormMaxRetries = SelectedSchedule.MaxRetries;
        IsFormOpen = true;
        OnStatusMessage?.Invoke("Edit schedule form opened");
    }

    [RelayCommand]
    private void CloseForm()
    {
        IsFormOpen = false;
        OnStatusMessage?.Invoke("Schedule form closed");
    }

    [RelayCommand]
    private async Task SaveScheduleAsync(CancellationToken ct)
    {
        IsLoading = true;
        ErrorMessage = null;

        try
        {
            if (IsEditing)
            {
                var updateRequest = new UpdateScheduleRequest(
                    FormFrequency, FormDayOfWeek, FormDayOfMonth,
                    FormTimeOfDay, FormTimeZoneId, FormExportFormats, FormMaxRetries);
                await _schedulerService.UpdateAsync(_editingId, updateRequest, ct);
                OnStatusMessage?.Invoke("Schedule updated");
            }
            else
            {
                var createRequest = new CreateScheduleRequest(
                    1, FormReportType, FormFrequency, FormDayOfWeek,
                    FormDayOfMonth, FormTimeOfDay, FormTimeZoneId,
                    FormExportFormats, FormMaxRetries);
                await _schedulerService.CreateAsync(createRequest, ct);
                OnStatusMessage?.Invoke("Schedule created");
            }

            IsFormOpen = false;
            await LoadSchedulesAsync(ct);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to save schedule: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task DeleteScheduleAsync(CancellationToken ct)
    {
        if (SelectedSchedule is null) return;

        IsLoading = true;
        ErrorMessage = null;

        try
        {
            await _schedulerService.DeleteAsync(SelectedSchedule.Id, ct);
            OnStatusMessage?.Invoke("Schedule deleted");
            await LoadSchedulesAsync(ct);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to delete schedule: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task ToggleActiveAsync(CancellationToken ct)
    {
        if (SelectedSchedule is null) return;

        IsLoading = true;
        ErrorMessage = null;

        try
        {
            await _schedulerService.ToggleActiveAsync(SelectedSchedule.Id, !SelectedSchedule.IsActive, ct);
            OnStatusMessage?.Invoke(SelectedSchedule.IsActive ? "Schedule deactivated" : "Schedule activated");
            await LoadSchedulesAsync(ct);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to toggle schedule: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    public static string[] ReportTypes => ["Daily", "Weekly", "Monthly", "Executive"];

    public static string[] Frequencies => ["Daily", "Weekly", "Monthly"];

    public static string[] TimeZones =>
    [
        "UTC",
        "Eastern Standard Time",
        "Central Standard Time",
        "Mountain Standard Time",
        "Pacific Standard Time",
        "Arab Standard Time",
        "Egypt Standard Time",
        "GMT Standard Time",
        "India Standard Time"
    ];
}
