namespace Planova.Wbs.Domain.Interfaces;

public interface IWbsService
{
    Task<Entities.Wbs> CreateAsync(string name, int projectId, WbsSource source, Guid? sourceBoqId, CancellationToken ct);
    Task<Entities.Wbs> GetByIdAsync(Guid id, CancellationToken ct);
    Task<IReadOnlyList<Entities.Wbs>> GetByProjectAsync(int projectId, CancellationToken ct);
    Task<Entities.Wbs> UpdateAsync(Entities.Wbs wbs, CancellationToken ct);
    Task DeleteAsync(Guid id, CancellationToken ct);
    Task ChangeStatusAsync(Guid id, WbsStatus newStatus, CancellationToken ct);
    Task<IReadOnlyList<Entities.WbsItem>> GetTreeAsync(Guid wbsId, CancellationToken ct);
    Task RedistributeWeightsAsync(Guid wbsId, CancellationToken ct);
}
