namespace Planova.Wbs.Application.Dto;

public record WbsTreeDto(
    Guid Id,
    string Name,
    int TotalItems,
    decimal TotalWeight,
    List<WbsTreeItemDto> Roots
);

public record WbsTreeItemDto(
    Guid Id,
    Guid? ParentId,
    string Code,
    string ShortCode,
    string Name,
    int Level,
    int SortOrder,
    string WbsLevel,
    decimal? Weight,
    string? AssignedTo,
    DateTime? PlannedStart,
    DateTime? PlannedFinish,
    List<WbsTreeItemDto> Children
);
