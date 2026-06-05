using Planova.Resource.Application.Dto;
using Planova.Resource.Application.Mappings;
using Planova.Resource.Domain.Interfaces;

namespace Planova.Resource.Application.Services;

public class ResourceRateService : IResourceRateService
{
    private readonly IResourceRateRepository _repository;
    private readonly IResourceRepository _resourceRepository;

    public ResourceRateService(IResourceRateRepository repository, IResourceRepository resourceRepository)
    {
        _repository = repository;
        _resourceRepository = resourceRepository;
    }

    public async Task<ResourceRateDto> AddRateAsync(CreateRateRequest request, CancellationToken ct = default)
    {
        var resource = await _resourceRepository.GetByIdAsync(request.ResourceId, ct);
        if (resource is null)
            throw new KeyNotFoundException($"Resource with Id {request.ResourceId} not found.");

        var hasDuplicate = await _repository.HasDuplicateEffectiveDateAsync(request.ResourceId, request.EffectiveDate, null, ct);
        if (hasDuplicate)
            throw new InvalidOperationException("A rate with this effective date already exists for this resource.");

        var entity = new Domain.Entities.ResourceRate
        {
            Id = Guid.NewGuid(),
            ResourceId = request.ResourceId,
            EffectiveDate = request.EffectiveDate,
            Rate = request.Rate,
            Currency = request.Currency,
            UnitOfMeasure = request.UnitOfMeasure,
            IsDefault = request.IsDefault,
            Notes = request.Notes,
            CreatedAt = DateTime.UtcNow
        };

        await _repository.AddAsync(entity, ct);
        return entity.ToDto();
    }

    public async Task<ResourceRateDto> UpdateRateAsync(Guid id, decimal rate, string currency, string unitOfMeasure, bool isDefault, string? notes, CancellationToken ct = default)
    {
        var entity = await _repository.GetByIdAsync(id, ct);
        if (entity is null)
            throw new KeyNotFoundException($"Rate with Id {id} not found.");

        entity.Rate = rate;
        entity.Currency = currency;
        entity.UnitOfMeasure = unitOfMeasure;
        entity.IsDefault = isDefault;
        entity.Notes = notes;

        await _repository.UpdateAsync(entity, ct);
        return entity.ToDto();
    }

    public async Task DeleteRateAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _repository.GetByIdAsync(id, ct);
        if (entity is null)
            throw new KeyNotFoundException($"Rate with Id {id} not found.");

        await _repository.DeleteAsync(id, ct);
    }

    public async Task<List<ResourceRateDto>> GetRateHistoryAsync(Guid resourceId, CancellationToken ct = default)
    {
        var rates = await _repository.GetByResourceAsync(resourceId, ct);
        return rates.Select(r => r.ToDto()).ToList();
    }

    public async Task<ResourceRateDto?> GetEffectiveRateAsync(Guid resourceId, DateTime date, CancellationToken ct = default)
    {
        var rate = await _repository.GetEffectiveRateAsync(resourceId, date, ct);
        return rate?.ToDto();
    }

    public async Task<List<ResourceRateDto>> BulkUpdateRatesAsync(
        List<Guid> resourceIds, decimal newRate, DateTime effectiveDate, string currency, string unitOfMeasure, CancellationToken ct = default)
    {
        var updates = resourceIds.Select(id => (id, newRate)).ToList();
        await _repository.BulkUpdateAsync(updates, effectiveDate, ct);

        var results = new List<ResourceRateDto>();
        foreach (var resourceId in resourceIds)
        {
            var rate = await _repository.GetEffectiveRateAsync(resourceId, effectiveDate, ct);
            if (rate is not null)
                results.Add(rate.ToDto());
        }
        return results;
    }
}
