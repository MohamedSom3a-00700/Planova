using Planova.Primavera.Application.Dto;
using Planova.Primavera.Application.Mappings;
using Planova.Primavera.Application.Models;
using Planova.Primavera.Domain.Entities;
using Planova.Primavera.Domain.Enums;
using Planova.Primavera.Domain.Interfaces;
using Planova.Shared.Abstractions;

namespace Planova.Primavera.Application.Services;

public class PrimaveraWorkspaceService : IPrimaveraWorkspaceService
{
    private readonly IPrimaveraWorkspaceRepository _repository;
    private readonly ILoggingService _logger;

    public PrimaveraWorkspaceService(IPrimaveraWorkspaceRepository repository, ILoggingService logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<List<PrimaveraActivityDto>> GetActivitiesAsync(int projectId, CancellationToken ct = default)
    {
        var entities = await _repository.GetActivitiesAsync(projectId, ct);
        return entities.Select(e => e.ToDto()).ToList();
    }

    public async Task<List<PrimaveraRelationshipDto>> GetRelationshipsAsync(int projectId, CancellationToken ct = default)
    {
        var entities = await _repository.GetRelationshipsAsync(projectId, ct);
        return entities.Select(e => e.ToDto()).ToList();
    }

    public async Task<List<PrimaveraResourceAssignmentDto>> GetResourceAssignmentsAsync(int projectId, CancellationToken ct = default)
    {
        var entities = await _repository.GetResourceAssignmentsAsync(projectId, ct);
        return entities.Select(e => e.ToDto()).ToList();
    }

    public async Task<List<PrimaveraCalendarDto>> GetCalendarsAsync(int projectId, CancellationToken ct = default)
    {
        var entities = await _repository.GetCalendarsAsync(projectId, ct);
        return entities.Select(e => e.ToDto()).ToList();
    }

    public async Task<List<PrimaveraCodeDto>> GetCodesAsync(int projectId, CancellationToken ct = default)
    {
        var entities = await _repository.GetCodesAsync(projectId, ct);
        return entities.Select(e => e.ToDto()).ToList();
    }

    public async Task<List<PrimaveraBaselineDto>> GetBaselinesAsync(int projectId, CancellationToken ct = default)
    {
        var entities = await _repository.GetBaselinesAsync(projectId, ct);
        return entities.Select(e => e.ToDto()).ToList();
    }

    public async Task<List<PrimaveraUdfDto>> GetUdfsAsync(int projectId, CancellationToken ct = default)
    {
        var entities = await _repository.GetUdfsAsync(projectId, ct);
        return entities.Select(e => e.ToDto()).ToList();
    }

    public async Task<PrimaveraWorkspaceSnapshot> GetSnapshotAsync(int projectId, CancellationToken ct = default)
    {
        return new PrimaveraWorkspaceSnapshot
        {
            CapturedAt = DateTime.UtcNow,
            Activities = await GetActivitiesAsync(projectId, ct),
            Relationships = await GetRelationshipsAsync(projectId, ct),
            ResourceAssignments = await GetResourceAssignmentsAsync(projectId, ct),
            Calendars = await GetCalendarsAsync(projectId, ct),
            Codes = await GetCodesAsync(projectId, ct),
            Baselines = await GetBaselinesAsync(projectId, ct),
            Udfs = await GetUdfsAsync(projectId, ct)
        };
    }

    public async Task<bool> HasDataAsync(int projectId, CancellationToken ct = default)
    {
        var activities = await _repository.GetActivitiesAsync(projectId, ct);
        return activities.Count > 0;
    }

    public async Task<bool> UpdateActivityAsync(PrimaveraActivityDto dto, CancellationToken ct = default)
    {
        var entity = await _repository.GetActivityByIdAsync(dto.Id, ct);
        if (entity == null) return false;
        entity.Name = dto.Name;
        entity.Status = dto.Status;
        entity.StartDate = dto.StartDate;
        entity.EndDate = dto.EndDate;
        entity.Duration = dto.Duration;
        entity.RemainingDuration = dto.RemainingDuration;
        entity.PercentComplete = dto.PercentComplete;
        entity.SourceType = PrimaveraSourceType.ManualEdit;
        await _repository.UpdateActivityAsync(entity, ct);
        return true;
    }

    public async Task<bool> UpdateRelationshipAsync(PrimaveraRelationshipDto dto, CancellationToken ct = default)
    {
        var entity = await _repository.GetRelationshipByIdAsync(dto.Id, ct);
        if (entity == null) return false;
        entity.Type = dto.Type;
        entity.LagDuration = dto.LagDuration;
        entity.SourceType = PrimaveraSourceType.ManualEdit;
        await _repository.UpdateRelationshipAsync(entity, ct);
        return true;
    }

    public async Task<bool> UpdateResourceAssignmentAsync(PrimaveraResourceAssignmentDto dto, CancellationToken ct = default)
    {
        var entity = await _repository.GetResourceAssignmentByIdAsync(dto.Id, ct);
        if (entity == null) return false;
        entity.Units = dto.Units;
        entity.PlannedUnits = dto.PlannedUnits;
        entity.ActualUnits = dto.ActualUnits;
        entity.CostPerUnit = dto.CostPerUnit;
        entity.SourceType = PrimaveraSourceType.ManualEdit;
        await _repository.UpdateResourceAssignmentAsync(entity, ct);
        return true;
    }

    public async Task<bool> UpdateCalendarAsync(PrimaveraCalendarDto dto, CancellationToken ct = default)
    {
        var entity = await _repository.GetCalendarByIdAsync(dto.Id, ct);
        if (entity == null) return false;
        entity.Name = dto.Name;
        entity.IsBaseCalendar = dto.IsBaseCalendar;
        entity.SourceType = PrimaveraSourceType.ManualEdit;
        await _repository.UpdateCalendarAsync(entity, ct);
        return true;
    }

    public async Task<bool> UpdateCodeAsync(PrimaveraCodeDto dto, CancellationToken ct = default)
    {
        var entity = await _repository.GetCodeByIdAsync(dto.Id, ct);
        if (entity == null) return false;
        entity.CodeValue = dto.CodeValue;
        entity.CodeName = dto.CodeName;
        entity.SourceType = PrimaveraSourceType.ManualEdit;
        await _repository.UpdateCodeAsync(entity, ct);
        return true;
    }

    public async Task<bool> UpdateBaselineAsync(PrimaveraBaselineDto dto, CancellationToken ct = default)
    {
        var entity = await _repository.GetBaselineByIdAsync(dto.Id, ct);
        if (entity == null) return false;
        entity.Name = dto.Name;
        entity.IsActive = dto.IsActive;
        entity.SourceType = PrimaveraSourceType.ManualEdit;
        await _repository.UpdateBaselineAsync(entity, ct);
        return true;
    }

    public async Task<bool> UpdateUdfAsync(PrimaveraUdfDto dto, CancellationToken ct = default)
    {
        var entity = await _repository.GetUdfByIdAsync(dto.Id, ct);
        if (entity == null) return false;
        entity.FieldName = dto.FieldName;
        entity.FieldType = dto.FieldType;
        entity.SourceType = PrimaveraSourceType.ManualEdit;
        await _repository.UpdateUdfAsync(entity, ct);
        return true;
    }

    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        var count = await _repository.SaveChangesAsync(ct);
        _logger.Info("Workspace batch save: {Count} entities updated at {Time}", count, DateTime.UtcNow);
        return count;
    }
}
