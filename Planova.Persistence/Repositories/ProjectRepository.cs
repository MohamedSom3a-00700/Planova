using Microsoft.EntityFrameworkCore;
using Planova.Application.Repositories;
using Planova.Domain.Entities;
using Planova.Persistence.DbContext;

namespace Planova.Persistence.Repositories;

public class ProjectRepository : IProjectRepository
{
    private readonly PlanovaDbContext _context;

    public ProjectRepository(PlanovaDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Project>> GetAllAsync(CancellationToken ct = default)
    {
        return await _context.Projects
            .Include(p => p.Client)
            .Include(p => p.Contractor)
            .Include(p => p.Subcontractor)
            .OrderByDescending(p => p.UpdatedAt)
            .ToListAsync(ct);
    }

    public async Task<Project?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var project = await _context.Projects
            .Include(p => p.Client)
            .Include(p => p.Contractor)
            .Include(p => p.Subcontractor)
            .FirstOrDefaultAsync(p => p.Id == id, ct);

        if (project != null)
        {
            await _context.Contracts
                .Include(c => c.Client)
                .Where(c => c.ProjectId == id)
                .LoadAsync(ct);
        }

        return project;
    }

    public async Task<Project> AddAsync(Project project, CancellationToken ct = default)
    {
        _context.Projects.Add(project);
        await _context.SaveChangesAsync(ct);
        return project;
    }

    public async Task UpdateAsync(Project project, CancellationToken ct = default)
    {
        _context.Projects.Update(project);
        await _context.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Project project, CancellationToken ct = default)
    {
        _context.Projects.Remove(project);
        await _context.SaveChangesAsync(ct);
    }

    public async Task<IEnumerable<Project>> SearchAsync(string query, CancellationToken ct = default)
    {
        var q = query.ToLower();
        return await _context.Projects
            .Include(p => p.Client)
            .Include(p => p.Contractor)
            .Include(p => p.Subcontractor)
            .Where(p => p.Name.ToLower().Contains(q) || p.Code.ToLower().Contains(q))
            .OrderByDescending(p => p.UpdatedAt)
            .ToListAsync(ct);
    }

    public async Task<IEnumerable<Project>> GetByStatusAsync(string status, CancellationToken ct = default)
    {
        return await _context.Projects
            .Include(p => p.Client)
            .Include(p => p.Contractor)
            .Include(p => p.Subcontractor)
            .Where(p => p.Status == status)
            .OrderByDescending(p => p.UpdatedAt)
            .ToListAsync(ct);
    }

    public async Task<bool> CodeExistsAsync(string code, int? excludeId = null, CancellationToken ct = default)
    {
        return await _context.Projects
            .AnyAsync(p => p.Code == code && (!excludeId.HasValue || p.Id != excludeId.Value), ct);
    }

    public async Task<int> GetCountAsync(CancellationToken ct = default)
    {
        return await _context.Projects.CountAsync(ct);
    }
}
