namespace Planova.Wbs.Application.Dto;

public record WbsGenerationResult(
    List<SuggestedItem> Items,
    bool IsAvailable
);

public record SuggestedItem(
    Guid? Id,
    Guid? ParentId,
    string Name,
    string? Description,
    int Level,
    int SortOrder,
    string WbsLevel,
    decimal? Weight,
    List<SuggestedItem> Children
);
