using Planova.Primavera.Domain.Entities;

namespace Planova.Primavera.Domain.Interfaces;

public interface IPrimaveraValidationRepository
{
    Task<List<PrimaveraValidationIssue>> GetIssuesAsync(int projectId, CancellationToken ct = default);
    Task AddIssuesAsync(IEnumerable<PrimaveraValidationIssue> issues, CancellationToken ct = default);
    Task ClearIssuesAsync(int projectId, CancellationToken ct = default);
}
