using Planova.Activity.Application.Dto;
using Planova.Activity.Domain.Entities;
using Planova.Activity.Domain.Enums;
using Planova.Activity.Domain.Interfaces;

namespace Planova.Activity.Application.Services;

public class ActivityRelationshipService : IActivityRelationshipService
{
    private readonly IActivityRelationshipRepository _relationshipRepository;
    private readonly IActivityRepository _activityRepository;
    private readonly CircularReferenceDetector _detector;

    public ActivityRelationshipService(
        IActivityRelationshipRepository relationshipRepository,
        IActivityRepository activityRepository)
    {
        _relationshipRepository = relationshipRepository;
        _activityRepository = activityRepository;
        _detector = new CircularReferenceDetector();
    }

    public async Task<ActivityRelationshipDto> CreateAsync(CreateRelationshipRequest request, CancellationToken ct = default)
    {
        var predecessor = await _activityRepository.GetByIdAsync(request.PredecessorId, ct)
            ?? throw new KeyNotFoundException($"Predecessor activity {request.PredecessorId} not found");
        var successor = await _activityRepository.GetByIdAsync(request.SuccessorId, ct)
            ?? throw new KeyNotFoundException($"Successor activity {request.SuccessorId} not found");

        if (predecessor.ProjectId != successor.ProjectId)
            throw new InvalidOperationException("Activities must belong to the same project");

        var validation = await ValidateNewRelationshipAsync(request.PredecessorId, request.SuccessorId, ct);
        if (validation.HasCycle)
            throw new InvalidOperationException(validation.Message);

        var relationship = new ActivityRelationship
        {
            Id = Guid.NewGuid(),
            ProjectId = request.ProjectId,
            PredecessorId = request.PredecessorId,
            SuccessorId = request.SuccessorId,
            Type = Enum.Parse<RelationshipType>(request.Type),
            LagDays = request.LagDays,
            Description = request.Description,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _relationshipRepository.AddAsync(relationship, ct);
        return MapToDto(relationship);
    }

    public async Task<ActivityRelationshipDto> UpdateAsync(UpdateRelationshipRequest request, CancellationToken ct = default)
    {
        var relationship = await _relationshipRepository.GetByIdAsync(request.Id, ct)
            ?? throw new KeyNotFoundException($"Relationship {request.Id} not found");

        if (request.Type is not null) relationship.Type = Enum.Parse<RelationshipType>(request.Type);
        if (request.LagDays.HasValue) relationship.LagDays = request.LagDays.Value;
        if (request.Description is not null) relationship.Description = request.Description;

        relationship.UpdatedAt = DateTime.UtcNow;
        await _relationshipRepository.UpdateAsync(relationship, ct);
        return MapToDto(relationship);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        await _relationshipRepository.DeleteAsync(id, ct);
    }

    public async Task<List<ActivityRelationshipDto>> GetByProjectAsync(int projectId, CancellationToken ct = default)
    {
        var relationships = await _relationshipRepository.GetByProjectIdAsync(projectId, ct);
        return relationships.Select(MapToDto).ToList();
    }

    public async Task<List<ActivityRelationshipDto>> GetByActivityAsync(Guid activityId, CancellationToken ct = default)
    {
        var relationships = await _relationshipRepository.GetByActivityIdAsync(activityId, ct);
        return relationships.Select(MapToDto).ToList();
    }

    public async Task<CircularReferenceCheckResult> ValidateNewRelationshipAsync(
        Guid predecessorId, Guid successorId, CancellationToken ct = default)
    {
        return await Task.Run(() =>
        {
            return _detector.Detect(predecessorId, successorId,
                async (activityId) =>
                {
                    var rels = await _relationshipRepository.GetByActivityIdAsync(activityId, ct);
                    return rels.Where(r => r.SuccessorId == activityId)
                               .Select(r => r.PredecessorId)
                               .ToList();
                });
        }, ct);
    }

    private static ActivityRelationshipDto MapToDto(ActivityRelationship r) => new()
    {
        Id = r.Id,
        ProjectId = r.ProjectId,
        PredecessorId = r.PredecessorId,
        SuccessorId = r.SuccessorId,
        Type = r.Type.ToString(),
        LagDays = r.LagDays,
        Description = r.Description,
        CreatedAt = r.CreatedAt,
        UpdatedAt = r.UpdatedAt
    };
}
