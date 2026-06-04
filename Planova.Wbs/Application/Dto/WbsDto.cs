using Planova.Wbs.Domain.Entities;

namespace Planova.Wbs.Application.Dto;

public record WbsDto(
    Guid Id,
    int ProjectId,
    string Name,
    string? Description,
    int Revision,
    string Status,
    string Source,
    Guid? SourceBoqId,
    decimal TotalWeight,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

public record WbsItemDto(
    Guid Id,
    Guid WbsId,
    Guid? ParentId,
    string Code,
    string ShortCode,
    string Name,
    string? Description,
    int Level,
    int SortOrder,
    string WbsLevel,
    Guid? SourceBoqItemId,
    decimal? Weight,
    DateTime? PlannedStart,
    DateTime? PlannedFinish,
    int? DurationDays,
    string? AssignedTo,
    string? Deliverable,
    string? Notes,
    bool IsActive,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    List<WbsItemDto>? Children = null
);
