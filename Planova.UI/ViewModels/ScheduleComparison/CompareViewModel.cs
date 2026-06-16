using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using Planova.Primavera.Domain.Interfaces;
using Planova.ScheduleComparison.Application.Dto;
using Planova.ScheduleComparison.Application.Mappings;
using Planova.ScheduleComparison.Domain.Entities;
using Planova.ScheduleComparison.Domain.Enums;
using Planova.ScheduleComparison.Domain.Interfaces;
using Planova.Shared.Abstractions;

namespace Planova.UI.ViewModels.ScheduleComparison;

public class ScheduleSourceItem
{
    public string Id { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string Kind { get; set; } = string.Empty;
    public Guid? SnapshotId { get; set; }
    public Guid? ImportSessionId { get; set; }
}

public partial class SelectableSession : ObservableObject
{
    [ObservableProperty]
    private bool _isSelected;

    public Guid Id => Session.Id;
    public string SourceLabel => Session.SourceLabel;
    public string TargetLabel => Session.TargetLabel;
    public string Mode => Session.Mode;
    public string State => Session.State;
    public string? Error => Session.Error;
    public DateTime CreatedAt => Session.CreatedAt;
    public DateTime? CompletedAt => Session.CompletedAt;

    public ComparisonSessionDto Session { get; }

    public SelectableSession(ComparisonSessionDto session)
    {
        Session = session;
    }
}

public partial class CompareViewModel : ObservableObject
{
    private readonly IScheduleComparisonService _comparisonService;
    private readonly IScheduleSnapshotService _snapshotService;
    private readonly ICurrentProjectService _currentProjectService;
    private readonly IPrimaveraImportService? _importService;

    public Func<string, Task>? OnComparisonCompleted { get; set; }

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private bool _isRunning;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    [ObservableProperty]
    private ObservableCollection<ScheduleSourceItem> _sourceItems = new();

    [ObservableProperty]
    private ObservableCollection<ScheduleSourceItem> _targetItems = new();

    [ObservableProperty]
    private ScheduleSourceItem? _selectedSource;

    [ObservableProperty]
    private ScheduleSourceItem? _selectedTarget;

    [ObservableProperty]
    private bool _hasValidationError;

    [ObservableProperty]
    private string _validationErrorMessage = string.Empty;

    partial void OnSelectedSourceChanged(ScheduleSourceItem? value)
    {
        ClearValidation();
    }

    partial void OnSelectedTargetChanged(ScheduleSourceItem? value)
    {
        ClearValidation();
    }

    private void ClearValidation()
    {
        HasValidationError = false;
        ValidationErrorMessage = string.Empty;
    }

    [ObservableProperty]
    private ObservableCollection<SelectableSession> _historySessions = new();

    [ObservableProperty]
    private SelectableSession? _selectedHistorySession;

    [ObservableProperty]
    private ComparisonSummaryDto? _currentSummary;

    public CompareViewModel(
        IScheduleComparisonService comparisonService,
        IScheduleSnapshotService snapshotService,
        ICurrentProjectService currentProjectService,
        IServiceProvider serviceProvider)
    {
        _comparisonService = comparisonService;
        _snapshotService = snapshotService;
        _currentProjectService = currentProjectService;
        _importService = serviceProvider.GetService<IPrimaveraImportService>();
        _currentProjectService.CurrentProjectChanged += OnCurrentProjectChanged;
        _ = InitializeAsync();
    }

    private async void OnCurrentProjectChanged(object? sender, ProjectContext? project)
    {
        await InitializeAsync();
    }

