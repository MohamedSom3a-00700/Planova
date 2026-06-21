using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Planova.Application.Dto;
using Planova.Application.Services;

namespace Planova.UI.ViewModels.Projects;

public sealed partial class ProjectListViewModel : ObservableObject
{
    private readonly IProjectService _projectService;
    private List<ProjectSummaryDto> _allProjects = new();

    public ProjectListViewModel(IProjectService projectService)
    {
        _projectService = projectService;
    }

    public System.Windows.Input.ICommand? NewProjectCommand { get; set; }
    public System.Windows.Input.ICommand? EditProjectCommand { get; set; }
    public System.Windows.Input.ICommand? DeleteProjectCommand { get; set; }

    public ObservableCollection<ProjectListItem> Projects { get; } = new();
    public ObservableCollection<string> StatusFilters { get; } = new()
    {
        "All", "Draft", "Under Review", "Approved", "In Progress", "On Hold", "Completed", "Cancelled"
    };

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private bool _isEmpty = true;

    [ObservableProperty]
    private bool _hasError;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private ProjectListItem? _selectedProject;

    [ObservableProperty]
    private string _searchQuery = string.Empty;

    [ObservableProperty]
    private string _selectedStatusFilter = "All";

    partial void OnSearchQueryChanged(string value)
    {
        ApplyFilters();
    }

    partial void OnSelectedStatusFilterChanged(string value)
    {
        ApplyFilters();
    }

    [RelayCommand]
    private async Task LoadAsync(CancellationToken ct)
    {
        IsLoading = true;
        HasError = false;

        try
        {
            _allProjects = (await _projectService.GetAllAsync(ct)).ToList();
            ApplyFilters();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to load projects: {ex.Message}";
            HasError = true;
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void OpenProject(ProjectListItem? item)
    {
        if (item is null) return;
        foreach (var p in Projects)
            p.IsSelected = false;
        item.IsSelected = true;
        SelectedProject = item;
    }

    [RelayCommand]
    private void EditSelectedProject(ProjectListItem? item)
    {
        if (item is null) return;
        SelectedProject = item;
        EditProjectCommand?.Execute(null);
    }

    [RelayCommand]
    private void DeleteSelectedProject(ProjectListItem? item)
    {
        if (item is null) return;
        SelectedProject = item;
        DeleteProjectCommand?.Execute(null);
    }

    [RelayCommand]
    private void Search()
    {
        ApplyFilters();
    }

    private void ApplyFilters()
    {
        var filtered = _allProjects.AsEnumerable();

        if (SelectedStatusFilter != "All")
        {
            filtered = filtered.Where(p => string.Equals(p.Status, SelectedStatusFilter, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(SearchQuery))
        {
            var query = SearchQuery.Trim();
            filtered = filtered.Where(p =>
                p.Code.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                p.Name.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                (p.ClientName?.Contains(query, StringComparison.OrdinalIgnoreCase) == true) ||
                (p.ContractorName?.Contains(query, StringComparison.OrdinalIgnoreCase) == true) ||
                p.Status.Contains(query, StringComparison.OrdinalIgnoreCase));
        }

        Projects.Clear();
        foreach (var p in filtered)
        {
            Projects.Add(new ProjectListItem(p));
        }

        IsEmpty = Projects.Count == 0;
    }
}

public sealed partial class ProjectListItem : ObservableObject
{
    public int Id { get; }
    public string Code { get; }
    public string Name { get; }
    public string Status { get; }
    public string? ClientName { get; }
    public DateTime? StartDate { get; }
    public DateTime? FinishDate { get; }
    public DateTime UpdatedAt { get; }
    public string? ContractorName { get; }
    public int DocumentCount { get; }
    public string? LogoPath { get; }
    public double? ProgressPercentage { get; }
    public string? HealthStatus { get; }
    public DateTime? DataDate { get; }
    public decimal? Budget { get; }
    public int ActivitiesCount { get; }
    public string? CoverImagePath { get; }
    public string? ConnectedXerPath { get; }
    public string? ConnectedDatabaseName { get; }
    public DateTime? LastImportDate { get; }
    public DateTime? LastExportDate { get; }

    [ObservableProperty]
    private bool _isSelected;

    public ProjectListItem(ProjectSummaryDto dto)
    {
        Id = dto.Id;
        Code = dto.Code;
        Name = dto.Name;
        Status = dto.Status;
        ClientName = dto.ClientName;
        StartDate = dto.StartDate;
        FinishDate = dto.FinishDate;
        UpdatedAt = dto.UpdatedAt;
        ContractorName = dto.ContractorName;
        DocumentCount = dto.DocumentCount;
        LogoPath = dto.LogoPath;
        ProgressPercentage = dto.ProgressPercentage;
        HealthStatus = dto.HealthStatus;
        DataDate = dto.DataDate;
        Budget = dto.Budget;
        ActivitiesCount = dto.ActivitiesCount;
        CoverImagePath = dto.CoverImagePath;
        ConnectedXerPath = dto.ConnectedXerPath;
        ConnectedDatabaseName = dto.ConnectedDatabaseName;
        LastImportDate = dto.LastImportDate;
        LastExportDate = dto.LastExportDate;
    }
}
