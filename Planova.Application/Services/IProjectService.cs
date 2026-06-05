using Planova.Application.Dto;

namespace Planova.Application.Services;

public interface IProjectService
{
    Task<IEnumerable<ProjectSummaryDto>> GetAllAsync(CancellationToken ct = default);
    Task<ProjectDetailDto?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<ProjectDetailDto> CreateAsync(CreateProjectDto dto, CancellationToken ct = default);
    Task<ProjectDetailDto> UpdateAsync(int id, UpdateProjectDto dto, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
    Task<ProjectDetailDto> ChangeStatusAsync(int id, string newStatus, CancellationToken ct = default);
    Task<IEnumerable<ProjectSummaryDto>> SearchAsync(string query, CancellationToken ct = default);
    Task<IEnumerable<ProjectSummaryDto>> GetByStatusAsync(string status, CancellationToken ct = default);
    Task<DashboardSummaryDto> GetDashboardSummaryAsync(CancellationToken ct = default);
}
