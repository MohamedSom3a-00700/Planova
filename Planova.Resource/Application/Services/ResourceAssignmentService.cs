using Planova.Resource.Application.Dto;
using Planova.Resource.Application.Mappings;
using Planova.Resource.Domain.Entities;
using Planova.Resource.Domain.Interfaces;

namespace Planova.Resource.Application.Services;

public class ResourceAssignmentService : IResourceAssignmentService
{
    private readonly IResourceAssignmentRepository _repository;
    private readonly IResourceUsageRepository _usageRepository;
    private readonly IResourceRepository _resourceRepository;

    public ResourceAssignmentService(
        IResourceAssignmentRepository repository,
        IResourceUsageRepository usageRepository,
        IResourceRepository resourceRepository)
    {
        _repository = repository;
        _usageRepository = usageRepository;
        _resourceRepository = resourceRepository;
    }

    public async Task<ResourceAssignmentDto> CreateAsync(CreateAssignmentRequest request, CancellationToken ct = default)
    {
        var resource = await _resourceRepository.GetByIdAsync(request.ResourceId, ct);
        if (resource is null)
            throw new KeyNotFoundException($"Resource with Id {request.ResourceId} not found.");

        var totalCost = ComputeTotalCost(request.Quantity, request.Rate, request.StartDate, request.EndDate);

        var entity = new ResourceAssignment
        {
            Id = Guid.NewGuid(),
            ProjectId = request.ProjectId,
            ActivityId = request.ActivityId,
            ResourceId = request.ResourceId,
            CrewId = request.CrewId,
            Quantity = request.Quantity,
            Rate = request.Rate,
            Currency = request.Currency,
            UnitOfMeasure = request.UnitOfMeasure,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            TotalCost = totalCost,
            DurationDays = request.StartDate.HasValue && request.EndDate.HasValue
                ? (decimal)(request.EndDate.Value - request.StartDate.Value).TotalDays
                : null,
            Notes = request.Notes,
            CreatedAt = DateTime.UtcNow
        };

        await _repository.AddAsync(entity, ct);
        await RegenerateUsageAsync(entity.Id, ct);

        var saved = await _repository.GetByIdAsync(entity.Id, ct);
        return saved!.ToDto();
    }

    public async Task<ResourceAssignmentDto> UpdateAsync(UpdateAssignmentRequest request, CancellationToken ct = default)
    {
        var entity = await _repository.GetByIdAsync(request.Id, ct);
        if (entity is null)
            throw new KeyNotFoundException($"Assignment with Id {request.Id} not found.");

        entity.Quantity = request.Quantity;
        entity.Rate = request.Rate;
        entity.Currency = request.Currency;
        entity.UnitOfMeasure = request.UnitOfMeasure;
        entity.StartDate = request.StartDate;
        entity.EndDate = request.EndDate;
        entity.TotalCost = ComputeTotalCost(request.Quantity, request.Rate, request.StartDate, request.EndDate);
        entity.DurationDays = request.StartDate.HasValue && request.EndDate.HasValue
            ? (decimal)(request.EndDate.Value - request.StartDate.Value).TotalDays
            : null;
        entity.Notes = request.Notes;
        entity.UpdatedAt = DateTime.UtcNow;

        await _repository.UpdateAsync(entity, ct);
        await RegenerateUsageAsync(entity.Id, ct);

        var updated = await _repository.GetByIdAsync(request.Id, ct);
        return updated!.ToDto();
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _repository.GetByIdAsync(id, ct);
        if (entity is null)
            throw new KeyNotFoundException($"Assignment with Id {id} not found.");

        await _usageRepository.DeleteByAssignmentAsync(id, ct);
        await _repository.DeleteAsync(id, ct);
    }

    public async Task<ResourceAssignmentDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _repository.GetByIdAsync(id, ct);
        return entity?.ToDto();
    }

    public async Task<List<ResourceAssignmentDto>> GetByActivityAsync(Guid activityId, CancellationToken ct = default)
    {
        var entities = await _repository.GetByActivityAsync(activityId, ct);
        return entities.Select(e => e.ToDto()).ToList();
    }

    public async Task<List<ResourceAssignmentDto>> GetByProjectAsync(int projectId, CancellationToken ct = default)
    {
        var entities = await _repository.GetByProjectAsync(projectId, ct);
        return entities.Select(e => e.ToDto()).ToList();
    }

    public async Task<decimal> GetActivityTotalCostAsync(Guid activityId, CancellationToken ct = default)
    {
        return await _repository.GetTotalCostForActivityAsync(activityId, ct);
    }

    public async Task<bool> ActivityHasAssignmentsAsync(Guid activityId, CancellationToken ct = default)
    {
        return await _repository.HasAssignmentsForActivityAsync(activityId, ct);
    }

    public async Task CheckActivityDeletionAsync(Guid activityId, CancellationToken ct = default)
    {
        var hasAssignments = await _repository.HasAssignmentsForActivityAsync(activityId, ct);
        if (hasAssignments)
        {
            var assignments = await _repository.GetByActivityAsync(activityId, ct);
            var resourceNames = assignments
                .Select(a => a.Resource?.Name ?? "Unknown")
                .Distinct()
                .ToList();

            var message = $"Cannot delete activity. It has {assignments.Count} resource assignment(s) using: {string.Join(", ", resourceNames)}";
            throw new InvalidOperationException(message);
        }
    }

    private static decimal ComputeTotalCost(decimal quantity, decimal rate, DateTime? startDate, DateTime? endDate)
    {
        if (startDate.HasValue && endDate.HasValue && endDate.Value > startDate.Value)
        {
            var days = (endDate.Value - startDate.Value).Days + 1;
            return quantity * rate * days;
        }
        return quantity * rate;
    }

    private async Task RegenerateUsageAsync(Guid assignmentId, CancellationToken ct)
    {
        try
        {
            await _usageRepository.RegenerateForAssignmentAsync(assignmentId, ct);
        }
        catch
        {
            // Usage regeneration is best-effort
        }
    }
}
