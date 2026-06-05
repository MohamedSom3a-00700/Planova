using Planova.Wbs.Domain.Enums;
using Planova.Wbs.Domain.Interfaces;

namespace Planova.Wbs.Application.Services;

using WbsEntity = Planova.Wbs.Domain.Entities.Wbs;
using WbsItemEntity = Planova.Wbs.Domain.Entities.WbsItem;

public class WbsValidationService : IWbsValidationService
{
    public Task<IReadOnlyList<ValidationError>> ValidateWbsAsync(WbsEntity wbs, CancellationToken ct)
    {
        var errors = new List<ValidationError>();

        if (string.IsNullOrWhiteSpace(wbs.Name))
            errors.Add(new ValidationError(nameof(wbs.Name), "WBS name is required"));
        else if (wbs.Name.Length > 200)
            errors.Add(new ValidationError(nameof(wbs.Name), "WBS name must not exceed 200 characters"));

        return Task.FromResult<IReadOnlyList<ValidationError>>(errors);
    }

    public Task<IReadOnlyList<ValidationError>> ValidateItemAsync(WbsItemEntity item, IEnumerable<WbsItemEntity> siblings, CancellationToken ct)
    {
        var errors = new List<ValidationError>();

        if (string.IsNullOrWhiteSpace(item.Name))
            errors.Add(new ValidationError(nameof(item.Name), "Item name is required"));
        else if (item.Name.Length > 200)
            errors.Add(new ValidationError(nameof(item.Name), "Item name must not exceed 200 characters"));

        if (item.Weight.HasValue)
        {
            var siblingWeightSum = siblings.Where(s => s.Id != item.Id).Sum(s => s.Weight ?? 0);
            if (siblingWeightSum + item.Weight.Value > 100)
                errors.Add(new ValidationError(nameof(item.Weight), "Total weight of siblings cannot exceed 100%"));
        }

        if (item.WbsLevel == WbsLevelType.WorkPackage && siblings.Any(s => s.ParentId == item.Id))
            errors.Add(new ValidationError(nameof(item.WbsLevel), "WorkPackage items cannot have children"));

        if (item.PlannedStart.HasValue && item.PlannedFinish.HasValue && item.PlannedStart > item.PlannedFinish)
            errors.Add(new ValidationError(nameof(item.PlannedFinish), "Finish date must be on or after start date"));

        return Task.FromResult<IReadOnlyList<ValidationError>>(errors);
    }

    public Task<IReadOnlyList<ValidationError>> ValidateTreeAsync(Guid wbsId, CancellationToken ct)
    {
        return Task.FromResult<IReadOnlyList<ValidationError>>(new List<ValidationError>());
    }

    public bool IsCircularReference(Guid itemId, Guid? newParentId, IEnumerable<WbsItemEntity> allItems)
    {
        if (newParentId == null || newParentId == itemId)
            return true;

        var items = allItems.ToDictionary(i => i.Id);
        var current = newParentId.Value;

        while (items.ContainsKey(current))
        {
            if (current == itemId)
                return true;

            var parent = items[current];
            if (parent.ParentId == null)
                break;

            current = parent.ParentId.Value;
        }

        return false;
    }

    public Task<bool> ValidateWeightConsistencyAsync(Guid wbsId, IEnumerable<WbsItemEntity> allItems, CancellationToken ct)
    {
        var groups = allItems.GroupBy(i => i.ParentId);
        foreach (var group in groups)
        {
            var total = group.Sum(i => i.Weight ?? 0);
            if (total > 100)
                return Task.FromResult(false);
        }
        return Task.FromResult(true);
    }
}
