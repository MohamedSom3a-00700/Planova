using Planova.Boq.Application.Dto;
using Planova.Boq.Domain.Entities;
using Planova.Boq.Domain.Enums;
using Planova.Boq.Domain.Interfaces;

namespace Planova.Boq.Application.Services;

public class BoqService : IBoqService
{
    private readonly IBoqRepository _boqRepository;
    private readonly IBoqItemRepository _itemRepository;

    public BoqService(IBoqRepository boqRepository, IBoqItemRepository itemRepository)
    {
        _boqRepository = boqRepository;
        _itemRepository = itemRepository;
    }

    public async Task<BoqDto> GetByIdAsync(Guid id, CancellationToken ct)
    {
        var boq = await _boqRepository.GetByIdAsync(id, ct);
        return MapToDto(boq);
    }

    public async Task<IReadOnlyList<BoqSummaryDto>> GetByProjectIdAsync(Guid projectId, CancellationToken ct)
    {
        var boqs = await _boqRepository.GetByProjectIdAsync(projectId, ct);
        return boqs.Select(b => new BoqSummaryDto(
            b.Id, b.Name, b.Currency, b.Status, b.TotalAmount,
            b.Items?.Count ?? 0, b.ModifiedAt
        )).ToList();
    }

    public async Task<BoqDto> CreateAsync(Guid projectId, string name, string currency, CancellationToken ct)
    {
        var boq = new Domain.Entities.Boq
        {
            Id = Guid.NewGuid(),
            ProjectId = projectId,
            Name = name,
            Currency = currency,
            Status = BoqStatus.Draft,
            CreatedAt = DateTime.UtcNow,
            ModifiedAt = DateTime.UtcNow,
            CreatedBy = "system",
            ModifiedBy = "system",
        };

        var created = await _boqRepository.AddAsync(boq, ct);
        return MapToDto(created);
    }

    public async Task<BoqDto> UpdateAsync(Guid id, string name, string? description, string currency, CancellationToken ct)
    {
        var boq = await _boqRepository.GetByIdAsync(id, ct);
        boq.Name = name;
        boq.Description = description;
        boq.Currency = currency;
        boq.ModifiedAt = DateTime.UtcNow;
        boq.Version++;

        var updated = await _boqRepository.UpdateAsync(boq, ct);
        return MapToDto(updated);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct)
    {
        await _boqRepository.DeleteAsync(id, ct);
    }

    public async Task<IReadOnlyList<BoqItemDto>> GetTreeAsync(Guid boqId, CancellationToken ct)
    {
        var items = await _itemRepository.GetByBoqIdAsync(boqId, ct);
        return BuildTreeDto(items, null);
    }

    public async Task<BoqItemDto> UpdateItemAsync(Guid boqId, Guid itemId, string code, string description,
        string unit, decimal quantity, decimal rate, string? costCode, bool isActive, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(code) || code.Length > 50)
            throw new ArgumentException("Code is required and max 50 characters", nameof(code));
        if (quantity < 0)
            throw new ArgumentException("Quantity must be >= 0", nameof(quantity));
        if (rate < 0)
            throw new ArgumentException("Rate must be >= 0", nameof(rate));

        var item = await _itemRepository.GetByIdAsync(itemId, ct);
        if (item.BoqId != boqId)
            throw new InvalidOperationException("Item does not belong to the specified BOQ");

        item.Code = code;
        item.Description = description;
        item.Unit = unit;
        item.Quantity = quantity;
        item.Rate = rate;
        item.Amount = quantity * rate;
        item.CostCode = costCode;
        item.IsActive = isActive;

