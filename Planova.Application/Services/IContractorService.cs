using Planova.Application.Dto;

namespace Planova.Application.Services;

public interface IContractorService
{
    Task<IEnumerable<ContractorSummaryDto>> GetAllAsync(CancellationToken ct = default);
    Task<ContractorDetailDto?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<ContractorDetailDto> CreateAsync(CreateContractorDto dto, CancellationToken ct = default);
    Task<ContractorDetailDto> UpdateAsync(int id, UpdateContractorDto dto, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
    Task<IEnumerable<ContractorSummaryDto>> SearchAsync(string query, CancellationToken ct = default);
}