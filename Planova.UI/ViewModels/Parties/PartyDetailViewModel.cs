using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Planova.Application.Dto;
using Planova.Application.Interfaces;

namespace Planova.UI.ViewModels.Parties;

public sealed partial class PartyDetailViewModel : ObservableObject
{
    private readonly IPartyService _partyService;

    public PartyDetailViewModel(IPartyService partyService)
    {
        _partyService = partyService;
    }

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private bool _hasError;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private PartyDto? _party;

    [ObservableProperty]
    private string _editName = string.Empty;

    [ObservableProperty]
    private string _editPartyType = string.Empty;

    [ObservableProperty]
    private string _editContactName = string.Empty;

    [ObservableProperty]
    private string _editContactEmail = string.Empty;

    [ObservableProperty]
    private string _editContactPhone = string.Empty;

    [ObservableProperty]
    private string _editAddress = string.Empty;

    [ObservableProperty]
    private string _editNotes = string.Empty;

    [RelayCommand]
    private async Task LoadAsync(Guid partyId, CancellationToken ct)
    {
        // In a real implementation, we'd load the party by ID
        // For now, the party data comes from the list selection
        IsLoading = false;
    }

    [RelayCommand]
    private async Task SaveAsync(CancellationToken ct)
    {
        if (Party is null) return;

        IsLoading = true;
        HasError = false;

        try
        {
            var request = new UpdatePartyRequest(
                EditName, EditPartyType, Party.LogoPath,
                EditContactName, EditContactEmail, EditContactPhone,
                EditAddress, EditNotes);
            await _partyService.UpdatePartyAsync(Party.Id, request, ct);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to save party: {ex.Message}";
            HasError = true;
        }
        finally
        {
            IsLoading = false;
        }
    }
}
