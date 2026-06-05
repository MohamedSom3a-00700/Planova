using Microsoft.EntityFrameworkCore;
using Planova.Activity.Domain.Entities;
using Planova.Activity.Domain.Interfaces;
using Planova.Persistence.DbContext;

namespace Planova.Persistence.Repositories;

public class ActivityBankItemRelationshipRepository : IActivityBankItemRelationshipRepository
{
    private readonly PlanovaDbContext _context;

    public ActivityBankItemRelationshipRepository(PlanovaDbContext context)
    {
        _context = context;
    }

    public async Task<List<ActivityBankItemRelationship>> GetByBankIdAsync(Guid bankId, CancellationToken ct = default)
    {
        return await _context.ActivityBankItemRelationships
            .Where(r => r.BankId == bankId)
            .ToListAsync(ct);
    }

    public async Task AddRangeAsync(IEnumerable<ActivityBankItemRelationship> relationships, CancellationToken ct = default)
    {
        _context.ActivityBankItemRelationships.AddRange(relationships);
        await _context.SaveChangesAsync(ct);
    }

    public async Task DeleteByBankIdAsync(Guid bankId, CancellationToken ct = default)
    {
        var relationships = await _context.ActivityBankItemRelationships
            .Where(r => r.BankId == bankId)
            .ToListAsync(ct);
        _context.ActivityBankItemRelationships.RemoveRange(relationships);
        await _context.SaveChangesAsync(ct);
    }
}
