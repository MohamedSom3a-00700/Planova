using Microsoft.EntityFrameworkCore;
using Planova.Persistence.DbContext;
using Planova.Reporting.Domain.Entities;
using Planova.Reporting.Domain.Interfaces;

namespace Planova.Persistence.Repositories;

public class ProjectPartyRepository : IProjectPartyRepository
{
    private readonly PlanovaDbContext _context;

    public ProjectPartyRepository(PlanovaDbContext context)
    {
        _context = context;
    }

    public async Task<List<ProjectParty>> GetByProjectAsync(int projectId, CancellationToken ct = default)
    {
        return await _context.ProjectParties
            .Where(p => p.ProjectId == projectId)
            .OrderBy(p => p.DisplayOrder)
            .ToListAsync(ct);
    }

    public async Task<ProjectParty?> GetClientAsync(int projectId, CancellationToken ct = default)
    {
        return await _context.ProjectParties
            .FirstOrDefaultAsync(p => p.ProjectId == projectId && p.Role == Reporting.Domain.Enums.PartyRole.Client, ct);
    }

    public async Task<ProjectParty?> GetMainContractorAsync(int projectId, CancellationToken ct = default)
    {
        return await _context.ProjectParties
            .FirstOrDefaultAsync(p => p.ProjectId == projectId && p.Role == Reporting.Domain.Enums.PartyRole.MainContractor, ct);
    }

    public async Task<List<ProjectParty>> GetSubContractorsAsync(int projectId, CancellationToken ct = default)
    {
        return await _context.ProjectParties
            .Where(p => p.ProjectId == projectId && p.Role == Reporting.Domain.Enums.PartyRole.SubContractor)
            .OrderBy(p => p.DisplayOrder)
            .ToListAsync(ct);
    }

    public async Task<ProjectParty?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.ProjectParties.FindAsync(new object[] { id }, ct);
    }

    public async Task AddAsync(ProjectParty party, CancellationToken ct = default)
    {
        await _context.ProjectParties.AddAsync(party, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(ProjectParty party, CancellationToken ct = default)
    {
        _context.ProjectParties.Update(party);
        await _context.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(ProjectParty party, CancellationToken ct = default)
    {
        _context.ProjectParties.Remove(party);
        await _context.SaveChangesAsync(ct);
    }
}
