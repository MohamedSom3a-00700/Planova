using Planova.Application.Dto;

namespace Planova.Application.Services;

public interface IContractService
{
    Task<IEnumerable<ContractSummaryDto>> GetAllAsync(CancellationToken ct = default);
    Task<ContractDetailDto?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<ContractDetailDto> CreateAsync(CreateContractDto dto, CancellationToken ct = default);
    Task<ContractDetailDto> UpdateAsync(int id, UpdateContractDto dto, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
    Task<IEnumerable<ContractSummaryDto>> SearchAsync(string query, CancellationToken ct = default);
    Task<IEnumerable<ContractSummaryDto>> GetByProjectAsync(int projectId, CancellationToken ct = default);
    Task<IEnumerable<ContractSummaryDto>> GetByClientAsync(int clientId, CancellationToken ct = default);
}
