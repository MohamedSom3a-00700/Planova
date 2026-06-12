using Planova.Reporting.Application.Dto;

namespace Planova.Reporting.Domain.Interfaces;

public interface IProjectPartyService
{
    Task<List<ProjectPartyDto>> GetPartiesAsync(int projectId, CancellationToken ct = default);
    Task<ProjectPartyDto> GetClientAsync(int projectId, CancellationToken ct = default);
    Task<ProjectPartyDto> GetMainContractorAsync(int projectId, CancellationToken ct = default);
    Task<List<ProjectPartyDto>> GetSubContractorsAsync(int projectId, CancellationToken ct = default);
    Task<ProjectPartyDto> SavePartyAsync(int projectId, SavePartyRequest request, CancellationToken ct = default);
    Task DeletePartyAsync(Guid partyId, CancellationToken ct = default);
    Task<string> UploadLogoAsync(Guid partyId, string fileName, Stream fileStream, CancellationToken ct = default);
}
