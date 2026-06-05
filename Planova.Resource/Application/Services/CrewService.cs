using Planova.Resource.Application.Dto;
using Planova.Resource.Application.Mappings;
using Planova.Resource.Domain.Entities;
using Planova.Resource.Domain.Enums;
using Planova.Resource.Domain.Interfaces;

namespace Planova.Resource.Application.Services;

public class CrewService : ICrewService
{
    private readonly ICrewRepository _crewRepository;
    private readonly ICrewResourceRepository _crewResourceRepository;
    private readonly IResourceRateRepository _rateRepository;
    private readonly IResourceAssignmentRepository _assignmentRepository;
    private readonly IResourceRepository _resourceRepository;

    public CrewService(
        ICrewRepository crewRepository,
        ICrewResourceRepository crewResourceRepository,
        IResourceRateRepository rateRepository,
        IResourceAssignmentRepository assignmentRepository,
        IResourceRepository resourceRepository)
    {
        _crewRepository = crewRepository;
        _crewResourceRepository = crewResourceRepository;
        _rateRepository = rateRepository;
        _assignmentRepository = assignmentRepository;
        _resourceRepository = resourceRepository;
    }

    public async Task<CrewDto> CreateAsync(CreateCrewRequest request, CancellationToken ct = default)
    {
        if (request.Resources.Count == 0)
            throw new ArgumentException("Crew must have at least one resource.");

        var crew = new Crew
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Description = request.Description,
            ProjectId = request.ProjectId,
            Category = request.Category,
            Status = CrewStatus.Draft,
            CreatedAt = DateTime.UtcNow
        };

        await _crewRepository.AddAsync(crew, ct);

        foreach (var input in request.Resources)
        {
            var resource = await _resourceRepository.GetByIdAsync(input.ResourceId, ct);
            if (resource is null)
                throw new KeyNotFoundException($"Resource with Id {input.ResourceId} not found.");

            var crewResource = new CrewResource
            {
                Id = Guid.NewGuid(),
                CrewId = crew.Id,
                ResourceId = input.ResourceId,
                Quantity = input.Quantity,
                IsLead = input.IsLead,
                SortOrder = 0
            };
            await _crewResourceRepository.AddAsync(crewResource, ct);
        }

