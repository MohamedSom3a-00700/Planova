namespace Planova.Boq.Application.Dto;

public record BoqItemValidationDto(
    int SortOrder,
    string Code,
    string Description,
    string Unit,
    decimal Quantity,
    decimal Rate,
    decimal Amount,
    string? CostCode,
    string ItemType,
    int Level,
    bool IsActive,
    string? ValidationStatus,
    string? Remarks
);
