using Microsoft.EntityFrameworkCore;
using Planova.Activity.Domain.Entities;
using Planova.Activity.Domain.Interfaces;
using Planova.Persistence.DbContext;

namespace Planova.Persistence.Repositories;

public class ActivityBankRepository : IActivityBankRepository
{
    private readonly PlanovaDbContext _context;

    public ActivityBankRepository(PlanovaDbContext context)
    {
        _context = context;
    }

    public async Task<ActivityBank?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.ActivityBanks
            .Include(b => b.Items)
            .Include(b => b.Relationships)
            .FirstOrDefaultAsync(b => b.Id == id, ct);
    }

    public async Task<List<ActivityBank>> GetByCategoryAsync(string category, CancellationToken ct = default)
    {
        return await _context.ActivityBanks
            .Where(b => b.Category == category)
            .Include(b => b.Items)
            .Include(b => b.Relationships)
            .ToListAsync(ct);
    }

    public async Task<List<string>> GetCategoriesAsync(CancellationToken ct = default)
    {
        return await _context.ActivityBanks
            .Select(b => b.Category)
            .Distinct()
            .OrderBy(c => c)
            .ToListAsync(ct);
    }

    public async Task<List<ActivityBank>> SearchAsync(string query, CancellationToken ct = default)
    {
        var lower = query.ToLowerInvariant();
        return await _context.ActivityBanks
            .Where(b => b.Name.ToLower().Contains(lower)
                || b.Code.ToLower().Contains(lower)
                || b.Category.ToLower().Contains(lower)
                || b.Description!.ToLower().Contains(lower))
            .Include(b => b.Items)
            .Include(b => b.Relationships)
            .ToListAsync(ct);
    }

    public async Task AddAsync(ActivityBank bank, CancellationToken ct = default)
    {
        _context.ActivityBanks.Add(bank);
        await _context.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(ActivityBank bank, CancellationToken ct = default)
    {
        _context.ActivityBanks.Update(bank);
        await _context.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _context.ActivityBanks.FindAsync(new object[] { id }, ct);
        if (entity is not null)
        {
            _context.ActivityBanks.Remove(entity);
            await _context.SaveChangesAsync(ct);
        }
    }

    public async Task<bool> IsStandardAsync(Guid id, CancellationToken ct = default)
    {
        var bank = await _context.ActivityBanks
            .Select(b => new { b.Id, b.IsStandard })
            .FirstOrDefaultAsync(b => b.Id == id, ct);
        return bank?.IsStandard ?? false;
    }
}
