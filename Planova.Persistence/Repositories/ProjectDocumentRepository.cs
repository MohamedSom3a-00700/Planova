using Microsoft.EntityFrameworkCore;
using Planova.Application.Repositories;
using Planova.Domain.Entities;
using Planova.Persistence.DbContext;

namespace Planova.Persistence.Repositories;

public class ProjectDocumentRepository : IProjectDocumentRepository
{
    private readonly PlanovaDbContext _context;

    public ProjectDocumentRepository(PlanovaDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<ProjectDocument>> GetByProjectIdAsync(int projectId, CancellationToken ct = default)
    {
        return await _context.ProjectDocuments
            .Where(d => d.ProjectId == projectId)
            .OrderByDescending(d => d.UploadedAt)
            .ToListAsync(ct);
    }

    public async Task<IEnumerable<ProjectDocument>> GetByProjectIdAndTypeAsync(int projectId, string documentType, CancellationToken ct = default)
    {
        return await _context.ProjectDocuments
            .Where(d => d.ProjectId == projectId && d.DocumentType == documentType)
            .OrderByDescending(d => d.UploadedAt)
            .ToListAsync(ct);
    }

    public async Task<ProjectDocument?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        return await _context.ProjectDocuments.FindAsync(new object[] { id }, ct);
    }

    public async Task<ProjectDocument> AddAsync(ProjectDocument document, CancellationToken ct = default)
    {
        _context.ProjectDocuments.Add(document);
        await _context.SaveChangesAsync(ct);
        return document;
    }

    public async Task DeleteAsync(ProjectDocument document, CancellationToken ct = default)
    {
        _context.ProjectDocuments.Remove(document);
        await _context.SaveChangesAsync(ct);
    }

    public async Task<bool> PathExistsAsync(int projectId, string relativePath, CancellationToken ct = default)
    {
        return await _context.ProjectDocuments
            .AnyAsync(d => d.ProjectId == projectId && d.RelativePath == relativePath, ct);
    }

    public async Task DeleteByProjectIdAsync(int projectId, CancellationToken ct = default)
    {
        var docs = await _context.ProjectDocuments
            .Where(d => d.ProjectId == projectId)
            .ToListAsync(ct);
        _context.ProjectDocuments.RemoveRange(docs);
        await _context.SaveChangesAsync(ct);
    }
}
