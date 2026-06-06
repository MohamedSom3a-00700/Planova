namespace Planova.Shared.Abstractions;

public interface IAuditService
{
    Task LogAsync(string entityType, string entityId, string action,
        int? projectId = null, string? previousState = null,
        string? newState = null, string? changedBy = null,
        CancellationToken ct = default);
}
