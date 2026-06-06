using Planova.Domain.Entities;
using Planova.Persistence.DbContext;
using Planova.Shared.Abstractions;

namespace Planova.Persistence.Services;

public class AuditService : IAuditService
{
    private readonly PlanovaDbContext _context;

    public AuditService(PlanovaDbContext context)
    {
        _context = context;
    }

    public async Task LogAsync(string entityType, string entityId, string action,
        int? projectId = null, string? previousState = null,
        string? newState = null, string? changedBy = null,
        CancellationToken ct = default)
    {
        var log = new AuditLog
        {
            Id = Guid.NewGuid(),
            ProjectId = projectId,
            EntityType = entityType,
            EntityId = entityId,
            Action = action,
            PreviousState = previousState,
            NewState = newState,
            ChangedBy = changedBy,
            CreatedAt = DateTime.UtcNow
        };

        _context.Set<AuditLog>().Add(log);
        await _context.SaveChangesAsync(ct);
    }
}
