using Planova.Boq.Domain.Enums;

namespace Planova.Boq.Application.Dto;

public record BoqItemDto(
    Guid Id,
    Guid BoqId,
    Guid? ParentId,
    int SortOrder,
    string Code,
    string Description,
    string Unit,
    decimal Quantity,
    decimal Rate,
    decimal Amount,
    ItemType ItemType,
    int Level,
    bool IsActive,
    string? CostCode,
    Guid? ClassificationId,
    decimal? Subtotal,
    List<BoqItemDto> Children
);
