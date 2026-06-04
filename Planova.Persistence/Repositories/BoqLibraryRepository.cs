using Microsoft.EntityFrameworkCore;
using Planova.Boq.Domain.Entities;
using Planova.Boq.Domain.Enums;
using Planova.Boq.Domain.Interfaces;
using Planova.Persistence.DbContext;

namespace Planova.Persistence.Repositories;

public class BoqLibraryRepository : IBoqLibraryRepository
{
    private readonly PlanovaDbContext _context;

    public BoqLibraryRepository(PlanovaDbContext context)
    {
        _context = context;
    }

    public async Task<BoqLibrary> GetByIdAsync(Guid id, CancellationToken ct)
    {
        return await _context.BoqLibraries
            .Include(l => l.Items)
            .FirstOrDefaultAsync(l => l.Id == id, ct)
            ?? throw new KeyNotFoundException($"BoqLibrary {id} not found");
    }

    public async Task<IReadOnlyList<BoqLibrary>> GetAllAsync(CancellationToken ct)
    {
        return await _context.BoqLibraries
            .OrderBy(l => l.Name)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<BoqLibrary>> GetByTypeAsync(LibraryType type, CancellationToken ct)
    {
        return await _context.BoqLibraries
            .Where(l => l.LibraryType == type)
            .OrderBy(l => l.Name)
            .ToListAsync(ct);
    }

    public async Task<BoqLibrary> AddAsync(BoqLibrary library, CancellationToken ct)
    {
        _context.BoqLibraries.Add(library);
        await _context.SaveChangesAsync(ct);
        return library;
    }

    public async Task<BoqLibrary> UpdateAsync(BoqLibrary library, CancellationToken ct)
    {
        _context.BoqLibraries.Update(library);
        await _context.SaveChangesAsync(ct);
        return library;
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct)
    {
        var entity = await _context.BoqLibraries.FindAsync(new object[] { id }, ct);
        if (entity is not null)
        {
            _context.BoqLibraries.Remove(entity);
            await _context.SaveChangesAsync(ct);
        }
    }
}
