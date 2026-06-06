using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Planova.Application.Dto;
using Planova.Application.Services;
using Planova.Shared.Abstractions;

namespace Planova.UI.ViewModels;

public partial class ProjectsWorkspaceViewModel : ObservableObject
{
    private readonly IProjectService _projectService;
    private readonly IClientService _clientService;
    private readonly IContractorService _contractorService;
    private readonly ISubcontractorService _subcontractorService;
    private readonly ICurrentProjectService _currentProjectService;
    private List<ProjectSummaryDto> _allProjects = new();

    public ProjectsWorkspaceViewModel(
        IProjectService projectService,
        IClientService clientService,
        IContractorService contractorService,
        ISubcontractorService subcontractorService,
        ICurrentProjectService currentProjectService)
    {
        _projectService = projectService;
        _clientService = clientService;
        _contractorService = contractorService;
        _subcontractorService = subcontractorService;
        _currentProjectService = currentProjectService;
    }

    public ObservableCollection<ProjectSummaryDto> Projects { get; } = new();
    public ObservableCollection<ClientSummaryDto> Clients { get; } = new();
    public ObservableCollection<ContractorSummaryDto> Contractors { get; } = new();
    public ObservableCollection<SubcontractorSummaryDto> Subcontractors { get; } = new();
    public ObservableCollection<string> Currencies { get; } = new()
    {
        "USD", "EUR", "GBP", "EGP", "SAR", "AED", "JOD", "KWD", "QAR", "OMR", "BHD", "LYD", "TND", "DZD", "MAD"
    };
    public ObservableCollection<string> StatusFilters { get; } = new()
    {
        "All", "Draft", "Under Review", "Approved", "In Progress", "On Hold", "Completed", "Cancelled"
    };

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _searchQuery = string.Empty;

    [ObservableProperty]
    private string _selectedStatusFilter = "All";

    [ObservableProperty]
    private ProjectDetailDto? _selectedProject;

    [ObservableProperty]
    private ProjectSummaryDto? _selectedSummary;

    partial void OnSelectedSummaryChanged(ProjectSummaryDto? value)
    {
        if (value is not null)
            SelectProjectCommand.Execute(value);
    }

    partial void OnSearchQueryChanged(string value)
    {
        ApplyFilters();
    }

    partial void OnSelectedStatusFilterChanged(string value)
    {
        ApplyFilters();
    }

    [ObservableProperty]
    private bool _isEditing;

    [ObservableProperty]
    private bool _isCreating;

    [ObservableProperty]
    private string _editCode = string.Empty;

    [ObservableProperty]
    private string _editName = string.Empty;

    [ObservableProperty]
    private string? _editDescription;

    [ObservableProperty]
    private DateTime? _editStartDate;

    [ObservableProperty]
    private DateTime? _editFinishDate;

    [ObservableProperty]
    private string? _editCurrency;

    [ObservableProperty]
    private string? _editLocation;

    [ObservableProperty]
    private int? _editClientId;

    [ObservableProperty]
    private int? _editContractorId;

    [ObservableProperty]
    private int? _editSubcontractorId;

    [ObservableProperty]
    private string? _editNotes;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private bool _hasError;

    private void ApplyFilters()
    {
        Projects.Clear();
        var filtered = _allProjects.AsEnumerable();

        if (SelectedStatusFilter != "All")
            filtered = filtered.Where(p => p.Status == SelectedStatusFilter);

        if (!string.IsNullOrEmpty(SearchQuery))
        {
            var query = SearchQuery.Trim().ToLowerInvariant();
            filtered = filtered.Where(p =>
                (p.Code?.ToLowerInvariant().Contains(query) == true) ||
                (p.Name?.ToLowerInvariant().Contains(query) == true) ||
                (p.ClientName?.ToLowerInvariant().Contains(query) == true) ||
                (p.Status?.ToLowerInvariant().Contains(query) == true));
        }

        foreach (var p in filtered)
            Projects.Add(p);
    }

