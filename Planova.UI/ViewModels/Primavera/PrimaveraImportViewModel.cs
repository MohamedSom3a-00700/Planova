using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Planova.Primavera.Application.Dto;
using Planova.Primavera.Domain.Interfaces;

namespace Planova.UI.ViewModels.Primavera;

public enum ImportLogEntrySeverity
{
    Info,
    Warning,
    Error
}

public partial class ImportLogEntry : ObservableObject
{
    [ObservableProperty]
    private string _timestamp = string.Empty;

    [ObservableProperty]
    private string _message = string.Empty;

    [ObservableProperty]
    private ImportLogEntrySeverity _severity = ImportLogEntrySeverity.Info;

    public ImportLogEntry(string message, ImportLogEntrySeverity severity = ImportLogEntrySeverity.Info)
    {
        Timestamp = DateTime.Now.ToString("HH:mm:ss");
        Message = message;
        Severity = severity;
    }
}

public partial class PrimaveraImportViewModel : ObservableObject
{
    private readonly IPrimaveraImportService _importService;

    public PrimaveraImportViewModel(IPrimaveraImportService importService)
    {
        _importService = importService;
        _ = LoadImportedSessionsAsync();
    }

    [ObservableProperty]
    private string _selectedFilePath = string.Empty;

    [ObservableProperty]
    private bool _isPreviewVisible;

    [ObservableProperty]
    private bool _isImporting;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    [ObservableProperty]
    private XerImportPreviewDto? _preview;

    public ObservableCollection<PrimaveraValidationIssueDto> ValidationIssues { get; } = new();

    public ObservableCollection<ImportLogEntry> ImportLog { get; } = new();

    public ObservableCollection<XerImportSessionDto> ImportedSessions { get; } = new();

    [ObservableProperty]
    private XerImportSessionDto? _selectedImportedSession;

    public async Task LoadImportedSessionsAsync()
    {
        try
        {
            var sessions = await _importService.GetImportedSessionsAsync();
            ImportedSessions.Clear();
            foreach (var s in sessions)
                ImportedSessions.Add(s);
        }
        catch
        {
            // Silently fail - sessions list will just be empty
        }
    }

    [RelayCommand]
    private async Task BrowseFileAsync()
    {
        var dialog = new Microsoft.Win32.OpenFileDialog
        {
            Filter = "XER Files (*.xer)|*.xer|All Files (*.*)|*.*",
            Title = "Select Primavera XER File"
        };

        if (dialog.ShowDialog() == true)
        {
            SelectedFilePath = dialog.FileName;
            ImportLog.Clear();
            ImportLog.Add(new ImportLogEntry($"Selected file: {dialog.FileName}"));
            await PreviewFileAsync();
        }
    }

    [RelayCommand]
    private async Task PreviewFileAsync()
    {
        if (string.IsNullOrEmpty(SelectedFilePath)) return;

        IsImporting = true;
        IsPreviewVisible = false;
        StatusMessage = "Parsing XER file...";
        ImportLog.Add(new ImportLogEntry("Parsing XER file..."));

        try
        {
            Preview = await _importService.PreviewAsync(SelectedFilePath);
            IsPreviewVisible = true;
            ValidationIssues.Clear();

            ImportLog.Add(new ImportLogEntry($"Parsed {Preview.FileName} ({Preview.FileSize:N0} bytes)"));

            if (Preview.RowCounts?.Count > 0)
            {
                ImportLog.Add(new ImportLogEntry($"Tables found: {Preview.RowCounts.Count}"));
                foreach (var kv in Preview.RowCounts)
                    ImportLog.Add(new ImportLogEntry($"  {kv.Key}: {kv.Value} rows"));
            }

            if (Preview.ValidationIssues != null)
            {
                foreach (var issue in Preview.ValidationIssues)
                {
                    ValidationIssues.Add(issue);
                    var severity = issue.Severity == "Error" ? ImportLogEntrySeverity.Error
                        : issue.Severity == "Warning" ? ImportLogEntrySeverity.Warning
                        : ImportLogEntrySeverity.Info;
                    ImportLog.Add(new ImportLogEntry($"[{issue.Severity}] {issue.Description}", severity));
                }
            }

            if (Preview.CanCommit)
            {
                StatusMessage = "Preview ready. Review and commit.";
                ImportLog.Add(new ImportLogEntry("Preview ready. Review and commit."));
            }
            else
            {
                StatusMessage = "Validation errors found. Cannot commit.";
                ImportLog.Add(new ImportLogEntry("Validation errors found. Cannot commit.", ImportLogEntrySeverity.Error));
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
            ImportLog.Add(new ImportLogEntry($"Error: {ex.Message}", ImportLogEntrySeverity.Error));
        }
        finally
        {
            IsImporting = false;
        }
    }

    [RelayCommand]
    private async Task CommitImportAsync()
    {
        if (Preview == null)
        {
            ImportLog.Add(new ImportLogEntry("No preview available to commit.", ImportLogEntrySeverity.Warning));
            return;
        }

        IsImporting = true;
        StatusMessage = "Committing import...";
        ImportLog.Add(new ImportLogEntry("Committing import..."));

        try
        {
            var result = await _importService.CommitAsync(Preview.SessionId, false);

            if (result.Success)
            {
                StatusMessage = "Import completed successfully.";
                ImportLog.Add(new ImportLogEntry("Import completed successfully."));
                await LoadImportedSessionsAsync();
            }
            else
            {
                StatusMessage = $"Import failed: {result.ErrorMessage}";
                ImportLog.Add(new ImportLogEntry($"Import failed: {result.ErrorMessage}", ImportLogEntrySeverity.Error));
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
            ImportLog.Add(new ImportLogEntry($"Error: {ex.Message}", ImportLogEntrySeverity.Error));
        }
        finally
        {
            IsImporting = false;
        }
    }

    [RelayCommand]
    private void CancelImport()
    {
        SelectedFilePath = string.Empty;
        IsPreviewVisible = false;
        Preview = null;
        ValidationIssues.Clear();
        StatusMessage = string.Empty;
        ImportLog.Add(new ImportLogEntry("Import cancelled."));
    }
}