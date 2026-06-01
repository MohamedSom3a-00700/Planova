using System.IO;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Planova.Application.Dto;
using Planova.Application.Services;

namespace Planova.UI.ViewModels;

public partial class ReportViewModel : ObservableObject
{
    private readonly IReportService _reportService;

    public ReportViewModel(IReportService reportService)
    {
        _reportService = reportService;
    }

    public ObservableCollection<ProjectSummaryDto> ProjectReports { get; } = new();
    public ObservableCollection<ClientSummaryDto> ClientReports { get; } = new();
    public ObservableCollection<ContractSummaryDto> ContractReports { get; } = new();

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private int _selectedTabIndex;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private bool _hasError;

    [RelayCommand]
    private async Task LoadAsync()
    {
        IsLoading = true;
        HasError = false;

        try
        {
            var projects = await _reportService.GetProjectSummaryAsync();
            ProjectReports.Clear();
            foreach (var p in projects)
                ProjectReports.Add(p);

            var clients = await _reportService.GetClientSummaryAsync();
            ClientReports.Clear();
            foreach (var c in clients)
                ClientReports.Add(c);

            var contracts = await _reportService.GetContractSummaryAsync();
            ContractReports.Clear();
            foreach (var c in contracts)
                ContractReports.Add(c);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to load reports: {ex.Message}";
            HasError = true;
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task ExportProjectsPdfAsync()
    {
        await ExportPdfAsync(_reportService.ExportProjectsPdfAsync, "Projects");
    }

    [RelayCommand]
    private async Task ExportClientsPdfAsync()
    {
        await ExportPdfAsync(_reportService.ExportClientsPdfAsync, "Clients");
    }

    [RelayCommand]
    private async Task ExportContractsPdfAsync()
    {
        await ExportPdfAsync(_reportService.ExportContractsPdfAsync, "Contracts");
    }

    private async Task ExportPdfAsync(Func<CancellationToken, Task<byte[]>> exportFunc, string name)
    {
        IsLoading = true;
        HasError = false;

        try
        {
            var pdf = await exportFunc(default);
            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                FileName = $"{name}Report.pdf",
                Filter = "PDF files (*.pdf)|*.pdf"
            };

            if (dialog.ShowDialog() == true)
            {
                await File.WriteAllBytesAsync(dialog.FileName, pdf);
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to export {name} report: {ex.Message}";
            HasError = true;
        }
        finally
        {
            IsLoading = false;
        }
    }
}
