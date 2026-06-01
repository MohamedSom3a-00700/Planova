using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Planova.Application.Dto;
using Planova.Application.Services;

namespace Planova.UI.ViewModels;

public partial class ProjectsWorkspaceViewModel : ObservableObject
{
    private readonly IProjectService _projectService;
    private readonly IClientService _clientService;

    public ProjectsWorkspaceViewModel(IProjectService projectService, IClientService clientService)
    {
        _projectService = projectService;
        _clientService = clientService;
    }

    public ObservableCollection<ProjectSummaryDto> Projects { get; } = new();
    public ObservableCollection<ClientSummaryDto> Clients { get; } = new();
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
    private string? _editStartDate;

    [ObservableProperty]
    private string? _editFinishDate;

    [ObservableProperty]
    private string? _editCurrency;

    [ObservableProperty]
    private string? _editLocation;

    [ObservableProperty]
    private int? _editClientId;

    [ObservableProperty]
    private string? _editNotes;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private bool _hasError;

    [RelayCommand]
    private async Task LoadAsync()
    {
        IsLoading = true;
        HasError = false;

        try
        {
            var projects = SelectedStatusFilter == "All"
                ? await _projectService.GetAllAsync()
                : await _projectService.GetByStatusAsync(SelectedStatusFilter);

            Projects.Clear();
            foreach (var p in projects)
                Projects.Add(p);

            if (!string.IsNullOrEmpty(SearchQuery))
            {
                var filtered = await _projectService.SearchAsync(SearchQuery);
                Projects.Clear();
                foreach (var p in filtered.Where(p =>
                    SelectedStatusFilter == "All" || p.Status == SelectedStatusFilter))
                    Projects.Add(p);
            }

            var clients = await _clientService.GetAllAsync();
            Clients.Clear();
            foreach (var c in clients)
                Clients.Add(c);
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
    private async Task SearchAsync()
    {
        await LoadAsync();
    }

    [RelayCommand]
    private async Task FilterByStatusAsync(string? status)
    {
        SelectedStatusFilter = status ?? "All";
        await LoadAsync();
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
        EditStartDate = SelectedProject.StartDate?.ToString("yyyy-MM-dd");
        EditFinishDate = SelectedProject.FinishDate?.ToString("yyyy-MM-dd");
        EditCurrency = SelectedProject.Currency;
        EditLocation = SelectedProject.Location;
        EditClientId = SelectedProject.ClientId;
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
                    EditLocation, EditClientId, EditNotes);

                SelectedProject = await _projectService.CreateAsync(dto);
            }
            else if (SelectedProject != null)
            {
                var dto = new UpdateProjectDto(
                    EditCode, EditName, EditDescription,
                    EditStartDate, EditFinishDate, EditCurrency,
                    EditLocation, EditClientId, EditNotes);

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
