using Planova.Wbs.Domain.Interfaces;

namespace Planova.Wbs.Application.Services;

using WbsItemEntity = Planova.Wbs.Domain.Entities.WbsItem;

public sealed class WbsCodeGenerationService : IWbsCodeGenerationService
{
    private readonly IWbsItemRepository _itemRepository;

    public WbsCodeGenerationService(IWbsItemRepository itemRepository)
    {
        _itemRepository = itemRepository;
    }

    public async Task RegenerateCodesAsync(Guid wbsId, CancellationToken ct)
    {
        var allItems = await _itemRepository.GetByWbsIdAsync(wbsId, ct);
        var roots = allItems.Where(i => i.ParentId is null)
                            .OrderBy(i => i.SortOrder)
                            .ToList();

        var toUpdate = new List<WbsItemEntity>();
        foreach (var root in roots)
        {
            AssignCodes(root, allItems, root.SortOrder.ToString(), toUpdate);
        }

        if (toUpdate.Count > 0)
            await _itemRepository.UpdateRangeAsync(toUpdate, ct);
    }

    public async Task<bool> ValidateCodesAsync(Guid wbsId, CancellationToken ct)
    {
        var allItems = await _itemRepository.GetByWbsIdAsync(wbsId, ct);
        var roots = allItems.Where(i => i.ParentId is null)
                            .OrderBy(i => i.SortOrder)
                            .ToList();

        for (var i = 0; i < roots.Count; i++)
        {
            var expectedCode = (i + 1).ToString();
            if (roots[i].Code != expectedCode)
                return false;

            if (!ValidateChildrenCodes(roots[i], allItems))
                return false;
        }

        return true;
    }

    private static void AssignCodes(
        WbsItemEntity item,
        IReadOnlyList<WbsItemEntity> allItems,
        string code,
        List<WbsItemEntity> toUpdate)
    {
        if (item.Code != code)
        {
            item.Code = code;
            toUpdate.Add(item);
        }

        var children = allItems.Where(i => i.ParentId == item.Id)
                               .OrderBy(i => i.SortOrder)
                               .ToList();

        for (var i = 0; i < children.Count; i++)
        {
            var childCode = $"{code}.{i + 1}";
            AssignCodes(children[i], allItems, childCode, toUpdate);
        }
    }

    private static bool ValidateChildrenCodes(
        WbsItemEntity parent,
        IReadOnlyList<WbsItemEntity> allItems)
    {
        var children = allItems.Where(i => i.ParentId == parent.Id)
                               .OrderBy(i => i.SortOrder)
                               .ToList();

        for (var i = 0; i < children.Count; i++)
        {
            var expectedCode = $"{parent.Code}.{i + 1}";
            if (children[i].Code != expectedCode)
                return false;

            if (!ValidateChildrenCodes(children[i], allItems))
                return false;
        }

        return true;
    }
}
