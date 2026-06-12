using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Planova.Application.Services;
using Planova.Shared.Abstractions;

namespace Planova.UI.ViewModels.Shared;

public partial class DocumentValidationBannerViewModel : ObservableObject
{
    private readonly IProjectDocumentService _docService;
    private readonly ICurrentProjectService _currentProject;

    public DocumentValidationBannerViewModel(
        IProjectDocumentService docService,
        ICurrentProjectService currentProject)
    {
        _docService = docService;
        _currentProject = currentProject;
        _currentProject.CurrentProjectChanged += OnCurrentProjectChanged;
    }

    [ObservableProperty]
    private bool _hasWarning;

    [ObservableProperty]
    private string _warningMessage = string.Empty;

    public string[] RequiredTypes { get; set; } = Array.Empty<string>();

    private async void OnCurrentProjectChanged(object? sender, ProjectContext? context)
    {
        if (context != null)
            await CheckAsync(RequiredTypes);
        else
            HasWarning = false;
    }

    [RelayCommand]
    private async Task LoadAsync()
    {
        if (_currentProject.CurrentProject != null)
            await CheckAsync(RequiredTypes);
    }

    [RelayCommand]
    private void GoToProjectDocuments()
    {
    }

    public async Task CheckAsync(string[] requiredTypes, CancellationToken ct = default)
    {
        var projectId = _currentProject.CurrentProject?.Id;
        if (projectId == null || requiredTypes.Length == 0)
        {
            HasWarning = false;
            return;
        }

        var missing = new List<string>();
        foreach (var type in requiredTypes)
        {
            var docs = await _docService.GetByTypeAsync(projectId.Value, type, ct);
            if (!docs.Any())
                missing.Add(type);
        }

        if (missing.Count > 0)
        {
            HasWarning = true;
            WarningMessage = $"Missing required documents: {string.Join(", ", missing)}";
        }
        else
        {
            HasWarning = false;
            WarningMessage = string.Empty;
        }
    }
}
