using Microsoft.EntityFrameworkCore;
using Planova.Application.Repositories;
using Planova.Domain.Entities;
using Planova.Persistence.DbContext;

namespace Planova.Persistence.Repositories;

public class ContractRepository : IContractRepository
{
    private readonly PlanovaDbContext _context;

    public ContractRepository(PlanovaDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Contract>> GetAllAsync(CancellationToken ct = default)
    {
        return await _context.Contracts
            .Include(c => c.Project)
            .Include(c => c.Client)
            .OrderByDescending(c => c.UpdatedAt)
            .ToListAsync(ct);
    }

    public async Task<Contract?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        return await _context.Contracts
            .Include(c => c.Project)
            .Include(c => c.Client)
            .FirstOrDefaultAsync(c => c.Id == id, ct);
    }

    public async Task<Contract> AddAsync(Contract contract, CancellationToken ct = default)
    {
        _context.Contracts.Add(contract);
        await _context.SaveChangesAsync(ct);
        return contract;
    }

    public async Task UpdateAsync(Contract contract, CancellationToken ct = default)
    {
        _context.Contracts.Update(contract);
        await _context.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Contract contract, CancellationToken ct = default)
    {
        _context.Contracts.Remove(contract);
        await _context.SaveChangesAsync(ct);
    }

    public async Task<IEnumerable<Contract>> SearchAsync(string query, CancellationToken ct = default)
    {
        var q = query.ToLower();
        return await _context.Contracts
            .Include(c => c.Project)
            .Include(c => c.Client)
            .Where(c => c.Title.ToLower().Contains(q) || c.Number.ToLower().Contains(q))
            .OrderByDescending(c => c.UpdatedAt)
            .ToListAsync(ct);
    }

    public async Task<IEnumerable<Contract>> GetByProjectAsync(int projectId, CancellationToken ct = default)
    {
        return await _context.Contracts
            .Include(c => c.Project)
            .Include(c => c.Client)
            .Where(c => c.ProjectId == projectId)
            .OrderByDescending(c => c.UpdatedAt)
            .ToListAsync(ct);
    }

    public async Task<IEnumerable<Contract>> GetByClientAsync(int clientId, CancellationToken ct = default)
    {
        return await _context.Contracts
            .Include(c => c.Project)
            .Include(c => c.Client)
            .Where(c => c.ClientId == clientId)
            .OrderByDescending(c => c.UpdatedAt)
            .ToListAsync(ct);
    }

    public async Task<bool> NumberExistsAsync(string number, int? excludeId = null, CancellationToken ct = default)
    {
        return await _context.Contracts
            .AnyAsync(c => c.Number == number && (!excludeId.HasValue || c.Id != excludeId.Value), ct);
    }

    public async Task<int> GetCountAsync(CancellationToken ct = default)
    {
        return await _context.Contracts.CountAsync(ct);
    }
}
