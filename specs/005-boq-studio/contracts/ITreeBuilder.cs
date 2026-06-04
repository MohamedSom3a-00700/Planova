// ITreeBuilder — Assembles flat imported rows into a hierarchical BOQ item tree
// Implemented by Planova.Boq.Application.Services

public interface ITreeBuilder
{
    IReadOnlyList<BoqItem> BuildTree(IReadOnlyList<ImportRow> rows, TreeBuildStrategy strategy);
    TreeBuildStrategy DetectStrategy(IReadOnlyList<ImportRow> rows);
}

public enum TreeBuildStrategy
{
    LevelColumn,
    ParentIdColumn,
    CodePrefix
}

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
