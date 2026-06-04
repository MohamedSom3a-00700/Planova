// IWbsService — WBS CRUD and tree management
// Implemented by Planova.Wbs.Application.Services

public interface IWbsService
{
    Task<Wbs> CreateAsync(string name, int projectId, WbsSource source, Guid? sourceBoqId, CancellationToken ct);
    Task<Wbs> GetByIdAsync(Guid id, CancellationToken ct);
    Task<IReadOnlyList<Wbs>> GetByProjectAsync(int projectId, CancellationToken ct);
    Task<Wbs> UpdateAsync(Wbs wbs, CancellationToken ct);
    Task DeleteAsync(Guid id, CancellationToken ct);
    Task ChangeStatusAsync(Guid id, WbsStatus newStatus, CancellationToken ct);
    Task<IReadOnlyList<WbsItem>> GetTreeAsync(Guid wbsId, CancellationToken ct);
    Task RedistributeWeightsAsync(Guid wbsId, CancellationToken ct);
}
