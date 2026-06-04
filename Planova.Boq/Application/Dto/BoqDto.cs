using Planova.Boq.Domain.Enums;

namespace Planova.Boq.Application.Dto;

public record BoqDto(
    Guid Id,
    Guid ProjectId,
    string Name,
    string? Description,
    string Currency,
    BoqStatus Status,
    int RevisionNumber,
    decimal TotalAmount,
    string? ImportSource,
    int Version,
    DateTime CreatedAt,
    DateTime ModifiedAt
);

public record BoqSummaryDto(
    Guid Id,
    string Name,
    string Currency,
    BoqStatus Status,
    decimal TotalAmount,
    int ItemCount,
    DateTime ModifiedAt
);
