using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Planova.Application.Services;
using Planova.Boq.Application.Dto;
using Planova.Boq.Domain.Interfaces;
using Planova.Shared.Abstractions;

namespace Planova.UI.ViewModels.Boq;

public partial class BoqTreeViewModel : ObservableObject
{
    private readonly IBoqService _boqService;
    private readonly IBoqSession _session;
    private readonly ICurrentProjectService _currentProjectService;
    private readonly IProjectDocumentService _projectDocumentService;

    public BoqTreeViewModel(IBoqService boqService, IBoqSession session,
        ICurrentProjectService currentProjectService,
        IProjectDocumentService projectDocumentService)
    {
        _boqService = boqService;
        _session = session;
        _currentProjectService = currentProjectService;
        _projectDocumentService = projectDocumentService;
        _session.BoqChanged += OnBoqChanged;
        _currentProjectService.CurrentProjectChanged += OnCurrentProjectChanged;

        if (_currentProjectService.CurrentProject is not null)
        {
            _ = LoadAvailableBoqsAsync(CancellationToken.None);
        }
    }

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    [ObservableProperty]
    private string _boqName = string.Empty;

    [ObservableProperty]
    private decimal _grandTotal;

    [ObservableProperty]
    private Guid _currentBoqId;

    [ObservableProperty]
    private BoqItemDto? _selectedItem;

    [ObservableProperty]
    private BoqSummaryDto? _selectedBoq;

    public ObservableCollection<BoqSummaryDto> AvailableBoqs { get; } = new();
    public ObservableCollection<BoqItemDto> FlatItems { get; } = new();

    public bool HasNoBoqs => AvailableBoqs.Count == 0;

    partial void OnSelectedBoqChanged(BoqSummaryDto? value)
    {
        if (value is not null)
        {
            _ = LoadTreeAsync(value.Id, CancellationToken.None);
        }
    }

    private async void OnCurrentProjectChanged(object? sender, ProjectContext? context)
    {
        if (context is not null)
        {
            try
            {
                await LoadAvailableBoqsAsync(CancellationToken.None);
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error loading BOQs: {ex.Message}";
            }
        }
        else
        {
            ResetState();
            AvailableBoqs.Clear();
            OnPropertyChanged(nameof(HasNoBoqs));
            StatusMessage = string.Empty;
        }
    }

    [RelayCommand]
    private async Task LoadAvailableBoqsAsync(CancellationToken ct)
    {
        var projectId = _session.CurrentProjectId;
        if (projectId is null && _currentProjectService.CurrentProject is not null)
        {
            projectId = GuidFromInt(_currentProjectService.CurrentProject.Id);
        }
        if (projectId is null)
        {
            ResetState();
            AvailableBoqs.Clear();
            OnPropertyChanged(nameof(HasNoBoqs));
            StatusMessage = string.Empty;
            return;
        }

        try
        {
            IsLoading = true;
            var boqs = await _boqService.GetByProjectIdAsync(projectId.Value, ct);
            AvailableBoqs.Clear();
            foreach (var boq in boqs)
            {
                AvailableBoqs.Add(boq);
            }
            OnPropertyChanged(nameof(HasNoBoqs));

            if (boqs.Count > 0 && SelectedBoq is null)
            {
                SelectedBoq = boqs[0];
                StatusMessage = $"Loaded {boqs.Count} BOQ(s)";
            }
            else if (boqs.Count == 0)
            {
                ResetState();
                var projectIntId = _currentProjectService.CurrentProject?.Id;
                if (projectIntId.HasValue)
                {
                    var boqDocs = await _projectDocumentService.GetByTypeAsync(projectIntId.Value, "Boq", ct);
                    if (boqDocs.Any())
                    {
                        StatusMessage = "No BOQs have been imported yet. Go to the Import tab to import from project documents.";
                    }
                    else
                    {
                        StatusMessage = "No BOQs found. Add a BOQ document in Projects, then import from the Import tab.";
                    }
                }
                else
                {
                    StatusMessage = string.Empty;
                }
            }
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void ResetState()
    {
        SelectedBoq = null;
        CurrentBoqId = Guid.Empty;
        FlatItems.Clear();
        BoqName = string.Empty;
        GrandTotal = 0;
    }

    private static Guid GuidFromInt(int value)
    {
        var bytes = new byte[16];
        BitConverter.GetBytes(value).CopyTo(bytes, 0);
        return new Guid(bytes);
    }

    [RelayCommand]
    private async Task LoadTreeAsync(Guid boqId, CancellationToken ct)
    {
        try
        {
            IsLoading = true;
            CurrentBoqId = boqId;

            var boq = await _boqService.GetByIdAsync(boqId, ct);
            BoqName = boq.Name;

            var tree = await _boqService.GetTreeAsync(boqId, ct);
            FlatItems.Clear();
            var flatList = FlattenTree(tree).ToList();
            for (int i = 0; i < flatList.Count; i++)
            {
                FlatItems.Add(flatList[i] with { SortOrder = i + 1 });
            }

            GrandTotal = await _boqService.ComputeSubtotalAsync(boqId, null, ct);
            StatusMessage = $"Loaded {FlatItems.Count} items";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void ToggleExpand(BoqItemDto item)
    {
    }

    [RelayCommand]
    private async Task RefreshAsync(CancellationToken ct)
    {
        if (CurrentBoqId != Guid.Empty)
        {
            await LoadTreeAsync(CurrentBoqId, ct);
        }
    }

    private async void OnBoqChanged(object? sender, Guid boqId)
    {
        try
        {
            await LoadAvailableBoqsAsync(CancellationToken.None);

            var summary = AvailableBoqs.FirstOrDefault(b => b.Id == boqId);
            if (summary is not null)
            {
                SelectedBoq = summary;
            }
            else
            {
                await LoadTreeAsync(boqId, CancellationToken.None);
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error loading BOQ: {ex.Message}";
        }
    }

    private static IEnumerable<BoqItemDto> FlattenTree(IReadOnlyList<BoqItemDto> items, int depth = 0)
    {
        foreach (var item in items)
        {
            yield return item with { Level = depth };
            if (item.Children != null)
            {
                foreach (var child in FlattenTree(item.Children, depth + 1))
                {
                    yield return child;
                }
            }
        }
    }
}
