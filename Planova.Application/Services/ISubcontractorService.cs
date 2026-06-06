using Planova.Application.Dto;

namespace Planova.Application.Services;

public interface ISubcontractorService
{
    Task<IEnumerable<SubcontractorSummaryDto>> GetAllAsync(CancellationToken ct = default);
    Task<SubcontractorDetailDto?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<SubcontractorDetailDto> CreateAsync(CreateSubcontractorDto dto, CancellationToken ct = default);
    Task<SubcontractorDetailDto> UpdateAsync(int id, UpdateSubcontractorDto dto, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
    Task<IEnumerable<SubcontractorSummaryDto>> SearchAsync(string query, CancellationToken ct = default);
}