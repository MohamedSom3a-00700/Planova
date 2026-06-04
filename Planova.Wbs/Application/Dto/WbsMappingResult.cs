namespace Planova.Wbs.Application.Dto;

public record WbsMappingResult(
    List<MappedItem> Items,
    string Strategy
);

public record MappedItem(
    Guid? TargetId,
    Guid SourceBoqItemId,
    Guid? ParentTargetId,
    string Name,
    int Level,
    int SortOrder,
    string WbsLevel
);
