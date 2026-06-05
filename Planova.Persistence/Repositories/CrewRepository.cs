using Microsoft.EntityFrameworkCore;
using Planova.Persistence.DbContext;
using Planova.Resource.Domain.Entities;
using Planova.Resource.Domain.Enums;
using Planova.Resource.Domain.Interfaces;

namespace Planova.Persistence.Repositories;

public class CrewRepository : ICrewRepository
{
    private readonly PlanovaDbContext _context;

    public CrewRepository(PlanovaDbContext context) => _context = context;

    public async Task<Crew?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _context.Crews.Include(c => c.Resources).ThenInclude(cr => cr.Resource).FirstOrDefaultAsync(c => c.Id == id, ct);

    public async Task<List<Crew>> GetAllAsync(int? projectId = null, CrewStatus? status = null, CancellationToken ct = default)
    {
        var q = _context.Crews.Include(c => c.Resources).AsQueryable();
        if (projectId.HasValue)
            q = q.Where(c => c.ProjectId == projectId || c.ProjectId == null);
        if (status.HasValue)
            q = q.Where(c => c.Status == status.Value);
        return await q.ToListAsync(ct);
    }

    public async Task<List<Crew>> SearchAsync(string query, int? projectId = null, CancellationToken ct = default)
    {
        var q = _context.Crews.Include(c => c.Resources).Where(c => c.Name.Contains(query));
        if (projectId.HasValue)
            q = q.Where(c => c.ProjectId == projectId || c.ProjectId == null);
        return await q.ToListAsync(ct);
    }

    public async Task AddAsync(Crew crew, CancellationToken ct = default)
    {
        await _context.Crews.AddAsync(crew, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Crew crew, CancellationToken ct = default)
    {
        _context.Crews.Update(crew);
        await _context.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _context.Crews.FindAsync([id], ct);
        if (entity is not null)
        {
            _context.Crews.Remove(entity);
            await _context.SaveChangesAsync(ct);
        }
    }
}