        var updated = await _itemRepository.UpdateAsync(item, ct);
        return BuildItemDto(updated, []);
    }

    public async Task<BoqItemDto> AddItemAsync(Guid boqId, Guid? parentId, string code, string description,
        string unit, decimal quantity, decimal rate, string? costCode, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(code) || code.Length > 50)
            throw new ArgumentException("Code is required and max 50 characters", nameof(code));
        if (quantity < 0)
            throw new ArgumentException("Quantity must be >= 0", nameof(quantity));
        if (rate < 0)
            throw new ArgumentException("Rate must be >= 0", nameof(rate));

        var maxSort = await _itemRepository.GetMaxSortOrderAsync(boqId, parentId, ct);
        var level = 0;

        if (parentId.HasValue)
        {
            var parent = await _itemRepository.GetByIdAsync(parentId.Value, ct);
            level = parent.Level + 1;
        }

        var item = new Domain.Entities.BoqItem
        {
            Id = Guid.NewGuid(),
            BoqId = boqId,
            ParentId = parentId,
            Code = code,
            Description = description,
            Unit = unit,
            Quantity = quantity,
            Rate = rate,
            Amount = quantity * rate,
            ItemType = level == 0 ? ItemType.Section : ItemType.Item,
            Level = level,
            SortOrder = maxSort + 1,
            CostCode = costCode,
            IsActive = true,
        };

        var created = await _itemRepository.AddAsync(item, ct);
        var boq = await _boqRepository.GetByIdAsync(boqId, ct);
        boq.ModifiedAt = DateTime.UtcNow;
        boq.Version++;
        await _boqRepository.UpdateAsync(boq, ct);

        return BuildItemDto(created, []);
    }

    public async Task DeleteItemAsync(Guid boqId, Guid itemId, CancellationToken ct)
    {
        var item = await _itemRepository.GetByIdAsync(itemId, ct);
        if (item.BoqId != boqId)
            throw new InvalidOperationException("Item does not belong to the specified BOQ");

        var children = await _itemRepository.GetChildrenAsync(itemId, ct);
        var childIds = children.Select(c => c.Id).ToList();
        childIds.Add(itemId);

        await _itemRepository.DeleteRangeAsync(childIds, ct);

        var boq = await _boqRepository.GetByIdAsync(boqId, ct);
        boq.ModifiedAt = DateTime.UtcNow;
        boq.Version++;
        await _boqRepository.UpdateAsync(boq, ct);
    }

    public async Task ReorderItemAsync(Guid boqId, Guid itemId, int newSortOrder, CancellationToken ct)
    {
        var item = await _itemRepository.GetByIdAsync(itemId, ct);
        if (item.BoqId != boqId)
            throw new InvalidOperationException("Item does not belong to the specified BOQ");

        item.SortOrder = newSortOrder;
        await _itemRepository.UpdateAsync(item, ct);
    }

    public async Task<decimal> ComputeSubtotalAsync(Guid boqId, Guid? parentId, CancellationToken ct)
    {
        var items = await _itemRepository.GetByBoqIdAsync(boqId, ct);
        var descendants = parentId.HasValue
            ? GetDescendants(items, parentId.Value)
            : items.Where(i => i.ParentId == null).ToList();
        return descendants.Sum(i => i.Amount);
    }

    private static List<BoqItemDto> BuildTreeDto(IReadOnlyList<Domain.Entities.BoqItem> items, Guid? parentId)
    {
        return items
            .Where(i => i.ParentId == parentId)
            .OrderBy(i => i.SortOrder)
            .Select(i =>
            {
                var children = BuildTreeDto(items, i.Id);
                var subtotal = children.Sum(c => c.Subtotal ?? c.Amount);
                return new BoqItemDto(
                    i.Id, i.BoqId, i.ParentId, i.SortOrder,
                    i.Code, i.Description, i.Unit, i.Quantity, i.Rate, i.Amount,
                    i.ItemType, i.Level, i.IsActive,
                    i.CostCode, i.ClassificationId,
                    subtotal > 0 ? subtotal : null,
                    children
                );
            })
            .ToList();
    }

    private static List<Domain.Entities.BoqItem> GetDescendants(IReadOnlyList<Domain.Entities.BoqItem> items, Guid parentId)
    {
        var result = new List<Domain.Entities.BoqItem>();
        var children = items.Where(i => i.ParentId == parentId).ToList();
        foreach (var child in children)
        {
            result.Add(child);
            result.AddRange(GetDescendants(items, child.Id));
        }
        return result;
    }

    private static BoqItemDto BuildItemDto(Domain.Entities.BoqItem item, List<BoqItemDto> children) => new(
        item.Id, item.BoqId, item.ParentId, item.SortOrder,
        item.Code, item.Description, item.Unit, item.Quantity, item.Rate, item.Amount,
        item.ItemType, item.Level, item.IsActive,
        item.CostCode, item.ClassificationId,
        null, children
    );

    private static BoqDto MapToDto(Domain.Entities.Boq boq) => new(
        boq.Id, boq.ProjectId, boq.Name, boq.Description,
        boq.Currency, boq.Status, boq.RevisionNumber, boq.TotalAmount,
        boq.ImportSource, boq.Version, boq.CreatedAt, boq.ModifiedAt
    );
}
