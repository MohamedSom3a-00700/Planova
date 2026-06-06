using Planova.Cost.Application.Dto;
using Planova.Cost.Application.Mappings;
using Planova.Cost.Domain.Entities;
using Planova.Cost.Domain.Enums;
using Planova.Cost.Domain.Interfaces;

namespace Planova.Cost.Application.Services;

public class DirectCostService : IDirectCostService
{
    private readonly IDirectCostRepository _repository;

    public DirectCostService(IDirectCostRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<DirectCostDto>> GetByProjectIdAsync(int projectId, CancellationToken ct = default)
    {
        var costs = await _repository.GetByProjectIdAsync(projectId, ct);
        return costs.Select(c => c.ToDto()).ToList();
    }

    public async Task<List<DirectCostDto>> GetByActivityIdAsync(Guid activityId, CancellationToken ct = default)
    {
        var costs = await _repository.GetByActivityIdAsync(activityId, ct);
        return costs.Select(c => c.ToDto()).ToList();
    }

    public async Task<DirectCostDto> CreateAsync(CreateDirectCostRequest request, CancellationToken ct = default)
    {
        var entity = new DirectCost
        {
            Id = Guid.NewGuid(),
            ProjectId = request.ProjectId,
            ActivityId = request.ActivityId,
            Category = Enum.Parse<DirectCostCategory>(request.Category),
            CustomCategoryName = request.CustomCategoryName,
            Description = request.Description,
            Quantity = request.Quantity,
            UnitOfMeasure = request.UnitOfMeasure,
            UnitRate = request.UnitRate,
            Currency = request.Currency,
            TotalAmount = request.Quantity * request.UnitRate,
            Scope = Enum.Parse<DirectCostScope>(request.Scope),
            IsOrphaned = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _repository.AddAsync(entity, ct);
        return entity.ToDto();
    }

    public async Task<DirectCostDto> UpdateAsync(Guid id, UpdateDirectCostRequest request, CancellationToken ct = default)
    {
        var entity = await _repository.GetByIdAsync(id, ct);
        if (entity == null)
            throw new InvalidOperationException($"DirectCost {id} not found");

        entity.Description = request.Description;
        entity.Quantity = request.Quantity;
        entity.UnitRate = request.UnitRate;
        entity.CustomCategoryName = request.CustomCategoryName;
        entity.TotalAmount = request.Quantity * request.UnitRate;
        entity.UpdatedAt = DateTime.UtcNow;

        await _repository.UpdateAsync(entity, ct);
        return entity.ToDto();
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _repository.GetByIdAsync(id, ct);
        if (entity == null)
            throw new InvalidOperationException($"DirectCost {id} not found");

        await _repository.DeleteAsync(entity, ct);
    }

    public async Task MarkOrphanedByActivityIdAsync(Guid activityId, CancellationToken ct = default)
    {
        await _repository.MarkAsOrphanedByActivityIdAsync(activityId, ct);
    }
}
