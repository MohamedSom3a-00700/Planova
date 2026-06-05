namespace Planova.Boq.Application.Dto;

public record ImportRow(
    string Code,
    string Description,
    string Unit,
    decimal Quantity,
    decimal Rate,
    int? Level,
    string? ParentId,
    string? ParentCode,
    IReadOnlyDictionary<string, object> RawValues
);

public enum TreeBuildStrategy
{
    LevelColumn,
    ParentIdColumn,
    CodePrefix
}
