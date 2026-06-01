using Planova.Application.Dto;

namespace Planova.Application.Services;

public interface IClientService
{
    Task<IEnumerable<ClientSummaryDto>> GetAllAsync(CancellationToken ct = default);
    Task<ClientDetailDto?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<ClientDetailDto> CreateAsync(CreateClientDto dto, CancellationToken ct = default);
    Task<ClientDetailDto> UpdateAsync(int id, UpdateClientDto dto, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
    Task<IEnumerable<ClientSummaryDto>> SearchAsync(string query, CancellationToken ct = default);
}
