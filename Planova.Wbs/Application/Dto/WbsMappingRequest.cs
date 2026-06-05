namespace Planova.Wbs.Application.Dto;

public record ManualMapping(
    Guid BoqItemId,
    string Name,
    Guid? ParentMappingId
);
