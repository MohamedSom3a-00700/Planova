using Planova.Application.Dto;

namespace Planova.Application.Interfaces;

public interface IPartyService
{
    Task<IReadOnlyList<PartyDto>> GetPartiesByProjectAsync(int projectId, CancellationToken ct);
    Task<PartyDto> CreatePartyAsync(CreatePartyRequest request, CancellationToken ct);
    Task<PartyDto> UpdatePartyAsync(Guid partyId, UpdatePartyRequest request, CancellationToken ct);
    Task DeletePartyAsync(Guid partyId, CancellationToken ct);
    Task LinkPartyToProjectAsync(Guid partyId, int projectId, string role, CancellationToken ct);
    Task UnlinkPartyFromProjectAsync(Guid partyId, int projectId, CancellationToken ct);
}

public record PartyDto(Guid Id, string Name, string PartyType, string? LogoPath, string? ContactName, string? ContactEmail, string? ContactPhone, string? Address, string? Notes, DateTime CreatedAt, DateTime UpdatedAt);
public record CreatePartyRequest(string Name, string PartyType, string? LogoPath, string? ContactName, string? ContactEmail, string? ContactPhone, string? Address, string? Notes);
public record UpdatePartyRequest(string Name, string PartyType, string? LogoPath, string? ContactName, string? ContactEmail, string? ContactPhone, string? Address, string? Notes);
