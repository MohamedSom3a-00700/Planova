using Planova.Domain.Entities;

namespace Planova.Application.Repositories;

public interface IContractRepository
{
    Task<IEnumerable<Contract>> GetAllAsync(CancellationToken ct = default);
    Task<Contract?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<Contract> AddAsync(Contract contract, CancellationToken ct = default);
    Task UpdateAsync(Contract contract, CancellationToken ct = default);
    Task DeleteAsync(Contract contract, CancellationToken ct = default);
    Task<IEnumerable<Contract>> SearchAsync(string query, CancellationToken ct = default);
    Task<IEnumerable<Contract>> GetByProjectAsync(int projectId, CancellationToken ct = default);
    Task<IEnumerable<Contract>> GetByClientAsync(int clientId, CancellationToken ct = default);
    Task<bool> NumberExistsAsync(string number, int? excludeId = null, CancellationToken ct = default);
    Task<int> GetCountAsync(CancellationToken ct = default);
}
