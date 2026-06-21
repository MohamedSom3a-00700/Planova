using Planova.Application.Dto;

namespace Planova.Application.Services;

public interface IConsultantService
{
    Task<IEnumerable<ConsultantSummaryDto>> GetAllAsync(CancellationToken ct = default);
    Task<ConsultantDetailDto?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<ConsultantDetailDto> CreateAsync(CreateConsultantDto dto, CancellationToken ct = default);
    Task<ConsultantDetailDto> UpdateAsync(int id, UpdateConsultantDto dto, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
    Task<IEnumerable<ConsultantSummaryDto>> SearchAsync(string query, CancellationToken ct = default);
}
