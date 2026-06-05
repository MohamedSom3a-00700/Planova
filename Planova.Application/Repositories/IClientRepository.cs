using Planova.Domain.Entities;

namespace Planova.Application.Repositories;

public interface IClientRepository
{
    Task<IEnumerable<Client>> GetAllAsync(CancellationToken ct = default);
    Task<Client?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<Client> AddAsync(Client client, CancellationToken ct = default);
    Task UpdateAsync(Client client, CancellationToken ct = default);
    Task DeleteAsync(Client client, CancellationToken ct = default);
    Task<IEnumerable<Client>> SearchAsync(string query, CancellationToken ct = default);
    Task<bool> CodeExistsAsync(string code, int? excludeId = null, CancellationToken ct = default);
    Task<bool> NameExistsAsync(string name, int? excludeId = null, CancellationToken ct = default);
    Task<int> GetCountAsync(CancellationToken ct = default);
    Task<bool> HasLinkedProjectsAsync(int clientId, CancellationToken ct = default);
}
