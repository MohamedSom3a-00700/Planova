using Microsoft.EntityFrameworkCore;
using Planova.Persistence.DbContext;
using Planova.Wbs.Domain.Interfaces;

namespace Planova.Persistence.Repositories;

using WbsEntity = Planova.Wbs.Domain.Entities.Wbs;

public class WbsRepository : IWbsRepository
{
    private readonly PlanovaDbContext _context;

    public WbsRepository(PlanovaDbContext context)
    {
        _context = context;
    }

    public async Task<WbsEntity> GetByIdAsync(Guid id, CancellationToken ct)
    {
        return await _context.WbsEntries
            .Include(w => w.Items)
            .FirstOrDefaultAsync(w => w.Id == id, ct)
            ?? throw new KeyNotFoundException($"WBS {id} not found");
    }

    public async Task<IReadOnlyList<WbsEntity>> GetByProjectIdAsync(int projectId, CancellationToken ct)
    {
        return await _context.WbsEntries
            .Where(w => w.ProjectId == projectId)
            .OrderBy(w => w.Name)
            .ToListAsync(ct);
    }

    public async Task<WbsEntity> AddAsync(WbsEntity wbs, CancellationToken ct)
    {
        _context.WbsEntries.Add(wbs);
        await _context.SaveChangesAsync(ct);
        return wbs;
    }

    public async Task<WbsEntity> UpdateAsync(WbsEntity wbs, CancellationToken ct)
    {
        _context.WbsEntries.Update(wbs);
        await _context.SaveChangesAsync(ct);
        return wbs;
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct)
    {
        var entity = await _context.WbsEntries.FindAsync(new object[] { id }, ct);
        if (entity is not null)
        {
            _context.WbsEntries.Remove(entity);
            await _context.SaveChangesAsync(ct);
        }
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken ct)
    {
        return await _context.WbsEntries.AnyAsync(w => w.Id == id, ct);
    }
}
