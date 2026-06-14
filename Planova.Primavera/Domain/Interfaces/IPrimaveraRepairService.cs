using Planova.Primavera.Application.Dto;

namespace Planova.Primavera.Domain.Interfaces;

public interface IPrimaveraRepairService
{
    Task<List<PrimaveraRepairActionDto>> GetSuggestedFixesAsync(int projectId, CancellationToken ct = default);
    Task<bool> ApplyFixAsync(Guid actionId, CancellationToken ct = default);
    Task<bool> ApplyAllFixesAsync(int projectId, CancellationToken ct = default);
}
