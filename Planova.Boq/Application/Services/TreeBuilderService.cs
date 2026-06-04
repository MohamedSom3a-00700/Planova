using Planova.Boq.Application.Dto;
using Planova.Boq.Domain.Entities;
using Planova.Boq.Domain.Enums;
using Planova.Boq.Domain.Interfaces;

namespace Planova.Boq.Application.Services;

public class TreeBuilderService : ITreeBuilder
{
    public TreeBuildStrategy DetectStrategy(IReadOnlyList<ImportRow> rows)
    {
        if (rows.Count == 0) return TreeBuildStrategy.LevelColumn;

        if (rows.Any(r => r.Level.HasValue))
            return TreeBuildStrategy.LevelColumn;

        if (rows.Any(r => !string.IsNullOrEmpty(r.ParentId)))
            return TreeBuildStrategy.ParentIdColumn;

        return TreeBuildStrategy.CodePrefix;
    }

    public IReadOnlyList<BoqItem> BuildTree(IReadOnlyList<ImportRow> rows, TreeBuildStrategy strategy)
    {
        return strategy switch
        {
            TreeBuildStrategy.LevelColumn => BuildFromLevelColumn(rows),
            TreeBuildStrategy.ParentIdColumn => BuildFromParentId(rows),
            TreeBuildStrategy.CodePrefix => BuildFromCodePrefix(rows),
            _ => BuildFromLevelColumn(rows)
        };
    }

    private static List<BoqItem> BuildFromLevelColumn(IReadOnlyList<ImportRow> rows)
    {
        var items = new List<BoqItem>();
        var stack = new Stack<(int Level, BoqItem Item)>();

        foreach (var row in rows)
        {
            var level = row.Level ?? 0;
            var item = MapToItem(row, level);

            while (stack.Count > 0 && stack.Peek().Level >= level)
            {
                stack.Pop();
            }

            if (stack.TryPeek(out var parent))
            {
                item.ParentId = parent.Item.Id;
            }

            item.ItemType = DetermineItemType(level, row);
            items.Add(item);
            stack.Push((level, item));
        }

        return items;
    }

    private static List<BoqItem> BuildFromParentId(IReadOnlyList<ImportRow> rows)
    {
        var items = rows.Select(r => MapToItem(r, 0)).ToList();
        var lookup = items.ToDictionary(i => i.Code, i => i);

        foreach (var item in items)
        {
            var row = rows.FirstOrDefault(r => r.Code == item.Code);
            if (row == null) continue;

            if (!string.IsNullOrEmpty(row.ParentId) && lookup.TryGetValue(row.ParentId, out var parent))
            {
                item.ParentId = parent.Id;
                item.Level = parent.Level + 1;
            }
        }

        return items;
    }

    private static List<BoqItem> BuildFromCodePrefix(IReadOnlyList<ImportRow> rows)
    {
        var items = new List<BoqItem>();
        var byCode = new Dictionary<string, BoqItem>();

        foreach (var row in rows)
        {
            var segments = row.Code.Split('.');
            var level = segments.Length - 1;
            var item = MapToItem(row, level);
            item.ItemType = DetermineItemType(level, row);
            items.Add(item);
            byCode[row.Code] = item;
        }

        foreach (var item in items)
        {
            var row = rows.FirstOrDefault(r => r.Code == item.Code);
            if (row == null) continue;

            var segments = row.Code.Split('.');
            if (segments.Length > 1)
            {
                var parentCode = string.Join(".", segments.Take(segments.Length - 1));
                if (byCode.TryGetValue(parentCode, out var parent))
                {
                    item.ParentId = parent.Id;
                }
            }
        }

        return items;
    }

    private static BoqItem MapToItem(ImportRow row, int level)
    {
        return new BoqItem
        {
            Id = Guid.NewGuid(),
            Code = row.Code,
            Description = row.Description,
            Unit = row.Unit,
            Quantity = row.Quantity,
            Rate = row.Rate,
            Amount = row.Quantity * row.Rate,
            Level = level,
            SortOrder = 0,
            IsActive = true,
        };
    }

    private static ItemType DetermineItemType(int level, ImportRow row)
    {
        if (level == 0) return ItemType.Section;
        if (row.Quantity == 0 && row.Rate == 0) return ItemType.Section;
        return ItemType.Item;
    }
}
