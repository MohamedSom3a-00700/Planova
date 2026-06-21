using Planova.Boq.Application.Dto;
using Planova.Boq.Domain.Interfaces;
using Planova.Wbs.Domain.Interfaces;

namespace Planova.Wbs.Application.Services;

using WbsEntity = Planova.Wbs.Domain.Entities.Wbs;
using WbsItemEntity = Planova.Wbs.Domain.Entities.WbsItem;

public sealed class WbsMappingService : IWbsMappingService
{
    private readonly IBoqService _boqService;
    private readonly IWbsRepository _wbsRepository;
    private readonly IWbsItemRepository _itemRepository;
    private readonly IWbsCodeGenerationService _codeGenerationService;

    public WbsMappingService(
        IBoqService boqService,
        IWbsRepository wbsRepository,
        IWbsItemRepository itemRepository,
        IWbsCodeGenerationService codeGenerationService)
    {
        _boqService = boqService;
        _wbsRepository = wbsRepository;
        _itemRepository = itemRepository;
        _codeGenerationService = codeGenerationService;
    }

    public async Task<WbsMappingPreview> PreviewMappingAsync(
        Guid boqId, WbsMappingMethod method, CancellationToken ct)
    {
        var tree = await _boqService.GetTreeAsync(boqId, ct);
        var flat = FlattenTree(tree);

        var nodes = method switch
        {
            WbsMappingMethod.BySection => BuildBySection(tree),
            WbsMappingMethod.ByCsi => BuildByCsi(flat),
            WbsMappingMethod.ByCostCode => BuildByCostCode(flat),
            WbsMappingMethod.ByTrade => BuildByTrade(flat),
            WbsMappingMethod.ByDiscipline => BuildByDiscipline(flat),
            _ => throw new ArgumentOutOfRangeException(nameof(method))
        };

        var depth = CalculateDepth(nodes, 0);
        return new WbsMappingPreview(nodes, nodes.Count, depth);
    }

    public async Task<WbsMappingServiceResult> CreateWbsFromMappingAsync(
        Guid boqId, WbsMappingMethod method, string wbsName, int userId, CancellationToken ct)
    {
        var preview = await PreviewMappingAsync(boqId, method, ct);

        var boq = await _boqService.GetByIdAsync(boqId, ct);

        var wbs = new WbsEntity
        {
            Id = Guid.NewGuid(),
            ProjectId = boq.ProjectId.GetHashCode(),
            Name = wbsName,
            Source = Domain.Enums.WbsSource.Imported,
            Status = Domain.Enums.WbsStatus.Draft,
            SourceBoqId = boqId,
            Revision = 0,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        wbs = await _wbsRepository.AddAsync(wbs, ct);

        var items = new List<WbsItemEntity>();
        FlattenNodes(preview.Nodes, null, items);
        var sortOrder = 0;

        foreach (var item in items)
        {
            item.WbsId = wbs.Id;
            item.SortOrder = sortOrder++;
            await _itemRepository.AddAsync(item, ct);
        }

        await _codeGenerationService.RegenerateCodesAsync(wbs.Id, ct);

        return new WbsMappingServiceResult(wbs.Id, items.Count);
    }

    private static IReadOnlyList<WbsMappingNode> BuildBySection(IReadOnlyList<BoqItemDto> tree)
    {
        return tree
            .Where(i => i.ItemType == Boq.Domain.Enums.ItemType.Section || i.ParentId is null)
            .Select(section => new WbsMappingNode(
                section.Code,
                section.Description.Length > 80 ? section.Description[..80] : section.Description,
                0,
                (section.Children ?? new List<BoqItemDto>())
                    .Select(child => new WbsMappingNode(
                        child.Code,
                        child.Description.Length > 80 ? child.Description[..80] : child.Description,
                        0,
                        Array.Empty<WbsMappingNode>()
                    ))
                    .ToList()
            ))
            .ToList();
    }

    private static IReadOnlyList<WbsMappingNode> BuildByCsi(IReadOnlyList<BoqItemDto> flat)
    {
        return flat
            .GroupBy(i => i.Code.Contains('.') ? i.Code[..i.Code.IndexOf('.')] : "General")
            .Select(g => new WbsMappingNode(
                g.Key,
                $"CSI Section {g.Key}",
                0,
                g.Select(item => new WbsMappingNode(
                    item.Code,
                    item.Description.Length > 80 ? item.Description[..80] : item.Description,
                    0,
                    Array.Empty<WbsMappingNode>()
                )).ToList()
            ))
            .ToList();
    }

    private static IReadOnlyList<WbsMappingNode> BuildByCostCode(IReadOnlyList<BoqItemDto> flat)
    {
        return flat
            .Where(i => !string.IsNullOrWhiteSpace(i.CostCode))
            .GroupBy(i => i.CostCode!)
            .Select(g => new WbsMappingNode(
                g.Key,
                $"Cost Code {g.Key}",
                0,
                g.Select(item => new WbsMappingNode(
                    item.Code,
                    item.Description.Length > 80 ? item.Description[..80] : item.Description,
                    0,
                    Array.Empty<WbsMappingNode>()
                )).ToList()
            ))
            .ToList();
    }

    private static IReadOnlyList<WbsMappingNode> BuildByTrade(IReadOnlyList<BoqItemDto> flat)
    {
        return flat
            .GroupBy(i => i.ItemType.ToString())
            .Select(g => new WbsMappingNode(
                g.Key,
                g.Key,
                0,
                g.Select(item => new WbsMappingNode(
                    item.Code,
                    item.Description.Length > 80 ? item.Description[..80] : item.Description,
                    0,
                    Array.Empty<WbsMappingNode>()
                )).ToList()
            ))
            .ToList();
    }

    private static IReadOnlyList<WbsMappingNode> BuildByDiscipline(IReadOnlyList<BoqItemDto> flat)
    {
        return flat
            .GroupBy(i => i.Level switch { 0 => "Discipline 1", 1 => "Discipline 2", _ => "Discipline 3" })
            .Select(g => new WbsMappingNode(
                g.Key,
                g.Key,
                0,
                g.Select(item => new WbsMappingNode(
                    item.Code,
                    item.Description.Length > 80 ? item.Description[..80] : item.Description,
                    0,
                    Array.Empty<WbsMappingNode>()
                )).ToList()
            ))
            .ToList();
    }

    private static int CalculateDepth(IReadOnlyList<WbsMappingNode> nodes, int currentDepth)
    {
        if (nodes.Count == 0) return currentDepth;
        var maxChildDepth = nodes.Max(n => n.Children.Count > 0
            ? CalculateDepth(n.Children, currentDepth + 1)
            : currentDepth + 1);
        return maxChildDepth;
    }

    private static void FlattenNodes(
        IReadOnlyList<WbsMappingNode> nodes,
        Guid? parentId,
        List<WbsItemEntity> result,
        int level = 0)
    {
        foreach (var node in nodes)
        {
            var item = new WbsItemEntity
            {
                Id = Guid.NewGuid(),
                ParentId = parentId,
                Code = node.Code,
                Name = node.Name,
                Weight = node.Weight,
                Level = level,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            result.Add(item);

            if (node.Children.Count > 0)
                FlattenNodes(node.Children, item.Id, result, level + 1);
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
}
