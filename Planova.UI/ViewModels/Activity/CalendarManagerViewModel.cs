using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Planova.Activity.Application.Dto;
using Planova.Activity.Domain.Interfaces;

namespace Planova.UI.ViewModels.Activity;

public partial class CalendarManagerViewModel : ObservableObject
{
    private readonly ICalendarService _calendarService;

    public CalendarManagerViewModel(ICalendarService calendarService)
    {
        _calendarService = calendarService;
    }

    [ObservableProperty]
    private ObservableCollection<CalendarDto> _calendars = [];

    [ObservableProperty]
    private CalendarDto? _selectedCalendar;

    [ObservableProperty]
    private ObservableCollection<CalendarDayDto> _calendarDays = [];

    [ObservableProperty]
    private DateTime _currentMonth = DateTime.Today;

    public string CurrentMonthLabel => CurrentMonth.ToString("MMMM yyyy");

    partial void OnCurrentMonthChanged(DateTime value)
    {
        OnPropertyChanged(nameof(CurrentMonthLabel));
    }

    [RelayCommand]
    private void PreviousMonth()
    {
        CurrentMonth = CurrentMonth.AddMonths(-1);
    }

    [RelayCommand]
    private void NextMonth()
    {
        CurrentMonth = CurrentMonth.AddMonths(1);
    }

    [RelayCommand]
    private async Task CreateCalendarAsync(CancellationToken ct)
    {
        var request = new CreateCalendarRequest
        {
            Name = "New Calendar",
            ProjectId = null
        };
        var dto = await _calendarService.CreateAsync(request, ct);
        Calendars.Add(dto);
    }

    [RelayCommand]
    private async Task ToggleDayStatusAsync(DateTime date, CancellationToken ct)
    {
        if (SelectedCalendar is null) return;
        var existing = CalendarDays.FirstOrDefault(d => d.Date == date);
        var newStatus = existing?.Status == "Working" ? "NonWorking" : "Working";
        await _calendarService.SetDayStatusAsync(SelectedCalendar.Id, date,
            Planova.Activity.Domain.Enums.CalendarDayStatus.NonWorking, ct: ct);
    }

    [RelayCommand]
    private async Task BulkSetNonWorkingAsync(Tuple<DateTime, DateTime> range, CancellationToken ct)
    {
        if (SelectedCalendar is null) return;
        await _calendarService.BulkSetDaysAsync(SelectedCalendar.Id, range.Item1, range.Item2,
            Planova.Activity.Domain.Enums.CalendarDayStatus.NonWorking, ct: ct);
    }

    [ObservableProperty]
    private string? _statusMessage;

    [RelayCommand]
    private async Task ImportNonWorkingDaysFromExcelAsync(CancellationToken ct)
    {
        StatusMessage = "Import from Excel — coming in Phase 6.";
        await Task.CompletedTask;
    }

    [RelayCommand]
    private async Task GetNonWorkingDaysFromAiAsync(CancellationToken ct)
    {
        StatusMessage = "AI calendar generation — coming in Phase 6.";
        await Task.CompletedTask;
    }
}
