using Planova.Application.Dto;
using Planova.Application.Interfaces;

namespace Planova.Application.Services;

public sealed class PartyService : IPartyService
{
    public Task<IReadOnlyList<PartyDto>> GetPartiesByProjectAsync(int projectId, CancellationToken ct)
    {
        return Task.FromResult<IReadOnlyList<PartyDto>>(new List<PartyDto>());
    }

    public Task<PartyDto> CreatePartyAsync(CreatePartyRequest request, CancellationToken ct)
    {
        var party = new PartyDto(
            Guid.NewGuid(), request.Name, request.PartyType, request.LogoPath,
            request.ContactName, request.ContactEmail, request.ContactPhone,
            request.Address, request.Notes, DateTime.UtcNow, DateTime.UtcNow);
        return Task.FromResult(party);
    }

    public Task<PartyDto> UpdatePartyAsync(Guid partyId, UpdatePartyRequest request, CancellationToken ct)
    {
        var party = new PartyDto(
            partyId, request.Name, request.PartyType, request.LogoPath,
            request.ContactName, request.ContactEmail, request.ContactPhone,
            request.Address, request.Notes, DateTime.UtcNow, DateTime.UtcNow);
        return Task.FromResult(party);
    }

    public Task DeletePartyAsync(Guid partyId, CancellationToken ct)
    {
        return Task.CompletedTask;
    }

    public Task LinkPartyToProjectAsync(Guid partyId, int projectId, string role, CancellationToken ct)
    {
        return Task.CompletedTask;
    }

    public Task UnlinkPartyFromProjectAsync(Guid partyId, int projectId, CancellationToken ct)
    {
        return Task.CompletedTask;
    }
}
