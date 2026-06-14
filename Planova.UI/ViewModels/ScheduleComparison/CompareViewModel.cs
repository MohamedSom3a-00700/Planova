using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using Planova.Primavera.Domain.Interfaces;
using Planova.ScheduleComparison.Domain.Entities;
using Planova.ScheduleComparison.Domain.Enums;
using Planova.ScheduleComparison.Domain.Interfaces;
using Planova.Shared.Abstractions;

namespace Planova.UI.ViewModels.ScheduleComparison;

public partial class CompareViewModel : ObservableObject
{
    private readonly IScheduleComparisonService _comparisonService;
    private readonly IScheduleSnapshotService _snapshotService;
    private readonly ICurrentProjectService _currentProjectService;
    private readonly IServiceProvider _serviceProvider;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private bool _isRunning;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    [ObservableProperty]
    private ObservableCollection<ScheduleSnapshot> _sourceSnapshots = new();

    [ObservableProperty]
    private ObservableCollection<ScheduleSnapshot> _targetSnapshots = new();

    [ObservableProperty]
    private ScheduleSnapshot? _selectedSource;

    [ObservableProperty]
    private ScheduleSnapshot? _selectedTarget;

    [ObservableProperty]
    private bool _isPrimaveraAvailable;

    [ObservableProperty]
    private bool _usePrimaveraSource;

    [ObservableProperty]
    private bool _usePrimaveraTarget;

    public CompareViewModel(
        IScheduleComparisonService comparisonService,
        IScheduleSnapshotService snapshotService,
        ICurrentProjectService currentProjectService,
        IServiceProvider serviceProvider)
    {
        _comparisonService = comparisonService;
        _snapshotService = snapshotService;
        _currentProjectService = currentProjectService;
        _serviceProvider = serviceProvider;
        IsPrimaveraAvailable = _serviceProvider.GetService<IPrimaveraWorkspaceService>() != null;
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
            var snapshots = await _snapshotService.ListSnapshotsAsync(projectId.Value, default);
            SourceSnapshots = new ObservableCollection<ScheduleSnapshot>(snapshots);
            TargetSnapshots = new ObservableCollection<ScheduleSnapshot>(snapshots);
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task RunComparisonAsync(CancellationToken ct)
    {
        if (!UsePrimaveraSource && SelectedSource == null)
            return;
        if (!UsePrimaveraTarget && SelectedTarget == null)
            return;

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

            var sourceKind = UsePrimaveraSource ? "Primavera" : "Snapshot";
            var targetKind = UsePrimaveraTarget ? "Primavera" : "Snapshot";
            var sourceLabel = UsePrimaveraSource ? "Primavera Import" : (SelectedSource?.Label ?? "None");
            var targetLabel = UsePrimaveraTarget ? "Primavera Import" : (SelectedTarget?.Label ?? "None");

            await _comparisonService.CompareAsync(
                projectId,
                UsePrimaveraSource ? null : SelectedSource?.Id,
                UsePrimaveraTarget ? null : SelectedTarget?.Id,
                sourceKind,
                targetKind,
                sourceLabel,
                targetLabel,
                scopes,
                ct: ct);

            StatusMessage = "Comparison completed successfully.";
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
}
