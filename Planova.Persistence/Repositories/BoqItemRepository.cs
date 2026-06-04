using Microsoft.EntityFrameworkCore;
using Planova.Boq.Domain.Interfaces;
using Planova.Persistence.DbContext;

namespace Planova.Persistence.Repositories;

using BoqItemEntity = Planova.Boq.Domain.Entities.BoqItem;

public class BoqItemRepository : IBoqItemRepository
{
    private readonly PlanovaDbContext _context;

    public BoqItemRepository(PlanovaDbContext context)
    {
        _context = context;
    }

    public async Task<BoqItemEntity> GetByIdAsync(Guid id, CancellationToken ct)
    {
        return await _context.BoqItems
            .FirstOrDefaultAsync(i => i.Id == id, ct)
            ?? throw new KeyNotFoundException($"BoqItem {id} not found");
    }

    public async Task<IReadOnlyList<BoqItemEntity>> GetByBoqIdAsync(Guid boqId, CancellationToken ct)
    {
        return await _context.BoqItems
            .Where(i => i.BoqId == boqId)
            .OrderBy(i => i.SortOrder)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<BoqItemEntity>> GetChildrenAsync(Guid parentId, CancellationToken ct)
    {
        return await _context.BoqItems
            .Where(i => i.ParentId == parentId)
            .OrderBy(i => i.SortOrder)
            .ToListAsync(ct);
    }

    public async Task<BoqItemEntity> AddAsync(BoqItemEntity item, CancellationToken ct)
    {
        _context.BoqItems.Add(item);
        await _context.SaveChangesAsync(ct);
        return item;
    }

    public async Task AddRangeAsync(IEnumerable<BoqItemEntity> items, CancellationToken ct)
    {
        _context.BoqItems.AddRange(items);
        await _context.SaveChangesAsync(ct);
    }

    public async Task<BoqItemEntity> UpdateAsync(BoqItemEntity item, CancellationToken ct)
    {
        _context.BoqItems.Update(item);
        await _context.SaveChangesAsync(ct);
        return item;
    }

    public async Task UpdateRangeAsync(IEnumerable<BoqItemEntity> items, CancellationToken ct)
    {
        _context.BoqItems.UpdateRange(items);
        await _context.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct)
    {
        var entity = await _context.BoqItems.FindAsync(new object[] { id }, ct);
        if (entity is not null)
        {
            _context.BoqItems.Remove(entity);
            await _context.SaveChangesAsync(ct);
        }
    }

    public async Task DeleteRangeAsync(IEnumerable<Guid> ids, CancellationToken ct)
    {
        var items = await _context.BoqItems
            .Where(i => ids.Contains(i.Id))
            .ToListAsync(ct);
        _context.BoqItems.RemoveRange(items);
        await _context.SaveChangesAsync(ct);
    }

    public async Task<int> GetMaxSortOrderAsync(Guid boqId, Guid? parentId, CancellationToken ct)
    {
        var max = await _context.BoqItems
            .Where(i => i.BoqId == boqId && i.ParentId == parentId)
            .MaxAsync(i => (int?)i.SortOrder, ct);
        return max ?? 0;
    }
}
