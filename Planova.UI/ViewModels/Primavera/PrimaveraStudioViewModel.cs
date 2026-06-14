using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Planova.Shared.Abstractions;

namespace Planova.UI.ViewModels.Primavera;

public partial class PrimaveraStudioTab : ObservableObject
{
    [ObservableProperty]
    private string _header = string.Empty;

    [ObservableProperty]
    private object _content = null!;

    public PrimaveraStudioTab(string header, object content)
    {
        _header = header;
        _content = content;
    }

    public override string ToString() => Header;
}

public partial class PrimaveraStudioViewModel : ObservableObject
{
    private readonly ICurrentProjectService _currentProjectService;

    public PrimaveraStudioViewModel(
        PrimaveraImportViewModel importVm,
        PrimaveraWorkspaceViewModel workspaceVm,
        PrimaveraValidationViewModel validationVm,
        PrimaveraRepairViewModel repairVm,
        PrimaveraExportViewModel exportVm,
        ICurrentProjectService currentProjectService)
    {
        _currentProjectService = currentProjectService;
        ImportViewModel = importVm;
        WorkspaceViewModel = workspaceVm;
        ValidationViewModel = validationVm;
        RepairViewModel = repairVm;
        ExportViewModel = exportVm;

        _currentProjectService.CurrentProjectChanged += OnCurrentProjectChanged;

        if (_currentProjectService.CurrentProject is { } project)
            _ = LoadAsync(project.Id);
    }

    [ObservableProperty]
    private PrimaveraStudioTab? _selectedTab;

    [ObservableProperty]
    private bool _isProjectActive;

    public ObservableCollection<PrimaveraStudioTab> Tabs { get; } = new();

    public PrimaveraImportViewModel ImportViewModel { get; }
    public PrimaveraWorkspaceViewModel WorkspaceViewModel { get; }
    public PrimaveraValidationViewModel ValidationViewModel { get; }
    public PrimaveraRepairViewModel RepairViewModel { get; }
    public PrimaveraExportViewModel ExportViewModel { get; }

    private async void OnCurrentProjectChanged(object? sender, ProjectContext? context)
    {
        if (context is not null)
        {
            await LoadAsync(context.Id);
        }
    }

    public async Task LoadAsync(int projectId, CancellationToken ct = default)
    {
        await WorkspaceViewModel.LoadAsync(projectId, ct);
    }

    [RelayCommand]
    private async Task RunValidationFromToolbarAsync()
    {
        if (Tabs.Count > 2)
            SelectedTab = Tabs[2];
        await ValidationViewModel.RunValidationCommand.ExecuteAsync(null);
    }
}
