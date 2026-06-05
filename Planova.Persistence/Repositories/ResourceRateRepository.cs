using Microsoft.EntityFrameworkCore;
using Planova.Persistence.DbContext;
using Planova.Resource.Domain.Entities;
using Planova.Resource.Domain.Interfaces;

namespace Planova.Persistence.Repositories;

public class ResourceRateRepository : IResourceRateRepository
{
    private readonly PlanovaDbContext _context;

    public ResourceRateRepository(PlanovaDbContext context)
    {
        _context = context;
    }

    public async Task<ResourceRate?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _context.ResourceRates.FindAsync([id], ct);

    public async Task<List<ResourceRate>> GetByResourceAsync(Guid resourceId, CancellationToken ct = default)
        => await _context.ResourceRates.Where(r => r.ResourceId == resourceId).OrderByDescending(r => r.EffectiveDate).ToListAsync(ct);

    public async Task<ResourceRate?> GetEffectiveRateAsync(Guid resourceId, DateTime date, CancellationToken ct = default)
        => await _context.ResourceRates.Where(r => r.ResourceId == resourceId && r.EffectiveDate <= date).OrderByDescending(r => r.EffectiveDate).FirstOrDefaultAsync(ct);

    public async Task<bool> HasDuplicateEffectiveDateAsync(Guid resourceId, DateTime effectiveDate, Guid? excludeId = null, CancellationToken ct = default)
    {
        var q = _context.ResourceRates.Where(r => r.ResourceId == resourceId && r.EffectiveDate == effectiveDate);
        if (excludeId.HasValue)
            q = q.Where(r => r.Id != excludeId.Value);
        return await q.AnyAsync(ct);
    }

    public async Task AddAsync(ResourceRate rate, CancellationToken ct = default)
    {
        await _context.ResourceRates.AddAsync(rate, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(ResourceRate rate, CancellationToken ct = default)
    {
        _context.ResourceRates.Update(rate);
        await _context.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _context.ResourceRates.FindAsync([id], ct);
        if (entity is not null)
        {
            _context.ResourceRates.Remove(entity);
            await _context.SaveChangesAsync(ct);
        }
    }

    public async Task BulkUpdateAsync(List<(Guid ResourceId, decimal NewRate)> rateUpdates, DateTime effectiveDate, CancellationToken ct = default)
    {
        foreach (var (resourceId, newRate) in rateUpdates)
        {
            var rate = new ResourceRate
            {
                Id = Guid.NewGuid(),
                ResourceId = resourceId,
                EffectiveDate = effectiveDate,
                Rate = newRate,
                Currency = "USD",
                UnitOfMeasure = "hr",
                CreatedAt = DateTime.UtcNow
            };
            await _context.ResourceRates.AddAsync(rate, ct);
        }
        await _context.SaveChangesAsync(ct);
    }
}
