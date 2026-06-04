using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Planova.Boq.Application.Services;
using Planova.Boq.Domain.Enums;
using Planova.Boq.Domain.Interfaces;
using Microsoft.Win32;

namespace Planova.UI.ViewModels.Boq;

public partial class BoqReportViewModel : ObservableObject
{
    private readonly IBoqReportService _reportService;
    private readonly IBoqSession _session;

    public BoqReportViewModel(IBoqReportService reportService, IBoqSession session)
    {
        _reportService = reportService;
        _session = session;
        _session.BoqChanged += OnBoqChanged;
    }

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private bool _hasBoq;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    [ObservableProperty]
    private string _boqName = string.Empty;

    [ObservableProperty]
    private ReportType _selectedReportType = ReportType.Summary;

    [ObservableProperty]
    private ReportFormat _selectedFormat = ReportFormat.Pdf;

    [ObservableProperty]
    private byte[]? _lastGeneratedData;

    [ObservableProperty]
    private long _lastGeneratedSize;

    public List<ReportType> ReportTypeOptions { get; } = [ReportType.Summary, ReportType.Itemized];
    public List<ReportFormat> ReportFormatOptions { get; } = [ReportFormat.Pdf, ReportFormat.Excel];

    private async void OnBoqChanged(object? sender, Guid boqId)
    {
        HasBoq = true;
        LastGeneratedData = null;
        LastGeneratedSize = 0;
        StatusMessage = "Ready to generate report";
    }

    [RelayCommand]
    private async Task GenerateReportAsync(CancellationToken ct)
    {
        var boqId = _session.CurrentBoqId;
        if (boqId == null || boqId == Guid.Empty) return;

        try
        {
            IsLoading = true;
            StatusMessage = "Generating report...";

            var data = SelectedReportType switch
            {
                ReportType.Summary => await _reportService.GenerateSummaryReportAsync(boqId.Value, SelectedFormat, ct),
                ReportType.Itemized => await _reportService.GenerateItemizedReportAsync(boqId.Value, SelectedFormat, ct),
                _ => throw new ArgumentOutOfRangeException()
            };

            LastGeneratedData = data;
            LastGeneratedSize = data.Length;
            StatusMessage = $"Report generated: {data.Length} bytes ({SelectedFormat})";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task SaveReportAsync(CancellationToken ct)
    {
        if (LastGeneratedData == null) return;

        var boqId = _session.CurrentBoqId;
        if (boqId == null || boqId == Guid.Empty) return;

        var extension = SelectedFormat == ReportFormat.Pdf ? ".pdf" : ".xlsx";
        var dialog = new SaveFileDialog
        {
            Filter = SelectedFormat == ReportFormat.Pdf
                ? "PDF Files (*.pdf)|*.pdf"
                : "Excel Files (*.xlsx)|*.xlsx",
            DefaultExt = extension,
            FileName = $"BOQ_Report_{SelectedReportType}_{DateTime.Now:yyyyMMdd}{extension}"
        };

        if (dialog.ShowDialog() == true)
        {
            try
            {
                await _reportService.SaveReportAsync(boqId.Value, SelectedReportType, SelectedFormat, dialog.FileName, ct);
                StatusMessage = $"Saved to: {dialog.FileName}";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Save error: {ex.Message}";
            }
        }
    }
}
