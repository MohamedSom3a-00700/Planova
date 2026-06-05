using Microsoft.EntityFrameworkCore;
using Planova.Persistence.DbContext;
using Planova.Resource.Domain.Entities;
using Planova.Resource.Domain.Interfaces;

namespace Planova.Persistence.Repositories;

public class CrewResourceRepository : ICrewResourceRepository
{
    private readonly PlanovaDbContext _context;

    public CrewResourceRepository(PlanovaDbContext context) => _context = context;

    public async Task<List<CrewResource>> GetByCrewAsync(Guid crewId, CancellationToken ct = default)
        => await _context.CrewResources.Where(cr => cr.CrewId == crewId).Include(cr => cr.Resource).ToListAsync(ct);

    public async Task<List<CrewResource>> GetByResourceAsync(Guid resourceId, CancellationToken ct = default)
        => await _context.CrewResources.Where(cr => cr.ResourceId == resourceId).Include(cr => cr.Crew).ToListAsync(ct);

    public async Task<CrewResource?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _context.CrewResources.FindAsync([id], ct);

    public async Task AddAsync(CrewResource crewResource, CancellationToken ct = default)
    {
        await _context.CrewResources.AddAsync(crewResource, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(CrewResource crewResource, CancellationToken ct = default)
    {
        _context.CrewResources.Update(crewResource);
        await _context.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _context.CrewResources.FindAsync([id], ct);
        if (entity is not null)
        {
            _context.CrewResources.Remove(entity);
            await _context.SaveChangesAsync(ct);
        }
    }

    public async Task DeleteByCrewAsync(Guid crewId, CancellationToken ct = default)
    {
        var items = await _context.CrewResources.Where(cr => cr.CrewId == crewId).ToListAsync(ct);
        _context.CrewResources.RemoveRange(items);
        await _context.SaveChangesAsync(ct);
    }
}
