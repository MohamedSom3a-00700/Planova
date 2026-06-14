using Planova.Primavera.Application.Dto;
using Planova.Primavera.Application.Models;

namespace Planova.Primavera.Domain.Interfaces;

public interface IPrimaveraWorkspaceService
{
    Task<List<PrimaveraActivityDto>> GetActivitiesAsync(int projectId, CancellationToken ct = default);
    Task<List<PrimaveraRelationshipDto>> GetRelationshipsAsync(int projectId, CancellationToken ct = default);
    Task<List<PrimaveraResourceAssignmentDto>> GetResourceAssignmentsAsync(int projectId, CancellationToken ct = default);
    Task<List<PrimaveraCalendarDto>> GetCalendarsAsync(int projectId, CancellationToken ct = default);
    Task<List<PrimaveraCodeDto>> GetCodesAsync(int projectId, CancellationToken ct = default);
    Task<List<PrimaveraBaselineDto>> GetBaselinesAsync(int projectId, CancellationToken ct = default);
    Task<List<PrimaveraUdfDto>> GetUdfsAsync(int projectId, CancellationToken ct = default);
    Task<PrimaveraWorkspaceSnapshot> GetSnapshotAsync(int projectId, CancellationToken ct = default);
    Task<bool> HasDataAsync(int projectId, CancellationToken ct = default);

    Task<bool> UpdateActivityAsync(PrimaveraActivityDto dto, CancellationToken ct = default);
    Task<bool> UpdateRelationshipAsync(PrimaveraRelationshipDto dto, CancellationToken ct = default);
    Task<bool> UpdateResourceAssignmentAsync(PrimaveraResourceAssignmentDto dto, CancellationToken ct = default);
    Task<bool> UpdateCalendarAsync(PrimaveraCalendarDto dto, CancellationToken ct = default);
    Task<bool> UpdateCodeAsync(PrimaveraCodeDto dto, CancellationToken ct = default);
    Task<bool> UpdateBaselineAsync(PrimaveraBaselineDto dto, CancellationToken ct = default);
    Task<bool> UpdateUdfAsync(PrimaveraUdfDto dto, CancellationToken ct = default);

    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
