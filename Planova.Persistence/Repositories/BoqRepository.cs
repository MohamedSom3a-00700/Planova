using Microsoft.EntityFrameworkCore;
using Planova.Boq.Domain.Interfaces;
using Planova.Persistence.DbContext;

namespace Planova.Persistence.Repositories;

using BoqEntity = Planova.Boq.Domain.Entities.Boq;

public class BoqRepository : IBoqRepository
{
    private readonly PlanovaDbContext _context;

    public BoqRepository(PlanovaDbContext context)
    {
        _context = context;
    }

    public async Task<BoqEntity> GetByIdAsync(Guid id, CancellationToken ct)
    {
        return await _context.Boqs
            .Include(b => b.Items)
            .FirstOrDefaultAsync(b => b.Id == id, ct)
            ?? throw new KeyNotFoundException($"BOQ {id} not found");
    }

    public async Task<IReadOnlyList<BoqEntity>> GetByProjectIdAsync(Guid projectId, CancellationToken ct)
    {
        return await _context.Boqs
            .Where(b => b.ProjectId == projectId)
            .OrderBy(b => b.Name)
            .ToListAsync(ct);
    }

    public async Task<BoqEntity> AddAsync(BoqEntity boq, CancellationToken ct)
    {
        _context.Boqs.Add(boq);
        await _context.SaveChangesAsync(ct);
        return boq;
    }

    public async Task<BoqEntity> UpdateAsync(BoqEntity boq, CancellationToken ct)
    {
        _context.Boqs.Update(boq);
        await _context.SaveChangesAsync(ct);
        return boq;
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct)
    {
        var entity = await _context.Boqs.FindAsync(new object[] { id }, ct);
        if (entity is not null)
        {
            _context.Boqs.Remove(entity);
            await _context.SaveChangesAsync(ct);
        }
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken ct)
    {
        return await _context.Boqs.AnyAsync(b => b.Id == id, ct);
    }
}
