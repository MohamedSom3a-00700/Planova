using System.Collections.ObjectModel;
using System.Dynamic;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using Planova.Excel.Models;
using Planova.Excel.Services;

namespace Planova.UI.ViewModels.Excel;

public partial class WorkbookBrowserViewModel : ObservableObject
{
    private readonly IWorkbookPreviewService _previewService;

    public WorkbookBrowserViewModel(IWorkbookPreviewService previewService)
    {
        _previewService = previewService;
    }

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private bool _hasError;

    [ObservableProperty]
    private string _filePath = string.Empty;

    [ObservableProperty]
    private WorkbookInfo? _workbookInfo;

    [ObservableProperty]
    private WorksheetInfo? _selectedWorksheet;

    [ObservableProperty]
    private PreviewData? _previewData;

    [ObservableProperty]
    private int _currentPage = 1;

    [ObservableProperty]
    private int _pageSize = 50;

    [ObservableProperty]
    private int _totalPages = 1;

    [ObservableProperty]
    private string _searchQuery = string.Empty;

    public ObservableCollection<string> SearchResults { get; } = new();
    public ObservableCollection<ExpandoObject> PreviewRows { get; } = new();

    [RelayCommand]
    private async Task BrowseAsync()
    {
        var dialog = new OpenFileDialog
        {
            Filter = "Excel Workbooks|*.xlsx;*.xlsm|All Files|*.*",
            Title = "Select Excel Workbook"
        };

        if (dialog.ShowDialog() != true) return;

        FilePath = dialog.FileName;

        IsLoading = true;
        HasError = false;

        try
        {
            WorkbookInfo = await _previewService.GetWorkbookInfoAsync(FilePath, CancellationToken.None);
            CurrentPage = 1;
            TotalPages = 1;
            PreviewData = null;
            PreviewRows.Clear();
            SearchResults.Clear();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to open workbook: {ex.Message}";
            HasError = true;
        }
        finally
        {
            IsLoading = false;
        }
    }

    partial void OnSelectedWorksheetChanged(WorksheetInfo? value)
    {
        if (value is null) return;
        CurrentPage = 1;
        _ = LoadPreviewAsync();
    }

    [RelayCommand]
    private async Task NextPageAsync()
    {
        if (CurrentPage < TotalPages)
        {
            CurrentPage++;
            await LoadPreviewAsync();
        }
    }

    [RelayCommand]
    private async Task PreviousPageAsync()
    {
        if (CurrentPage > 1)
        {
            CurrentPage--;
            await LoadPreviewAsync();
        }
    }

    [RelayCommand]
    private async Task GoToPageAsync(int? page)
    {
        if (page is null || page < 1 || page > TotalPages) return;
        CurrentPage = page.Value;
        await LoadPreviewAsync();
    }

    [RelayCommand]
    private async Task SearchWorksheetAsync()
    {
        if (string.IsNullOrWhiteSpace(FilePath) || string.IsNullOrWhiteSpace(SearchQuery))
            return;

        IsLoading = true;
        try
        {
            SearchResults.Clear();
            await foreach (var name in _previewService.SearchAsync(FilePath, SearchQuery, CancellationToken.None))
            {
                SearchResults.Add(name);
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Search failed: {ex.Message}";
            HasError = true;
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task LoadPreviewAsync()
    {
        if (string.IsNullOrWhiteSpace(FilePath) || SelectedWorksheet is null) return;

        IsLoading = true;
        HasError = false;

        try
        {
            PreviewData = await _previewService.GetPreviewAsync(
                FilePath, SelectedWorksheet.Name, CurrentPage, PageSize, CancellationToken.None);

            if (PreviewData is not null)
            {
                TotalPages = Math.Max(1, (int)Math.Ceiling((double)PreviewData.TotalRowCount / PageSize));
                UpdatePreviewRows(PreviewData);
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to load preview: {ex.Message}";
            HasError = true;
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void UpdatePreviewRows(PreviewData preview)
    {
        PreviewRows.Clear();
        if (preview.Rows is null) return;
        foreach (var row in preview.Rows)
        {
            var expando = new ExpandoObject();
            var dict = (IDictionary<string, object?>)expando;
            foreach (var kvp in row)
                dict[kvp.Key] = kvp.Value;
            PreviewRows.Add(expando);
        }
    }
}
