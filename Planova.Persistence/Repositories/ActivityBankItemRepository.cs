using Microsoft.EntityFrameworkCore;
using Planova.Activity.Domain.Entities;
using Planova.Activity.Domain.Interfaces;
using Planova.Persistence.DbContext;

namespace Planova.Persistence.Repositories;

public class ActivityBankItemRepository : IActivityBankItemRepository
{
    private readonly PlanovaDbContext _context;

    public ActivityBankItemRepository(PlanovaDbContext context)
    {
        _context = context;
    }

    public async Task<List<ActivityBankItem>> GetByBankIdAsync(Guid bankId, CancellationToken ct = default)
    {
        return await _context.ActivityBankItems
            .Where(i => i.BankId == bankId)
            .OrderBy(i => i.SortOrder)
            .ToListAsync(ct);
    }

    public async Task<ActivityBankItem?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.ActivityBankItems
            .FirstOrDefaultAsync(i => i.Id == id, ct);
    }

    public async Task AddRangeAsync(IEnumerable<ActivityBankItem> items, CancellationToken ct = default)
    {
        _context.ActivityBankItems.AddRange(items);
        await _context.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(ActivityBankItem item, CancellationToken ct = default)
    {
        _context.ActivityBankItems.Update(item);
        await _context.SaveChangesAsync(ct);
    }

    public async Task DeleteByBankIdAsync(Guid bankId, CancellationToken ct = default)
    {
        var items = await _context.ActivityBankItems
            .Where(i => i.BankId == bankId)
            .ToListAsync(ct);
        _context.ActivityBankItems.RemoveRange(items);
        await _context.SaveChangesAsync(ct);
    }
}
