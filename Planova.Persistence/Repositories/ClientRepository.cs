using Microsoft.EntityFrameworkCore;
using Planova.Application.Repositories;
using Planova.Domain.Entities;
using Planova.Persistence.DbContext;

namespace Planova.Persistence.Repositories;

public class ClientRepository : IClientRepository
{
    private readonly PlanovaDbContext _context;

    public ClientRepository(PlanovaDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Client>> GetAllAsync(CancellationToken ct = default)
    {
        return await _context.Clients
            .Include(c => c.Projects)
            .OrderByDescending(c => c.UpdatedAt)
            .ToListAsync(ct);
    }

    public async Task<Client?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var client = await _context.Clients
            .FirstOrDefaultAsync(c => c.Id == id, ct);

        if (client != null)
        {
            await _context.Projects
                .Include(p => p.Client)
                .Where(p => p.ClientId == id)
                .LoadAsync(ct);

            await _context.Contracts
                .Include(c => c.Project)
                .Where(c => c.ClientId == id)
                .LoadAsync(ct);
        }

        return client;
    }

    public async Task<Client> AddAsync(Client client, CancellationToken ct = default)
    {
        _context.Clients.Add(client);
        await _context.SaveChangesAsync(ct);
        return client;
    }

    public async Task UpdateAsync(Client client, CancellationToken ct = default)
    {
        _context.Clients.Update(client);
        await _context.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Client client, CancellationToken ct = default)
    {
        _context.Clients.Remove(client);
        await _context.SaveChangesAsync(ct);
    }

    public async Task<IEnumerable<Client>> SearchAsync(string query, CancellationToken ct = default)
    {
        var q = query.ToLower();
        return await _context.Clients
            .Include(c => c.Projects)
            .Where(c => c.Name.ToLower().Contains(q) || c.Code.ToLower().Contains(q))
            .OrderByDescending(c => c.UpdatedAt)
            .ToListAsync(ct);
    }

    public async Task<bool> CodeExistsAsync(string code, int? excludeId = null, CancellationToken ct = default)
    {
        return await _context.Clients
            .AnyAsync(c => c.Code == code && (!excludeId.HasValue || c.Id != excludeId.Value), ct);
    }

    public async Task<bool> NameExistsAsync(string name, int? excludeId = null, CancellationToken ct = default)
    {
        return await _context.Clients
            .AnyAsync(c => c.Name == name && (!excludeId.HasValue || c.Id != excludeId.Value), ct);
    }

    public async Task<int> GetCountAsync(CancellationToken ct = default)
    {
        return await _context.Clients.CountAsync(ct);
    }

    public async Task<bool> HasLinkedProjectsAsync(int clientId, CancellationToken ct = default)
    {
        return await _context.Projects.AnyAsync(p => p.ClientId == clientId, ct);
    }
}
