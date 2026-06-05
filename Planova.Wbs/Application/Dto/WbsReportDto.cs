namespace Planova.Wbs.Application.Dto;

public record WbsSummaryReport(
    Guid WbsId,
    string WbsName,
    int TotalItems,
    int TotalLevels,
    decimal TotalWeight,
    List<ReportSection> Sections
);

public record ReportSection(
    string Name,
    int ItemCount,
    decimal WeightPercentage
);

public record WbsDictionaryReport(
    Guid WbsId,
    string WbsName,
    List<DictionaryEntry> Entries
);

public record DictionaryEntry(
    string Code,
    string ShortCode,
    string Name,
    string? Description,
    string WbsLevel,
    string? AssignedTo,
    string? Deliverable,
    Guid? SourceBoqItemId
);

public enum ReportType
{
    Summary,
    Dictionary
}
