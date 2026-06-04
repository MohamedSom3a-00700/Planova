using Microsoft.EntityFrameworkCore;
using Planova.Persistence.DbContext;
using Planova.Wbs.Domain.Interfaces;

namespace Planova.Persistence.Repositories;

using WbsItemEntity = Planova.Wbs.Domain.Entities.WbsItem;

public class WbsItemRepository : IWbsItemRepository
{
    private readonly PlanovaDbContext _context;

    public WbsItemRepository(PlanovaDbContext context)
    {
        _context = context;
    }

    public async Task<WbsItemEntity> GetByIdAsync(Guid id, CancellationToken ct)
    {
        return await _context.WbsItems
            .FirstOrDefaultAsync(i => i.Id == id, ct)
            ?? throw new KeyNotFoundException($"WbsItem {id} not found");
    }

    public async Task<IReadOnlyList<WbsItemEntity>> GetByWbsIdAsync(Guid wbsId, CancellationToken ct)
    {
        return await _context.WbsItems
            .Where(i => i.WbsId == wbsId)
            .OrderBy(i => i.SortOrder)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<WbsItemEntity>> GetChildrenAsync(Guid parentId, CancellationToken ct)
    {
        return await _context.WbsItems
            .Where(i => i.ParentId == parentId)
            .OrderBy(i => i.SortOrder)
            .ToListAsync(ct);
    }

    public async Task<WbsItemEntity> AddAsync(WbsItemEntity item, CancellationToken ct)
    {
        _context.WbsItems.Add(item);
        await _context.SaveChangesAsync(ct);
        return item;
    }

    public async Task UpdateRangeAsync(IEnumerable<WbsItemEntity> items, CancellationToken ct)
    {
        _context.WbsItems.UpdateRange(items);
        await _context.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct)
    {
        var entity = await _context.WbsItems.FindAsync(new object[] { id }, ct);
        if (entity is not null)
        {
            _context.WbsItems.Remove(entity);
            await _context.SaveChangesAsync(ct);
        }
    }

    public async Task DeleteRangeAsync(IEnumerable<Guid> ids, CancellationToken ct)
    {
        var items = await _context.WbsItems
            .Where(i => ids.Contains(i.Id))
            .ToListAsync(ct);
        _context.WbsItems.RemoveRange(items);
        await _context.SaveChangesAsync(ct);
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken ct)
    {
        return await _context.WbsItems.AnyAsync(i => i.Id == id, ct);
    }
}
