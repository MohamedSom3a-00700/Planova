using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Planova.Cost.Application.Dto;
using Planova.Cost.Domain.Interfaces;
using Planova.Shared.Abstractions;

namespace Planova.UI.ViewModels.Cost;

public partial class CostReportViewModel : ObservableObject
{
    private readonly ICostReportService _reportService;
    private readonly ICurrentProjectService _currentProjectService;

    [ObservableProperty]
    private CostReportType _selectedReportType;

    [ObservableProperty]
    private ReportResultDto? _reportResult;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    public List<CostReportType> ReportTypes { get; } = new()
    {
        CostReportType.CostBreakdown,
        CostReportType.CashFlow,
        CostReportType.Evm,
        CostReportType.BudgetSummary
    };

    public CostReportViewModel(
        ICostReportService reportService,
        ICurrentProjectService currentProjectService)
    {
        _reportService = reportService;
        _currentProjectService = currentProjectService;
    }

    [RelayCommand]
    private async Task GenerateReportAsync()
    {
        var projectId = _currentProjectService.CurrentProject?.Id;
        if (projectId == null) return;

        IsLoading = true;
        StatusMessage = $"Generating {SelectedReportType} report...";
        try
        {
            ReportResult = await _reportService.GenerateReportAsync(SelectedReportType, projectId.Value);
            StatusMessage = $"{SelectedReportType} report generated successfully.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Report generation failed: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task ExportExcelAsync()
    {
        var projectId = _currentProjectService.CurrentProject?.Id;
        if (projectId == null) return;

        IsLoading = true;
        try
        {
            var data = await _reportService.ExportToExcelAsync(SelectedReportType, projectId.Value);
            StatusMessage = $"Excel export generated ({data.Length} bytes).";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task ExportPdfAsync()
    {
        var projectId = _currentProjectService.CurrentProject?.Id;
        if (projectId == null) return;

        IsLoading = true;
        try
        {
            var data = await _reportService.ExportToPdfAsync(SelectedReportType, projectId.Value);
            StatusMessage = $"PDF export generated ({data.Length} bytes).";
        }
        finally
        {
            IsLoading = false;
        }
    }
}
