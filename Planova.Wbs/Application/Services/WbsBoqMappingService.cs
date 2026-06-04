using Planova.Boq.Domain.Interfaces;
using Planova.Wbs.Application.Dto;
using Planova.Wbs.Domain.Interfaces;

namespace Planova.Wbs.Application.Services;

using WbsEntity = Planova.Wbs.Domain.Entities.Wbs;
using WbsItemEntity = Planova.Wbs.Domain.Entities.WbsItem;
using WbsLevelType = Planova.Wbs.Domain.Enums.WbsLevelType;
using BoqItemDto = Planova.Boq.Application.Dto.BoqItemDto;

public class WbsBoqMappingService : IWbsBoqMappingService
{
    private readonly IBoqService _boqService;
    private readonly IWbsRepository _wbsRepository;
    private readonly IWbsItemRepository _itemRepository;

    public WbsBoqMappingService(
        IBoqService boqService,
        IWbsRepository wbsRepository,
        IWbsItemRepository itemRepository)
    {
        _boqService = boqService;
        _wbsRepository = wbsRepository;
        _itemRepository = itemRepository;
    }

    public async Task<WbsMappingResult> MapOneToOneAsync(Guid boqId, CancellationToken ct)
    {
        var boq = await _boqService.GetByIdAsync(boqId, ct);
        var tree = await _boqService.GetTreeAsync(boqId, ct);
        var mapped = new List<MappedItem>();
        var idMap = new Dictionary<Guid, Guid?>();

        FlattenAndMap(tree, null, mapped, idMap);

        return new WbsMappingResult(mapped, "OneToOne");
    }

    public async Task<WbsMappingResult> MapGroupedAsync(Guid boqId, CancellationToken ct)
    {
        var boq = await _boqService.GetByIdAsync(boqId, ct);
        var tree = await _boqService.GetTreeAsync(boqId, ct);
        var mapped = new List<MappedItem>();
        var idMap = new Dictionary<Guid, Guid?>();

        var topLevel = tree
            .Select((item, index) => new { item, index })
            .GroupBy(x => x.item.Code)
            .Select(g => g.First());

        foreach (var group in topLevel)
        {
            var groupTargetId = Guid.NewGuid();
            idMap[group.item.Id] = groupTargetId;

            mapped.Add(new MappedItem(
                groupTargetId,
                group.item.Id,
                null,
                group.item.Description.Length > 100
                    ? group.item.Description[..100]
                    : group.item.Description,
                0, group.index, "Summary"));

            var children = tree.Where(t => t.ParentId == group.item.Id).ToList();
            for (int i = 0; i < children.Count; i++)
            {
                var childTargetId = Guid.NewGuid();
                idMap[children[i].Id] = childTargetId;

                mapped.Add(new MappedItem(
                    childTargetId,
                    children[i].Id,
                    groupTargetId,
                    children[i].Description.Length > 100
                        ? children[i].Description[..100]
                        : children[i].Description,
                    1, i, "ControlAccount"));
            }
        }

        return new WbsMappingResult(mapped, "Grouped");
    }

    public async Task<WbsMappingResult> MapCustomAsync(Guid boqId, IReadOnlyList<ManualMapping> mappings, CancellationToken ct)
    {
        var tree = await _boqService.GetTreeAsync(boqId, ct);
        var allItems = FlattenTree(tree);
        var lookup = allItems.ToDictionary(i => i.Id);

        var mapped = new List<MappedItem>();
        var parentMap = new Dictionary<Guid, Guid?>();

        foreach (var mm in mappings)
        {
            var boqItem = lookup.GetValueOrDefault(mm.BoqItemId);
            if (boqItem == null) continue;

            var targetId = Guid.NewGuid();
            var resolvedParentId = mm.ParentMappingId;
            if (resolvedParentId == null && boqItem.ParentId.HasValue)
                parentMap.TryGetValue(boqItem.ParentId.Value, out resolvedParentId);
            parentMap[mm.BoqItemId] = targetId;

            mapped.Add(new MappedItem(
                targetId,
                mm.BoqItemId,
                resolvedParentId,
                mm.Name,
                boqItem.Level,
                boqItem.SortOrder,
                MapLevelToType(boqItem.Level)));
        }

        return new WbsMappingResult(mapped, "Custom");
    }

