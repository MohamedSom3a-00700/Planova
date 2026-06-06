using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Planova.Application.Dto;
using Planova.Application.Services;

namespace Planova.UI.ViewModels;

public partial class ClientsWorkspaceViewModel : ObservableObject
{
    private readonly IClientService _clientService;

    public ClientsWorkspaceViewModel(IClientService clientService)
    {
        _clientService = clientService;
    }

    public ObservableCollection<ClientSummaryDto> Clients { get; } = new();

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _searchQuery = string.Empty;

    [ObservableProperty]
    private ClientDetailDto? _selectedClient;

    [ObservableProperty]
    private bool _isEditing;

    [ObservableProperty]
    private bool _isCreating;

    [ObservableProperty]
    private string _editCode = string.Empty;

    [ObservableProperty]
    private string _editName = string.Empty;

    [ObservableProperty]
    private string? _editContactEmail;

    [ObservableProperty]
    private string? _editContactPhone;

    [ObservableProperty]
    private string? _editOrganizationDetails;

    [ObservableProperty]
    private string? _editLogo;

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
            var clients = string.IsNullOrWhiteSpace(SearchQuery)
                ? await _clientService.GetAllAsync()
                : await _clientService.SearchAsync(SearchQuery);

            Clients.Clear();
            foreach (var c in clients)
                Clients.Add(c);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to load clients: {ex.Message}";
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
    private async Task SelectClientAsync(ClientSummaryDto? client)
    {
        if (client == null) return;

        IsLoading = true;
        try
        {
            SelectedClient = await _clientService.GetByIdAsync(client.Id);
            IsEditing = false;
            IsCreating = false;
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to load client details: {ex.Message}";
            HasError = true;
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void NewClient()
    {
        IsCreating = true;
        IsEditing = true;
        SelectedClient = null;
        EditCode = string.Empty;
        EditName = string.Empty;
        EditContactEmail = null;
        EditContactPhone = null;
        EditOrganizationDetails = null;
        EditLogo = null;
        EditNotes = null;
        ErrorMessage = string.Empty;
        HasError = false;
    }

    [RelayCommand]
    private void EditClient()
    {
        if (SelectedClient == null) return;

        IsCreating = false;
        IsEditing = true;
        EditCode = SelectedClient.Code;
        EditName = SelectedClient.Name;
        EditContactEmail = SelectedClient.ContactEmail;
        EditContactPhone = SelectedClient.ContactPhone;
        EditOrganizationDetails = SelectedClient.OrganizationDetails;
        EditLogo = SelectedClient.Logo;
        EditNotes = SelectedClient.Notes;
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
                var dto = new CreateClientDto(EditCode, EditName, EditContactEmail,
                    EditContactPhone, EditOrganizationDetails, EditLogo, EditNotes);

                SelectedClient = await _clientService.CreateAsync(dto);
            }
            else if (SelectedClient != null)
            {
                var dto = new UpdateClientDto(EditCode, EditName, EditContactEmail,
                    EditContactPhone, EditOrganizationDetails, EditLogo, EditNotes);

                SelectedClient = await _clientService.UpdateAsync(SelectedClient.Id, dto);
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
        if (SelectedClient == null) return;

        IsLoading = true;
        try
        {
            await _clientService.DeleteAsync(SelectedClient.Id);
            SelectedClient = null;
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
