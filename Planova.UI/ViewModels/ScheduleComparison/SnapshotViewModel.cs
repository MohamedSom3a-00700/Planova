using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Planova.ScheduleComparison.Domain.Entities;
using Planova.ScheduleComparison.Domain.Interfaces;

namespace Planova.UI.ViewModels.ScheduleComparison;

public partial class SnapshotViewModel : ObservableObject
{
    private readonly IScheduleSnapshotService _snapshotService;

    [ObservableProperty]
    private ObservableCollection<ScheduleSnapshot> _snapshots = new();

    [ObservableProperty]
    private string _newSnapshotLabel = string.Empty;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private bool _isCapturing;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    public SnapshotViewModel(IScheduleSnapshotService snapshotService)
    {
        _snapshotService = snapshotService;
    }

    [RelayCommand]
    private async Task LoadSnapshotsAsync(int projectId, CancellationToken ct)
    {
        IsLoading = true;
        try
        {
            var snapshots = await _snapshotService.ListSnapshotsAsync(projectId, ct);
            Snapshots = new ObservableCollection<ScheduleSnapshot>(snapshots);
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task CaptureSnapshotAsync(int projectId, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(NewSnapshotLabel))
            return;

        IsCapturing = true;
        StatusMessage = "Capturing snapshot...";
        try
        {
            var snapshot = await _snapshotService.CaptureSnapshotAsync(projectId, NewSnapshotLabel.Trim(), ct);
            Snapshots.Insert(0, snapshot);
            NewSnapshotLabel = string.Empty;
            StatusMessage = $"Snapshot '{snapshot.Label}' captured.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Capture failed: {ex.Message}";
        }
        finally
        {
            IsCapturing = false;
        }
    }

    [RelayCommand]
    private async Task DeleteSnapshotAsync(ScheduleSnapshot snapshot, CancellationToken ct)
    {
        await _snapshotService.DeleteSnapshotAsync(snapshot.Id, ct);
        Snapshots.Remove(snapshot);
    }
}
