using Planova.Cost.Application.Dto;
using Planova.Cost.Application.Mappings;
using Planova.Cost.Domain.Entities;
using Planova.Cost.Domain.Enums;
using Planova.Cost.Domain.Interfaces;

namespace Planova.Cost.Application.Services;

public class ActualCostService : IActualCostService
{
    private readonly IActualCostRepository _repository;
    private readonly ICostImportService _importService;

    public ActualCostService(
        IActualCostRepository repository,
        ICostImportService importService)
    {
        _repository = repository;
        _importService = importService;
    }

    public async Task<ActualCostDto?> GetByActivityIdAsync(Guid activityId, CancellationToken ct = default)
    {
        var entity = await _repository.GetByActivityIdAsync(activityId, ct);
        return entity?.ToDto();
    }

    public async Task<ActualCostDto> SetAsync(Guid activityId, decimal amount, string currency, CancellationToken ct = default)
    {
        var existing = await _repository.GetByActivityIdAsync(activityId, ct);
        if (existing != null)
        {
            existing.Amount = amount;
            existing.Currency = currency;
            existing.UpdatedAt = DateTime.UtcNow;
            await _repository.UpdateAsync(existing, ct);
            return existing.ToDto();
        }

        var entity = new ActualCost
        {
            Id = Guid.NewGuid(),
            ActivityId = activityId,
            Amount = amount,
            Currency = currency,
            Source = ActualCostSource.Manual,
            EntryDate = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _repository.AddAsync(entity, ct);
        return entity.ToDto();
    }

    public async Task<ImportResultDto> ImportFromExcelAsync(Stream excelStream, int projectId, CancellationToken ct = default)
    {
        return await _importService.ImportActualCostsAsync(excelStream, projectId, ct);
    }

    public async Task<List<ActivityVarianceDto>> GetVarianceByProjectAsync(int projectId, CancellationToken ct = default)
    {
        var actualCosts = await _repository.GetByProjectIdAsync(projectId, ct);
        return actualCosts.Select(a => new ActivityVarianceDto(
            a.ActivityId, string.Empty, string.Empty, 0, a.Amount, -a.Amount, 0))
            .ToList();
    }

    public async Task MarkOrphanedByActivityIdAsync(Guid activityId, CancellationToken ct = default)
    {
        await _repository.MarkAsOrphanedByActivityIdAsync(activityId, ct);
    }
}
