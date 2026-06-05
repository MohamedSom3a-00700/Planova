using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Planova.Activity.Application.Dto;
using Planova.Activity.Domain.Interfaces;

namespace Planova.UI.ViewModels.Activity;

public partial class ScheduleReportViewModel : ObservableObject
{
    private readonly IActivityReportService _reportService;

    public ScheduleReportViewModel(IActivityReportService reportService)
    {
        _reportService = reportService;
    }

    [ObservableProperty]
    private ScheduleReportDto? _report;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private bool _hasData;

    [RelayCommand]
    private async Task GenerateReportAsync(CancellationToken ct)
    {
        IsLoading = true;
        try
        {
            Report = await _reportService.GenerateScheduleReportAsync(1, ct);
            HasData = true;
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task ExportToExcelAsync(CancellationToken ct)
    {
        if (Report is null) return;
        await _reportService.ExportToExcelAsync(1, ct);
    }

    [RelayCommand]
    private async Task ExportToPdfAsync(CancellationToken ct)
    {
        if (Report is null) return;
        await _reportService.ExportToPdfAsync(1, ct);
    }
}
