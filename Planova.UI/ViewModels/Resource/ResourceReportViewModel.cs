using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using Planova.Resource.Application.Dto;
using Planova.Resource.Domain.Interfaces;

namespace Planova.UI.ViewModels.Resource;

public partial class ResourceReportViewModel : ObservableObject
{
    private readonly IResourceReportService _reportService;

    [ObservableProperty]
    private int _projectId;

    [ObservableProperty]
    private ReportType _selectedReportType;

    [ObservableProperty]
    private ResourceUsageReportDto? _usageReport;

    [ObservableProperty]
    private ResourceCostReportDto? _costReport;

    [ObservableProperty]
    private bool _isGenerating;

    [ObservableProperty]
    private bool _hasData;

    public ResourceReportViewModel(IResourceReportService reportService)
    {
        _reportService = reportService;
    }

    [RelayCommand]
    private async Task GenerateReport()
    {
        if (ProjectId <= 0) return;

        IsGenerating = true;
        try
        {
            if (SelectedReportType == ReportType.UsageSummary)
            {
                UsageReport = await _reportService.GenerateUsageSummaryAsync(ProjectId);
                CostReport = null;
            }
            else
            {
                CostReport = await _reportService.GenerateCostReportAsync(ProjectId);
                UsageReport = null;
            }
            HasData = true;
        }
        finally
        {
            IsGenerating = false;
        }
    }

    [RelayCommand]
    private async Task ExportToExcel()
    {
        if (ProjectId <= 0) return;

        var dialog = new SaveFileDialog
        {
            Filter = "Excel Files|*.xlsx",
            Title = "Export Report",
            FileName = $"resource-report-{SelectedReportType}.xlsx"
        };
        if (dialog.ShowDialog() != true) return;

        var data = await _reportService.ExportToExcelAsync(ProjectId, SelectedReportType);
        await File.WriteAllBytesAsync(dialog.FileName, data);
    }

    [RelayCommand]
    private async Task ExportToPdf()
    {
        if (ProjectId <= 0) return;

        var dialog = new SaveFileDialog
        {
            Filter = "PDF Files|*.pdf",
            Title = "Export Report as PDF",
            FileName = $"resource-report-{SelectedReportType}.pdf"
        };
        if (dialog.ShowDialog() != true) return;

        var data = await _reportService.ExportToPdfAsync(ProjectId, SelectedReportType);
        await File.WriteAllBytesAsync(dialog.FileName, data);
    }
}