    public async Task<WbsEntity> CommitMappingAsync(WbsMappingResult result, string wbsName, int projectId, CancellationToken ct)
    {
        if (result.Items.Count == 0)
            throw new InvalidOperationException("Cannot commit an empty mapping result");

        var wbs = new WbsEntity
        {
            Id = Guid.NewGuid(),
            ProjectId = projectId,
            Name = wbsName,
            Source = Domain.Enums.WbsSource.FromBOQ,
            Status = Domain.Enums.WbsStatus.Draft,
            Revision = 0,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var saved = await _wbsRepository.AddAsync(wbs, ct);

        var targetIdMap = result.Items
            .Where(i => i.TargetId.HasValue)
            .ToDictionary(i => i.TargetId!.Value, i => i);

        var parentIdMap = new Dictionary<Guid, Guid?>();

        var levelGroups = result.Items
            .GroupBy(i => i.Level)
            .OrderBy(g => g.Key);

        foreach (var group in levelGroups)
        {
            foreach (var mappedItem in group.OrderBy(i => i.SortOrder))
            {
                var parentTarget = mappedItem.ParentTargetId;
                if (parentTarget.HasValue && parentIdMap.TryGetValue(parentTarget.Value, out var resolvedParent))
                {
                    parentTarget = resolvedParent;
                }

                var item = new WbsItemEntity
                {
                    Id = mappedItem.TargetId ?? Guid.NewGuid(),
                    WbsId = saved.Id,
                    ParentId = parentTarget,
                    SourceBoqItemId = mappedItem.SourceBoqItemId,
                    Name = mappedItem.Name,
                    Code = mappedItem.SortOrder.ToString(),
                    ShortCode = GenerateShortCode(mappedItem.Name),
                    Level = mappedItem.Level,
                    SortOrder = mappedItem.SortOrder,
                    WbsLevel = ParseWbsLevel(mappedItem.WbsLevel),
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                if (mappedItem.TargetId.HasValue)
                    parentIdMap[mappedItem.TargetId.Value] = item.Id;

                await _itemRepository.AddAsync(item, ct);
            }
        }

        var allItems = await _itemRepository.GetByWbsIdAsync(saved.Id, ct);
        RecalculateCodes(allItems.ToList());
        await _itemRepository.UpdateRangeAsync(allItems, ct);

        return saved;
    }

    private static void FlattenAndMap(IReadOnlyList<BoqItemDto> items, Guid? parentTargetId,
        List<MappedItem> result, Dictionary<Guid, Guid?> idMap, int level = 0)
    {
        foreach (var item in items.OrderBy(i => i.SortOrder))
        {
            var targetId = Guid.NewGuid();
            idMap[item.Id] = targetId;

            result.Add(new MappedItem(
                targetId,
                item.Id,
                parentTargetId,
                item.Description.Length > 100 ? item.Description[..100] : item.Description,
                level,
                item.SortOrder,
                MapLevelToType(level)));

            if (item.Children?.Count > 0)
            {
                FlattenAndMap(item.Children, targetId, result, idMap, level + 1);
            }
        }
    }

    private static List<BoqItemDto> FlattenTree(IReadOnlyList<BoqItemDto> items)
    {
        var result = new List<BoqItemDto>();
        foreach (var item in items)
        {
            result.Add(item);
            if (item.Children != null)
                result.AddRange(FlattenTree(item.Children));
        }
        return result;
    }

    private static string MapLevelToType(int level) => level switch
    {
        0 => "Summary",
        1 => "ControlAccount",
        2 => "WorkPackage",
        _ => "PlanningPackage"
    };

    private static WbsLevelType ParseWbsLevel(string level) => level switch
    {
        "Summary" => WbsLevelType.Summary,
        "ControlAccount" => WbsLevelType.ControlAccount,
        "WorkPackage" => WbsLevelType.WorkPackage,
        "PlanningPackage" => WbsLevelType.PlanningPackage,
        _ => WbsLevelType.WorkPackage
    };

    private static string GenerateShortCode(string name)
    {
        var letters = new string(name.Where(char.IsLetter).ToArray());
        if (letters.Length >= 3)
            return letters[..3].ToUpperInvariant();
        return letters.ToUpperInvariant().PadRight(3, 'X');
    }

    private static void RecalculateCodes(List<WbsItemEntity> items)
    {
        var roots = items.Where(i => i.ParentId == null).OrderBy(i => i.SortOrder).ToList();
        for (int i = 0; i < roots.Count; i++)
        {
            roots[i].Code = (i + 1).ToString();
            RecalculateChildCodes(items, roots[i], i + 1);
        }
    }

    private static void RecalculateChildCodes(List<WbsItemEntity> items, WbsItemEntity parent, int parentNum)
    {
        var children = items.Where(i => i.ParentId == parent.Id).OrderBy(i => i.SortOrder).ToList();
        for (int i = 0; i < children.Count; i++)
        {
            children[i].Code = $"{parentNum}.{i + 1}";
            RecalculateChildCodes(items, children[i], i + 1);
        }
    }
}
