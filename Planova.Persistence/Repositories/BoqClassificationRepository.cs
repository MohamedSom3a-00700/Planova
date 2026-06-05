using Microsoft.EntityFrameworkCore;
using Planova.Boq.Domain.Entities;
using Planova.Boq.Domain.Enums;
using Planova.Boq.Domain.Interfaces;
using Planova.Persistence.DbContext;

namespace Planova.Persistence.Repositories;

public class BoqClassificationRepository : IBoqClassificationRepository
{
    private readonly PlanovaDbContext _context;

    public BoqClassificationRepository(PlanovaDbContext context)
    {
        _context = context;
    }

    public async Task<BoqClassification> GetByIdAsync(Guid id, CancellationToken ct)
    {
        return await _context.BoqClassifications
            .FirstOrDefaultAsync(c => c.Id == id, ct)
            ?? throw new KeyNotFoundException($"BoqClassification {id} not found");
    }

    public async Task<IReadOnlyList<BoqClassification>> GetByProjectIdAsync(Guid? projectId, CancellationToken ct)
    {
        return await _context.BoqClassifications
            .Where(c => c.ProjectId == projectId)
            .OrderBy(c => c.Code)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<BoqClassification>> GetGlobalAsync(CancellationToken ct)
    {
        return await _context.BoqClassifications
            .Where(c => c.Scope == ClassificationScope.Global)
            .OrderBy(c => c.Code)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<BoqClassification>> GetChildrenAsync(Guid parentId, CancellationToken ct)
    {
        return await _context.BoqClassifications
            .Where(c => c.ParentId == parentId)
            .OrderBy(c => c.Code)
            .ToListAsync(ct);
    }

    public async Task<BoqClassification> AddAsync(BoqClassification classification, CancellationToken ct)
    {
        _context.BoqClassifications.Add(classification);
        await _context.SaveChangesAsync(ct);
        return classification;
    }

    public async Task<BoqClassification> UpdateAsync(BoqClassification classification, CancellationToken ct)
    {
        _context.BoqClassifications.Update(classification);
        await _context.SaveChangesAsync(ct);
        return classification;
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct)
    {
        var entity = await _context.BoqClassifications.FindAsync(new object[] { id }, ct);
        if (entity is not null)
        {
            _context.BoqClassifications.Remove(entity);
            await _context.SaveChangesAsync(ct);
        }
    }
}