    private async Task InitializeAsync()
    {
        var projectId = _currentProjectService.CurrentProject?.Id;
        if (projectId == null)
            return;

        IsLoading = true;
        try
        {
            var items = await LoadSourceItemsAsync();
            SourceItems = new ObservableCollection<ScheduleSourceItem>(items);
            TargetItems = new ObservableCollection<ScheduleSourceItem>(items);

            await LoadHistoryAsync(projectId.Value);
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task<List<ScheduleSourceItem>> LoadSourceItemsAsync()
    {
        var projectId = _currentProjectService.CurrentProject?.Id;
        if (projectId == null)
            return new List<ScheduleSourceItem>();

        var snapshots = await _snapshotService.ListSnapshotsAsync(projectId.Value, default);

        var items = new List<ScheduleSourceItem>();

        foreach (var s in snapshots)
        {
            items.Add(new ScheduleSourceItem
            {
                Id = $"snapshot:{s.Id}",
                Label = $"Snapshot: {s.Label}",
                Kind = "Snapshot",
                SnapshotId = s.Id
            });
        }

        if (_importService != null)
        {
            try
            {
                var sessions = await _importService.GetImportedSessionsByProjectAsync(projectId.Value, default);
                var committed = sessions.Where(s =>
                    string.Equals(s.Status, "Committed", StringComparison.OrdinalIgnoreCase) &&
                    !string.IsNullOrEmpty(s.ParsedDataJson));
                foreach (var session in committed)
                {
                    var typeLabel = session.ImportType ?? "Unknown";
                    items.Add(new ScheduleSourceItem
                    {
                        Id = $"xer:{session.Id}",
                        Label = $"XER: {session.SourceFileName} ({typeLabel})",
                        Kind = "XerImport",
                        ImportSessionId = session.Id
                    });
                }
            }
            catch
            {
                // XER imports unavailable — show snapshots only
            }
        }

        return items;
    }

    private async Task LoadHistoryAsync(int projectId)
    {
        var sessions = await _comparisonService.ListSessionsAsync(projectId, default);
        HistorySessions = new ObservableCollection<SelectableSession>(
            sessions.ToDtoList()
                .Where(s => !string.Equals(s.State, "Cancelled", StringComparison.OrdinalIgnoreCase))
                .Select(s => new SelectableSession(s)));
    }

    [RelayCommand]
    private async Task RunComparisonAsync(CancellationToken ct)
    {
        if (SelectedSource == null && SelectedTarget == null)
        {
            HasValidationError = true;
            ValidationErrorMessage = "Please select both a source and a target schedule to compare.";
            return;
        }

        if (SelectedSource == null)
        {
            HasValidationError = true;
            ValidationErrorMessage = "Please select a source schedule.";
            return;
        }

        if (SelectedTarget == null)
        {
            HasValidationError = true;
            ValidationErrorMessage = "Please select a target schedule.";
            return;
        }

        if (SelectedSource.Id == SelectedTarget.Id)
        {
            HasValidationError = true;
            ValidationErrorMessage = "Source and target must be different schedules.";
            return;
        }

        ClearValidation();
        IsRunning = true;
        StatusMessage = "Running comparison...";

        try
        {
            var scopes = new List<ComparisonScope>
            {
                ComparisonScope.Activities,
                ComparisonScope.Logic,
                ComparisonScope.Resources,
                ComparisonScope.CriticalPath,
                ComparisonScope.Float
            };

            var projectId = _currentProjectService.CurrentProject?.Id ?? 0;

            var session = await _comparisonService.CompareAsync(
                projectId,
                SelectedSource.SnapshotId,
                SelectedTarget.SnapshotId,
                SelectedSource.Kind,
                SelectedTarget.Kind,
                SelectedSource.Label,
                SelectedTarget.Label,
                scopes,
                sourceImportSessionId: SelectedSource.ImportSessionId,
                targetImportSessionId: SelectedTarget.ImportSessionId,
                ct: ct);

            StatusMessage = "Comparison completed successfully.";

            if (OnComparisonCompleted != null && session.ResultJson != null)
                await OnComparisonCompleted(session.ResultJson);

            await LoadHistoryAsync(projectId);
        }
        catch (OperationCanceledException)
        {
            StatusMessage = "Comparison was cancelled.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Comparison failed: {ex.Message}";
        }
        finally
        {
            IsRunning = false;
        }
    }

    [RelayCommand]
    private void SelectAllHistory()
    {
        foreach (var s in HistorySessions)
            s.IsSelected = true;
    }

    [RelayCommand]
    private void DeselectAllHistory()
    {
        foreach (var s in HistorySessions)
            s.IsSelected = false;
    }

    [RelayCommand]
    private async Task DeleteSelectedHistoryAsync()
    {
        var selected = HistorySessions.Where(s => s.IsSelected).ToList();
        if (selected.Count == 0)
        {
            StatusMessage = "No sessions selected for deletion.";
            return;
        }

        var ids = selected.Select(s => s.Id).ToList();

        try
        {
            foreach (var id in ids)
                await _comparisonService.SoftDeleteSessionAsync(id, default);

            foreach (var s in selected)
                HistorySessions.Remove(s);

            SelectedHistorySession = null;
            StatusMessage = $"Deleted {ids.Count} session(s).";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Delete failed: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task LoadHistorySessionAsync()
    {
        var selected = HistorySessions.FirstOrDefault(s => s.IsSelected) ?? SelectedHistorySession;
        if (selected == null)
            return;

        StatusMessage = $"Loading session: {selected.SourceLabel} → {selected.TargetLabel}...";

        var session = await _comparisonService.GetSessionAsync(selected.Id, default);
        if (session?.ResultJson != null && OnComparisonCompleted != null)
            await OnComparisonCompleted(session.ResultJson);

        StatusMessage = $"Loaded: {selected.SourceLabel} → {selected.TargetLabel}";
    }
}
