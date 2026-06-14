using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Planova.Primavera.Application.Dto;
using Planova.Primavera.Domain.Interfaces;

namespace Planova.UI.ViewModels.Primavera;

public partial class PrimaveraCalendarsViewModel : ObservableObject
{
    private readonly IPrimaveraWorkspaceService _workspaceService;
    private List<PrimaveraCalendarDto> _allCalendars = new();

    public PrimaveraCalendarsViewModel(IPrimaveraWorkspaceService workspaceService)
    {
        _workspaceService = workspaceService;
    }

    public ObservableCollection<PrimaveraCalendarDto> Calendars { get; } = new();

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _filterText = string.Empty;

    public async Task LoadAsync(int projectId, CancellationToken ct = default)
    {
        IsLoading = true;
        try
        {
            _allCalendars = await _workspaceService.GetCalendarsAsync(projectId, ct);
            ApplyFilter();
        }
        finally
        {
            IsLoading = false;
        }
    }

    partial void OnFilterTextChanged(string value) => ApplyFilter();

    private void ApplyFilter()
    {
        Calendars.Clear();
        var filtered = string.IsNullOrWhiteSpace(FilterText)
            ? _allCalendars
            : _allCalendars.Where(c =>
                c.Name.Contains(FilterText, StringComparison.OrdinalIgnoreCase) ||
                c.CalendarId.Contains(FilterText, StringComparison.OrdinalIgnoreCase)).ToList();
        foreach (var item in filtered) Calendars.Add(item);
    }

    [RelayCommand]
    private async Task SaveCalendarAsync(PrimaveraCalendarDto? dto)
    {
        if (dto == null) return;
        await _workspaceService.UpdateCalendarAsync(dto);
        var idx = _allCalendars.FindIndex(c => c.Id == dto.Id);
        if (idx >= 0) _allCalendars[idx] = dto;
    }
}