    [RelayCommand]
    private async Task LoadAsync()
    {
        IsLoading = true;
        HasError = false;

        try
        {
            _allProjects = (await _projectService.GetAllAsync()).ToList();

            var clients = await _clientService.GetAllAsync();
            Clients.Clear();
            foreach (var c in clients)
                Clients.Add(c);

            var contractors = await _contractorService.GetAllAsync();
            Contractors.Clear();
            foreach (var c in contractors)
                Contractors.Add(c);

            var subcontractors = await _subcontractorService.GetAllAsync();
            Subcontractors.Clear();
            foreach (var s in subcontractors)
                Subcontractors.Add(s);

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
    private void Search()
    {
        ApplyFilters();
    }

    [RelayCommand]
    private void FilterByStatus(string? status)
    {
        SelectedStatusFilter = status ?? "All";
    }

    [RelayCommand]
    private async Task SelectProjectAsync(ProjectSummaryDto? project)
    {
        if (project == null) return;

        IsLoading = true;
        try
        {
            SelectedProject = await _projectService.GetByIdAsync(project.Id);
            IsEditing = false;
            IsCreating = false;

            if (SelectedProject != null)
            {
                _currentProjectService.SetProject(new ProjectContext(
                    SelectedProject.Id,
                    SelectedProject.Code,
                    SelectedProject.Name));
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to load project details: {ex.Message}";
            HasError = true;
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void NewProject()
    {
        IsCreating = true;
        IsEditing = true;
        SelectedProject = null;
        EditCode = string.Empty;
        EditName = string.Empty;
        EditDescription = null;
        EditStartDate = null;
        EditFinishDate = null;
        EditCurrency = null;
        EditLocation = null;
        EditClientId = null;
        EditContractorId = null;
        EditSubcontractorId = null;
        EditNotes = null;
        ErrorMessage = string.Empty;
        HasError = false;
    }

    [RelayCommand]
    private void EditProject()
    {
        if (SelectedProject == null) return;

        IsCreating = false;
        IsEditing = true;
        EditCode = SelectedProject.Code;
        EditName = SelectedProject.Name;
        EditDescription = SelectedProject.Description;
        EditStartDate = SelectedProject.StartDate;
        EditFinishDate = SelectedProject.FinishDate;
        EditCurrency = SelectedProject.Currency;
        EditLocation = SelectedProject.Location;
        EditClientId = SelectedProject.ClientId;
        EditContractorId = SelectedProject.ContractorId;
        EditSubcontractorId = SelectedProject.SubcontractorId;
        EditNotes = SelectedProject.Notes;
        ErrorMessage = string.Empty;
        HasError = false;
    }

    [RelayCommand]
    private void CancelEdit()
    {
        IsEditing = false;
        IsCreating = false;
        ErrorMessage = string.Empty;
        HasError = false;
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        HasError = false;
        IsLoading = true;

        try
        {
            if (IsCreating)
            {
                var dto = new CreateProjectDto(
                    EditCode, EditName, EditDescription,
                    EditStartDate, EditFinishDate, EditCurrency,
                    EditLocation, EditClientId, EditContractorId, EditSubcontractorId, EditNotes);

                SelectedProject = await _projectService.CreateAsync(dto);
            }
            else if (SelectedProject != null)
            {
                var dto = new UpdateProjectDto(
                    EditCode, EditName, EditDescription,
                    EditStartDate, EditFinishDate, EditCurrency,
                    EditLocation, EditClientId, EditContractorId, EditSubcontractorId, EditNotes);

                SelectedProject = await _projectService.UpdateAsync(SelectedProject.Id, dto);
            }

            IsEditing = false;
            IsCreating = false;
            await LoadAsync();
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            HasError = true;
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task DeleteAsync()
    {
        if (SelectedProject == null) return;

        IsLoading = true;
        try
        {
            await _projectService.DeleteAsync(SelectedProject.Id);
            _currentProjectService.SetProject(null);
            SelectedProject = null;
            await LoadAsync();
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            HasError = true;
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task ChangeStatusAsync(string? newStatus)
    {
        if (SelectedProject == null || string.IsNullOrEmpty(newStatus)) return;

        IsLoading = true;
        try
        {
            SelectedProject = await _projectService.ChangeStatusAsync(SelectedProject.Id, newStatus);
            await LoadAsync();
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            HasError = true;
        }
        finally
        {
            IsLoading = false;
        }
    }
}
