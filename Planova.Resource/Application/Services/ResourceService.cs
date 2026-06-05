using Planova.Resource.Application.Dto;
using Planova.Resource.Application.Mappings;
using Planova.Resource.Domain.Enums;
using Planova.Resource.Domain.Interfaces;

namespace Planova.Resource.Application.Services;

public class ResourceService : IResourceService
{
    private readonly IResourceRepository _repository;
    private readonly IResourceRateRepository _rateRepository;
    private readonly ICrewResourceRepository _crewResourceRepository;
    private readonly IResourceAssignmentRepository _assignmentRepository;

    public ResourceService(
        IResourceRepository repository,
        IResourceRateRepository rateRepository,
        ICrewResourceRepository crewResourceRepository,
        IResourceAssignmentRepository assignmentRepository)
    {
        _repository = repository;
        _rateRepository = rateRepository;
        _crewResourceRepository = crewResourceRepository;
        _assignmentRepository = assignmentRepository;
    }

    public async Task<ResourceDto> CreateAsync(CreateResourceRequest request, CancellationToken ct = default)
    {
        if (request.Scope == ResourceScope.Project && !request.ProjectId.HasValue)
            throw new ArgumentException("Project-scoped resources must have a project selected.");

        var code = await _repository.GenerateNextCodeAsync(request.ResourceType, ct);
        var entity = request.ToEntity();
        entity.Code = code;

        var duplicateCheck = await _repository.HasDuplicateNameAsync(request.Name, request.Scope, request.ProjectId, null, ct);

        await _repository.AddAsync(entity, ct);
        var dto = entity.ToDto();
        return dto;
    }

    public async Task<ResourceDto> UpdateAsync(UpdateResourceRequest request, CancellationToken ct = default)
    {
        var entity = await _repository.GetByIdAsync(request.Id, ct);
        if (entity is null)
            throw new KeyNotFoundException($"Resource with Id {request.Id} not found.");

        entity.Name = request.Name;
        entity.DefaultRate = request.DefaultRate;
        entity.UnitOfMeasure = request.UnitOfMeasure;
        entity.MaxQuantity = request.MaxQuantity;
        entity.Currency = request.Currency;
        entity.Description = request.Description;
        entity.Trade = request.Trade;
        entity.SkillLevel = request.SkillLevel;
        entity.EquipmentType = request.EquipmentType;
        entity.Capacity = request.Capacity;
        entity.OperatingCost = request.OperatingCost;
        entity.UnitPrice = request.UnitPrice;
        entity.WastagePercent = request.WastagePercent;
        entity.Company = request.Company;
        entity.ContractValue = request.ContractValue;
        entity.ContactName = request.ContactName;
        entity.ContactPhone = request.ContactPhone;
        entity.UpdatedAt = DateTime.UtcNow;

        await _repository.UpdateAsync(entity, ct);
        return entity.ToDto();
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _repository.GetByIdAsync(id, ct);
        if (entity is null)
            throw new KeyNotFoundException($"Resource with Id {id} not found.");

        var crews = await _crewResourceRepository.GetByResourceAsync(id, ct);
        var assignments = await _assignmentRepository.GetByResourceAsync(id, ct);

        var references = new List<string>();
        if (crews.Count > 0)
            references.Add($"crew templates ({crews.Count} reference(s))");
        if (assignments.Count > 0)
            references.Add($"resource assignments ({assignments.Count} reference(s))");

        if (references.Count > 0)
        {
            var message = $"Resource '{entity.Name}' cannot be deleted because it is referenced by {string.Join(" and ", references)}. Deactivate it instead.";
            throw new InvalidOperationException(message);
        }

        await _repository.DeleteAsync(id, ct);
    }

    public async Task DeactivateAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _repository.GetByIdAsync(id, ct);
        if (entity is null)
            throw new KeyNotFoundException($"Resource with Id {id} not found.");
        entity.Status = ResourceStatus.Inactive;
        entity.UpdatedAt = DateTime.UtcNow;
        await _repository.UpdateAsync(entity, ct);
    }

    public async Task ReactivateAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _repository.GetByIdAsync(id, ct);
        if (entity is null)
            throw new KeyNotFoundException($"Resource with Id {id} not found.");
        entity.Status = ResourceStatus.Active;
        entity.UpdatedAt = DateTime.UtcNow;
        await _repository.UpdateAsync(entity, ct);
    }

    public async Task<ResourceDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _repository.GetByIdAsync(id, ct);
        if (entity is null) return null;

        var effectiveRate = await _rateRepository.GetEffectiveRateAsync(id, DateTime.UtcNow, ct);
        return entity.ToDto(effectiveRate?.Rate);
    }

    public async Task<List<ResourceDto>> GetByProjectAsync(int projectId, CancellationToken ct = default)
    {
        var entities = await _repository.GetByProjectAsync(projectId, null, null, ct);
        return entities.Select(e => e.ToDto()).ToList();
    }

    public async Task<List<ResourceDto>> SearchAsync(ResourceFilter filter, CancellationToken ct = default)
    {
        var entities = await _repository.SearchAsync(
            filter.SearchQuery ?? string.Empty,
            filter.Type,
            filter.Scope,
            filter.ProjectId,
            ct);
        return entities.Select(e => e.ToDto()).ToList();
    }

    public async Task<ResourceDuplicateCheckResult> CheckDuplicateNameAsync(string name, ResourceScope scope, int? projectId, Guid? excludeId = null, CancellationToken ct = default)
    {
        var hasDuplicate = await _repository.HasDuplicateNameAsync(name, scope, projectId, excludeId, ct);
        return new ResourceDuplicateCheckResult
        {
            HasDuplicate = hasDuplicate,
            WarningMessage = hasDuplicate ? "A resource with this name already exists in this scope." : null
        };
    }
}
