using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Planova.Application.Dto;
using Planova.Application.Services;

namespace Planova.UI.ViewModels;

public partial class ContractsWorkspaceViewModel : ObservableObject
{
    private readonly IContractService _contractService;
    private readonly IProjectService _projectService;
    private readonly IClientService _clientService;

    public ContractsWorkspaceViewModel(
        IContractService contractService,
        IProjectService projectService,
        IClientService clientService)
    {
        _contractService = contractService;
        _projectService = projectService;
        _clientService = clientService;
    }

    public ObservableCollection<ContractSummaryDto> Contracts { get; } = new();
    public ObservableCollection<ProjectSummaryDto> Projects { get; } = new();
    public ObservableCollection<ClientSummaryDto> Clients { get; } = new();

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _searchQuery = string.Empty;

    [ObservableProperty]
    private ContractDetailDto? _selectedContract;

    [ObservableProperty]
    private bool _isEditing;

    [ObservableProperty]
    private bool _isCreating;

    [ObservableProperty]
    private string _editNumber = string.Empty;

    [ObservableProperty]
    private string _editTitle = string.Empty;

    [ObservableProperty]
    private decimal? _editValue;

    [ObservableProperty]
    private string? _editCurrency;

    [ObservableProperty]
    private DateTime? _editAwardDate;

    [ObservableProperty]
    private DateTime? _editCommencementDate;

    [ObservableProperty]
    private DateTime? _editCompletionDate;

    [ObservableProperty]
    private string? _editStatus;

    [ObservableProperty]
    private string? _editNotes;

    [ObservableProperty]
    private int _editProjectId;

    [ObservableProperty]
    private int _editClientId;

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
            var contracts = await _contractService.GetAllAsync();
            Contracts.Clear();
            foreach (var c in contracts)
                Contracts.Add(c);

            var projects = await _projectService.GetAllAsync();
            Projects.Clear();
            foreach (var p in projects)
                Projects.Add(p);

            var clients = await _clientService.GetAllAsync();
            Clients.Clear();
            foreach (var c in clients)
                Clients.Add(c);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to load contracts: {ex.Message}";
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
        IsLoading = true;
        try
        {
            var contracts = await _contractService.SearchAsync(SearchQuery);
            Contracts.Clear();
            foreach (var c in contracts)
                Contracts.Add(c);
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
    private async Task SelectContractAsync(ContractSummaryDto? contract)
    {
        if (contract == null) return;

        IsLoading = true;
        try
        {
            SelectedContract = await _contractService.GetByIdAsync(contract.Id);
            IsEditing = false;
            IsCreating = false;
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to load contract details: {ex.Message}";
            HasError = true;
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void NewContract()
    {
        IsCreating = true;
        IsEditing = true;
        SelectedContract = null;
        EditNumber = string.Empty;
        EditTitle = string.Empty;
        EditValue = null;
        EditCurrency = null;
        EditAwardDate = null;
        EditCommencementDate = null;
        EditCompletionDate = null;
        EditStatus = null;
        EditNotes = null;
        EditProjectId = 0;
        EditClientId = 0;
        ErrorMessage = string.Empty;
        HasError = false;
    }

    [RelayCommand]
    private void EditContract()
    {
        if (SelectedContract == null) return;

        IsCreating = false;
        IsEditing = true;
        EditNumber = SelectedContract.Number;
        EditTitle = SelectedContract.Title;
        EditValue = SelectedContract.Value;
        EditCurrency = SelectedContract.Currency;
        EditAwardDate = SelectedContract.AwardDate;
        EditCommencementDate = SelectedContract.CommencementDate;
        EditCompletionDate = SelectedContract.CompletionDate;
        EditStatus = SelectedContract.Status;
        EditNotes = SelectedContract.Notes;
        EditProjectId = SelectedContract.ProjectId;
        EditClientId = SelectedContract.ClientId;
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
                var dto = new CreateContractDto(EditNumber, EditTitle, EditValue, EditCurrency,
                    EditAwardDate, EditCommencementDate, EditCompletionDate, EditStatus, EditNotes,
                    EditProjectId, EditClientId);

                SelectedContract = await _contractService.CreateAsync(dto);
            }
            else if (SelectedContract != null)
            {
                var dto = new UpdateContractDto(EditNumber, EditTitle, EditValue, EditCurrency,
                    EditAwardDate, EditCommencementDate, EditCompletionDate, EditStatus, EditNotes,
                    EditProjectId, EditClientId);

                SelectedContract = await _contractService.UpdateAsync(SelectedContract.Id, dto);
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
        if (SelectedContract == null) return;

        IsLoading = true;
        try
        {
            await _contractService.DeleteAsync(SelectedContract.Id);
            SelectedContract = null;
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
