using Planova.Primavera.Domain.Entities;

namespace Planova.Primavera.Domain.Interfaces;

public interface IPrimaveraRepairRepository
{
    Task<List<PrimaveraRepairAction>> GetActionsAsync(int projectId, CancellationToken ct = default);
    Task<PrimaveraRepairAction> AddActionAsync(PrimaveraRepairAction action, CancellationToken ct = default);
    Task UpdateActionAsync(PrimaveraRepairAction action, CancellationToken ct = default);
}
