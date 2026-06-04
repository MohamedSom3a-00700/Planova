using Planova.Wbs.Domain.Interfaces;

namespace Planova.Wbs.Application.Services;

using WbsEntity = Planova.Wbs.Domain.Entities.Wbs;
using WbsItemEntity = Planova.Wbs.Domain.Entities.WbsItem;

public class WbsService : IWbsService
{
    private readonly IWbsRepository _wbsRepository;
    private readonly IWbsItemRepository _itemRepository;
    private readonly IWbsValidationService _validationService;

    public WbsService(
        IWbsRepository wbsRepository,
        IWbsItemRepository itemRepository,
        IWbsValidationService validationService)
    {
        _wbsRepository = wbsRepository;
        _itemRepository = itemRepository;
        _validationService = validationService;
    }

    public async Task<WbsEntity> CreateAsync(string name, int projectId, WbsSource source, Guid? sourceBoqId, CancellationToken ct)
    {
        var wbs = new WbsEntity
        {
            Id = Guid.NewGuid(),
            ProjectId = projectId,
            Name = name,
            Source = source,
            SourceBoqId = sourceBoqId,
            Status = WbsStatus.Draft,
            Revision = 0,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var errors = await _validationService.ValidateWbsAsync(wbs, ct);
        if (errors.Count > 0)
            throw new InvalidOperationException($"Validation failed: {string.Join("; ", errors.Select(e => e.Message))}");

        return await _wbsRepository.AddAsync(wbs, ct);
    }

    public async Task<WbsEntity> GetByIdAsync(Guid id, CancellationToken ct)
    {
        return await _wbsRepository.GetByIdAsync(id, ct);
    }

    public async Task<IReadOnlyList<WbsEntity>> GetByProjectAsync(int projectId, CancellationToken ct)
    {
        return await _wbsRepository.GetByProjectIdAsync(projectId, ct);
    }

    public async Task<WbsEntity> UpdateAsync(WbsEntity wbs, CancellationToken ct)
    {
        var errors = await _validationService.ValidateWbsAsync(wbs, ct);
        if (errors.Count > 0)
            throw new InvalidOperationException($"Validation failed: {string.Join("; ", errors.Select(e => e.Message))}");

        wbs.UpdatedAt = DateTime.UtcNow;
        return await _wbsRepository.UpdateAsync(wbs, ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct)
    {
        await _wbsRepository.DeleteAsync(id, ct);
    }

    public async Task ChangeStatusAsync(Guid id, WbsStatus newStatus, CancellationToken ct)
    {
        var wbs = await _wbsRepository.GetByIdAsync(id, ct);
        if (!WbsStatusTransitions.CanTransition(wbs.Status, newStatus))
            throw new InvalidOperationException($"Cannot transition from {wbs.Status} to {newStatus}");

        wbs.Status = newStatus;
        wbs.Revision++;
        wbs.UpdatedAt = DateTime.UtcNow;
        await _wbsRepository.UpdateAsync(wbs, ct);
    }

    public static string GenerateNumericCode(IReadOnlyList<WbsItemEntity> siblings, WbsItemEntity item)
    {
        var parent = siblings.FirstOrDefault(i => i.Id == item.ParentId);
        var position = siblings.Count(i => i.ParentId == item.ParentId && i.SortOrder <= item.SortOrder);
        if (parent == null)
            return position.ToString();
        return $"{parent.Code}.{position}";
    }

    public static string GenerateShortCode(string name)
    {
        var letters = new string(name.Where(char.IsLetter).ToArray());
        if (letters.Length >= 3)
            return letters[..3].ToUpperInvariant();
        return letters.ToUpperInvariant().PadRight(3, 'X');
    }

    public async Task<IReadOnlyList<WbsItemEntity>> GetTreeAsync(Guid wbsId, CancellationToken ct)
    {
        return await _itemRepository.GetByWbsIdAsync(wbsId, ct);
    }

    public async Task RedistributeWeightsAsync(Guid wbsId, CancellationToken ct)
    {
        var items = await _itemRepository.GetByWbsIdAsync(wbsId, ct);

        var groups = items.GroupBy(i => i.ParentId);
        foreach (var group in groups)
        {
            var siblings = group.ToList();
            if (siblings.Count == 0) continue;

            var equalShare = Math.Round(100m / siblings.Count, 2);
            var remainder = 100m - (equalShare * siblings.Count);

            for (var i = 0; i < siblings.Count; i++)
                siblings[i].Weight = equalShare + (i == siblings.Count - 1 ? remainder : 0);

            await _itemRepository.UpdateRangeAsync(siblings, ct);
        }
    }
}