        var saved = await _crewRepository.GetByIdAsync(crew.Id, ct);
        var blendedRate = await ComputeBlendedRateAsync(crew.Id, null, ct);
        return saved!.ToDto(blendedRate);
    }

    public async Task<CrewDto> UpdateAsync(UpdateCrewRequest request, CancellationToken ct = default)
    {
        var crew = await _crewRepository.GetByIdAsync(request.Id, ct);
        if (crew is null)
            throw new KeyNotFoundException($"Crew with Id {request.Id} not found.");

        crew.Name = request.Name;
        crew.Description = request.Description;
        crew.Category = request.Category;
        crew.UpdatedAt = DateTime.UtcNow;

        await _crewRepository.UpdateAsync(crew, ct);

        await _crewResourceRepository.DeleteByCrewAsync(request.Id, ct);
        foreach (var input in request.Resources)
        {
            var crewResource = new CrewResource
            {
                Id = Guid.NewGuid(),
                CrewId = request.Id,
                ResourceId = input.ResourceId,
                Quantity = input.Quantity,
                IsLead = input.IsLead,
                SortOrder = 0
            };
            await _crewResourceRepository.AddAsync(crewResource, ct);
        }

        var updated = await _crewRepository.GetByIdAsync(request.Id, ct);
        var blendedRate = await ComputeBlendedRateAsync(request.Id, null, ct);
        return updated!.ToDto(blendedRate);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var crew = await _crewRepository.GetByIdAsync(id, ct);
        if (crew is null)
            throw new KeyNotFoundException($"Crew with Id {id} not found.");

        var assignments = await _assignmentRepository.GetByResourceAsync(id, ct);
        if (assignments.Count > 0)
            throw new InvalidOperationException($"Crew '{crew.Name}' has been applied to {assignments.Count} activities and cannot be deleted.");

        await _crewResourceRepository.DeleteByCrewAsync(id, ct);
        await _crewRepository.DeleteAsync(id, ct);
    }

    public async Task<CrewDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var crew = await _crewRepository.GetByIdAsync(id, ct);
        if (crew is null) return null;

        var blendedRate = await ComputeBlendedRateAsync(id, null, ct);
        return crew.ToDto(blendedRate);
    }

    public async Task<List<CrewDto>> GetAllAsync(int? projectId = null, CancellationToken ct = default)
    {
        var crews = await _crewRepository.GetAllAsync(projectId, null, ct);
        var result = new List<CrewDto>();
        foreach (var crew in crews)
        {
            var blendedRate = await ComputeBlendedRateAsync(crew.Id, null, ct);
            result.Add(crew.ToDto(blendedRate));
        }
        return result;
    }

    public async Task<CrewDto> CloneAsync(Guid id, string newName, CancellationToken ct = default)
    {
        var source = await _crewRepository.GetByIdAsync(id, ct);
        if (source is null)
            throw new KeyNotFoundException($"Crew with Id {id} not found.");

        var clone = new Crew
        {
            Id = Guid.NewGuid(),
            Name = newName,
            Description = source.Description,
            ProjectId = source.ProjectId,
            Category = source.Category,
            Status = CrewStatus.Draft,
            CreatedAt = DateTime.UtcNow
        };

        await _crewRepository.AddAsync(clone, ct);

        var sourceResources = await _crewResourceRepository.GetByCrewAsync(id, ct);
        foreach (var sr in sourceResources)
        {
            var cr = new CrewResource
            {
                Id = Guid.NewGuid(),
                CrewId = clone.Id,
                ResourceId = sr.ResourceId,
                Quantity = sr.Quantity,
                IsLead = sr.IsLead,
                SortOrder = sr.SortOrder
            };
            await _crewResourceRepository.AddAsync(cr, ct);
        }

        var saved = await _crewRepository.GetByIdAsync(clone.Id, ct);
        var blendedRate = await ComputeBlendedRateAsync(clone.Id, null, ct);
        return saved!.ToDto(blendedRate);
    }

    public async Task<decimal> ComputeBlendedRateAsync(Guid crewId, DateTime? rateDate = null, CancellationToken ct = default)
    {
        var resources = await _crewResourceRepository.GetByCrewAsync(crewId, ct);
        var date = rateDate ?? DateTime.UtcNow;
        decimal total = 0;

        foreach (var cr in resources)
        {
            var rate = await _rateRepository.GetEffectiveRateAsync(cr.ResourceId, date, ct);
            var rateValue = rate?.Rate ?? 0;
            total += cr.Quantity * rateValue;
        }

        return total;
    }

    public async Task<List<ResourceAssignmentDto>> ApplyToActivitiesAsync(
        Guid crewId, List<Guid> activityIds, DateTime? startDate = null, DateTime? endDate = null, CancellationToken ct = default)
    {
        var crewResources = await _crewResourceRepository.GetByCrewAsync(crewId, ct);
        if (crewResources.Count == 0)
            throw new InvalidOperationException("Crew has no resources to apply.");

        var crew = await _crewRepository.GetByIdAsync(crewId, ct);
        if (crew is null)
            throw new KeyNotFoundException($"Crew with Id {crewId} not found.");

        var assignments = new List<ResourceAssignment>();

        foreach (var activityId in activityIds)
        {
            foreach (var cr in crewResources)
            {
                var rate = await _rateRepository.GetEffectiveRateAsync(cr.ResourceId, DateTime.UtcNow, ct);
                var rateValue = rate?.Rate ?? 0;

                var assignment = new ResourceAssignment
                {
                    Id = Guid.NewGuid(),
                    ProjectId = crew.ProjectId ?? 0,
                    ActivityId = activityId,
                    ResourceId = cr.ResourceId,
                    CrewId = crewId,
                    Quantity = cr.Quantity,
                    Rate = rateValue,
                    Currency = "USD",
                    UnitOfMeasure = "hr",
                    StartDate = startDate,
                    EndDate = endDate,
                    TotalCost = cr.Quantity * rateValue,
                    Notes = $"Applied from crew '{crew.Name}'",
                    CreatedAt = DateTime.UtcNow
                };

                assignments.Add(assignment);
            }
        }

        await _assignmentRepository.AddRangeAsync(assignments, ct);
        return assignments.Select(a => a.ToDto()).ToList();
    }

    public async Task AddResourceToCrewAsync(Guid crewId, Guid resourceId, decimal quantity, bool isLead, CancellationToken ct = default)
    {
        var crew = await _crewRepository.GetByIdAsync(crewId, ct);
        if (crew is null)
            throw new KeyNotFoundException($"Crew with Id {crewId} not found.");

        var resource = await _resourceRepository.GetByIdAsync(resourceId, ct);
        if (resource is null)
            throw new KeyNotFoundException($"Resource with Id {resourceId} not found.");

        var existing = await _crewResourceRepository.GetByCrewAsync(crewId, ct);
        if (existing.Any(r => r.ResourceId == resourceId))
            throw new InvalidOperationException("Resource is already part of this crew.");

        var crewResource = new CrewResource
        {
            Id = Guid.NewGuid(),
            CrewId = crewId,
            ResourceId = resourceId,
            Quantity = quantity,
            IsLead = isLead,
            SortOrder = existing.Count
        };

        await _crewResourceRepository.AddAsync(crewResource, ct);
    }

    public async Task RemoveResourceFromCrewAsync(Guid crewResourceId, CancellationToken ct = default)
    {
        var cr = await _crewResourceRepository.GetByIdAsync(crewResourceId, ct);
        if (cr is null)
            throw new KeyNotFoundException($"CrewResource with Id {crewResourceId} not found.");

        await _crewResourceRepository.DeleteAsync(crewResourceId, ct);
    }

    public async Task UpdateCrewResourceQuantityAsync(Guid crewResourceId, decimal quantity, CancellationToken ct = default)
    {
        var cr = await _crewResourceRepository.GetByIdAsync(crewResourceId, ct);
        if (cr is null)
            throw new KeyNotFoundException($"CrewResource with Id {crewResourceId} not found.");

        cr.Quantity = quantity;
        await _crewResourceRepository.UpdateAsync(cr, ct);
    }
}
