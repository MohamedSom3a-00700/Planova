using Planova.Boq.Domain.Enums;

namespace Planova.Boq.Application.Dto;

public record LibraryDto(
    Guid Id,
    string Name,
    string? Description,
    LibraryType LibraryType,
    DateTime CreatedAt,
    int ItemCount
);

public record LibraryItemDto(
    Guid Id,
    Guid LibraryId,
    string Code,
    string Description,
    string Unit,
    decimal DefaultRate,
    string? Category,
    string? Tags
);
